/// <reference path="../../../types-gtanetwork/index.d.ts" />

class AsyncServerCallback {
	private static requestEventName = "GetFromServerRequest-";
	private static responseEventName = "GetFromServerResponse-";
	private static callbackTimeout = 1000;

	private static createUid() {
		return Math.floor((1 + Math.random()) * 0x10000)
			.toString(16)
			.substring(1);
	}

	private static sanitizeMethodName(methodName: string) {
		return methodName.replace(/[^a-zA-Z0-9]+/g, "_");
	}

	static async getFromServer(method: string, ...args: any[]) {
		const uid = AsyncServerCallback.createUid();
		
		const responsePromise = new Promise<System.Array<any>>((resolve) => {
			var handler = API.onServerEventTrigger.connect((eventName, eventArgs) => {
				if (eventName !== this.responseEventName + uid) return;

				handler.disconnect();
				resolve(eventArgs);
			});
		});
		
		this.shittyMethodArgsInvoke(`${this.requestEventName}${this.sanitizeMethodName(method)}-${uid}`, args);

		return responsePromise;
	}

	// Trust me, it was the ONLY fucking way to do this. Need mor args (wtf is wrong with you)? Add more cases.
	// Think you have a better solution? Tell me. But test it first, because trust me, it won't work.
	private static shittyMethodArgsInvoke(serverEventName: string, args: any[]) {
		switch (args.length) {
			case 0:
				API.triggerServerEvent(serverEventName);
				break;
			case 1:
				API.triggerServerEvent(serverEventName, args[0]);
				break;
			case 2:
				API.triggerServerEvent(serverEventName, args[0], args[1]);
				break;
			case 3:
				API.triggerServerEvent(serverEventName, args[0], args[1], args[2]);
				break;
			case 4:
				API.triggerServerEvent(serverEventName, args[0], args[1], args[2], args[3]);
				break;
			case 5:
				API.triggerServerEvent(serverEventName, args[0], args[1], args[2], args[3], args[4]);
				break;
		}
	}
}