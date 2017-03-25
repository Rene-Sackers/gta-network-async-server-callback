using System;
using GTANetworkServer;

namespace GTANAsyncServerCallback.GTA.resources.asyncsc.Server
{
	public class Main : Script
	{
		public Main()
		{
			AsyncServerCallback.RegisterEventHandler("test", TestAsyncCallbackHandler);
		}

		private object TestAsyncCallbackHandler(object[] argument)
		{
			return argument[0] + " to that!";
		}
	}
}