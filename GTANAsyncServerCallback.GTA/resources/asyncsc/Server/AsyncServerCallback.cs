using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GTANetworkServer;

namespace GTANAsyncServerCallback.GTA.resources.asyncsc.Server
{
	public class AsyncServerCallback : Script
	{
		private const string RequestEventName = "GetFromServerRequest-";
		private const string ResponseEventName = "GetFromServerResponse-";

		private static readonly Dictionary<string, Func<object[], object>> EventHandlers = new Dictionary<string, Func<object[], object>>();

		public AsyncServerCallback()
		{
			API.onClientEventTrigger += OnClientEventTrigger;
			API.onResourceStop += OnResourceStop;
		}

		private void OnResourceStop()
		{
			// Due to how resources are restarted. Apparently the class and statics remain, but it re-initializes the constructor. Wtf.
			API.onClientEventTrigger -= OnClientEventTrigger;
			EventHandlers.Clear();
		}

		private static void OnClientEventTrigger(Client sender, string eventName, params object[] arguments)
		{
			if (!eventName.StartsWith(RequestEventName))
				return;

			var splitEventName = eventName.Replace(RequestEventName, "").Split('-');

			if (splitEventName.Length != 2)
				throw new InvalidOperationException($"Invalid event name: {eventName}");

			var requestEventName = splitEventName[0];
			var requestUid = splitEventName[1];

			var response = ExecuteEvent(requestEventName, arguments);
			ReplyToClientWithResponse(sender, requestUid, response);
		}

		private static string SanitizeMethodName(string methodName)
		{
			return Regex.Replace(methodName, "[^a-zA-Z0-9]+", "_");
		}

		public static void RegisterEventHandler(string methodName, Func<object[], object> handler)
		{
			methodName = SanitizeMethodName(methodName);

			if (EventHandlers.ContainsKey(methodName))
				throw new InvalidOperationException($"Event handler already registered for event name {methodName}");

			EventHandlers.Add(methodName, handler);
		}

		public static void UnregisterEventHandler(string methodName)
		{
			if (!EventHandlers.ContainsKey(methodName))
				throw new InvalidOperationException($"Event handler not yet registered for event name {methodName}");

			EventHandlers.Remove(methodName);
		}

		private static object ExecuteEvent(string requestEventName, object[] requestArguments)
		{
			if (!EventHandlers.ContainsKey(requestEventName))
				throw new InvalidOperationException($"Tried to answer client async server callback with event name {requestEventName}, but a handler has not been registered for it.");

			return EventHandlers[requestEventName].Invoke(requestArguments);
		}
		
		private static void ReplyToClientWithResponse(Client sender, string requestUid, object response)
		{
			API.shared.triggerClientEvent(sender, $"{ResponseEventName}{requestUid}", response);
		}
	}
}