/// <reference path="../../../types-gtanetwork/index.d.ts" />

class AsyncServerCallback {
	private static serverRequestEventName = "GetFromServerRequest-";
	private static serverResponseEventName = "GetFromServerResponse-";
	private static clientRequestEventName = "GetFromClientRequest-";
	private static clientResponseEventName = "GetFromClientResponse-";
	private static callbackTimeout = 1000;

	private eventHandlers: { [methodName: string]: (argumentss: System.Array<any>) => any[] } = {};

	constructor() {
		API.onServerEventTrigger.connect(this.onServerEventTrigger);
	}

	registerEventHandler = (methodName: string, handler: (argumentss: System.Array<any>) => any[]) => {
		methodName = AsyncServerCallback.sanitizeMethodName(methodName);

		if (this.eventHandlers.hasOwnProperty(methodName))
			return;

		this.eventHandlers[methodName] = handler;
	}

	unregisterEventHandler = (methodName: string) => {
		methodName = AsyncServerCallback.sanitizeMethodName(methodName);

		if (!this.eventHandlers.hasOwnProperty(methodName))
			return;

		delete this.eventHandlers[methodName];
	}

	async getFromServer(method: string, ...args: any[]) {
		const uid = AsyncServerCallback.createUid();

		const responsePromise = new Promise<System.Array<any>>((resolve) => {
			var handler = API.onServerEventTrigger.connect((eventName, eventArgs) => {
				if (eventName !== AsyncServerCallback.serverResponseEventName + uid) return;

				handler.disconnect();
				resolve(eventArgs);
			});
		});

		AsyncServerCallback.shittyMethodArgsInvoke(`${AsyncServerCallback.serverRequestEventName}${AsyncServerCallback.sanitizeMethodName(method)}-${uid}`, args);

		return responsePromise;
	}

	private static createUid() {
		return Math.floor((1 + Math.random()) * 0x10000)
			.toString(16)
			.substring(1);
	}

	private static replyToServerWithResponse = (requestUid: string, response: any[]) => {
		AsyncServerCallback.shittyMethodArgsInvoke(`${AsyncServerCallback.clientResponseEventName}${requestUid}`, response);
	}

	private static sanitizeMethodName(methodName: string) {
		return methodName.replace(/[^a-zA-Z0-9]+/g, "_");
	}
	
	private onServerEventTrigger = (eventName: string, argumentss: System.Array<any>) => {
		if (eventName.indexOf(AsyncServerCallback.clientRequestEventName) !== 0)
			return;

		var splitEventName = eventName.replace(AsyncServerCallback.clientRequestEventName, "").split("-");

		if (splitEventName.length !== 2)
			return;

		var [requestEventName, requestUid] = splitEventName;

		var response = this.executeEvent(requestEventName, argumentss);
		AsyncServerCallback.replyToServerWithResponse(requestUid, response);
	}

	private executeEvent = (requestEventName: string, requestArguments: System.Array<any>) => {
		if (!this.eventHandlers.hasOwnProperty(requestEventName))
			return null;

		return this.eventHandlers[requestEventName].call(null, requestArguments);
	}

	// Trust me, it was the ONLY fucking way to do this. Need more args (wtf is wrong with you)? Add more cases.
	// Think you have a better solution? Tell me. But test it first, because trust me, it probably won't work.
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