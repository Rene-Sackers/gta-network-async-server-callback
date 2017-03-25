using System;
using GTANetworkServer;

namespace GTANAsyncServerCallback.GTA.resources.asyncsc.Server
{
	public class Main : Script
	{
		private readonly AsyncServerCallback _asyncServerCallback;

		public Main()
		{
			API.onPlayerFinishedDownload += OnPlayerFinishedDownload;

			_asyncServerCallback = new AsyncServerCallback(API);

			_asyncServerCallback.RegisterEventHandler("server-test", ServerTestMethodHandler);
		}

		private static object ServerTestMethodHandler(object[] argument)
		{
			return argument[0] + "to that!";
		}

		private async void OnPlayerFinishedDownload(Client player)
		{
			const string stringToSend = "Add this: ";

			API.consoleOutput($"Sending: {stringToSend}");

			var awaitedResponse = await _asyncServerCallback.GetFromClient(player, "client-test", stringToSend);

			API.consoleOutput($"Response: {awaitedResponse[0]}");
		}
	}
}