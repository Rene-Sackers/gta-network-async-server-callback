using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GTANetworkServer;

namespace GTANAsyncServerCallback.GTA.resources.asyncsc.Server
{
	public class AsyncServerCallback
	{
		private const string ServerRequestEventName = "GetFromServerRequest-";
		private const string ServerResponseEventName = "GetFromServerResponse-";
		private const string ClientRequestEventName = "GetFromClientRequest-";
		private const string ClientResponseEventName = "GetFromClientResponse-";
		private const int ClientResponseTimeout = 1000;
		private const int ClientResponseWaitStep = 5;

		private static readonly Random Random = new Random();

		private readonly Dictionary<string, Func<object[], object>> _eventHandlers = new Dictionary<string, Func<object[], object>>();
		private readonly API _apiInstance;

		public AsyncServerCallback(API api)
		{
			_apiInstance = api;
			_apiInstance.onClientEventTrigger += OnClientEventTrigger;
		}
		
		public void RegisterEventHandler(string methodName, Func<object[], object> handler)
		{
			methodName = SanitizeMethodName(methodName);

			if (_eventHandlers.ContainsKey(methodName))
				throw new InvalidOperationException($"Event handler already registered for event name {methodName}");

			_eventHandlers.Add(methodName, handler);
		}

		public void UnregisterEventHandler(string methodName)
		{
			methodName = SanitizeMethodName(methodName);

			if (!_eventHandlers.ContainsKey(methodName))
				throw new InvalidOperationException($"Event handler not yet registered for event name {methodName}");

			_eventHandlers.Remove(methodName);
		}

		public Task<object[]> GetFromClient(Client client, string methodName, params object[] arguments)
		{
			methodName = SanitizeMethodName(methodName);

			var uniqueId = CreateUid();

			return Task.Run(async () =>
			{
				object[] result = null;
				var waitTime = 0;

				_apiInstance.onClientEventTrigger += (sender, eventName, args) =>
				{
					if (sender != client || eventName != ClientResponseEventName + uniqueId) return;

					result = args;
				};

				_apiInstance.triggerClientEvent(client, $"{ClientRequestEventName}{methodName}-{uniqueId}", arguments);

				while (result == null && waitTime < ClientResponseTimeout)
				{
					await Task.Delay(ClientResponseWaitStep);
					waitTime += ClientResponseWaitStep;
				}

				return result;
			});
		}

		private static string CreateUid()
		{
			const int length = 4;
			var buffer = new byte[length / 2];
			Random.NextBytes(buffer);
			return string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
		}

		private static void ReplyToClientWithResponse(Client sender, string requestUid, object response)
		{
			API.shared.triggerClientEvent(sender, $"{ServerResponseEventName}{requestUid}", response);
		}

		private static string SanitizeMethodName(string methodName)
		{
			return Regex.Replace(methodName, "[^a-zA-Z0-9]+", "_");
		}

		private void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
		{
			if (!eventName.StartsWith(ServerRequestEventName))
				return;

			var splitEventName = eventName.Replace(ServerRequestEventName, "").Split('-');

			if (splitEventName.Length != 2)
				throw new InvalidOperationException($"Invalid event name: {eventName}");

			var requestEventName = splitEventName[0];
			var requestUid = splitEventName[1];

			var response = ExecuteEvent(requestEventName, arguments);
			ReplyToClientWithResponse(sender, requestUid, response);
		}

		private object ExecuteEvent(string requestEventName, object[] requestArguments)
		{
			if (!_eventHandlers.ContainsKey(requestEventName))
				throw new InvalidOperationException($"Tried to answer client async server callback with event name {requestEventName}, but a handler has not been registered for it.");

			return _eventHandlers[requestEventName].Invoke(requestArguments);
		}
	}
}