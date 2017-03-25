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

		private static object[] ServerTestMethodHandler(object[] arguments)
		{
			return new[] {arguments[0] + " " + arguments[1], arguments[2]};
		}

		private async void OnPlayerFinishedDownload(Client player)
		{
			var awaitedResponse = await _asyncServerCallback.GetFromClient(player, "client-test", "A", "B", "C");
			
			API.consoleOutput($"Response: {awaitedResponse[0]}, {awaitedResponse[1]}");
		}
	}
}