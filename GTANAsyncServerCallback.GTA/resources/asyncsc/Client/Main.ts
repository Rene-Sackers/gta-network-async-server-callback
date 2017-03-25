/// <reference path="../../../types-gtanetwork/index.d.ts" />
class Main {
	private asyncServerCallback = new AsyncServerCallback();

	constructor() {
		API.onUpdate.connect(this.onUpdate);

		this.asyncServerCallback.registerEventHandler("client-test", this.clientTestMethodHandler);
	}

	private onUpdate = () => {
		if (API.isControlJustPressed(Enums.Controls.Context)) {
			this.getFromServerAsync();
		}
	}

	private getFromServerAsync = async () => {
		const awaitedResponse = await this.asyncServerCallback.getFromServer("server-test", "A", "B", "C");

		API.sendChatMessage(`Response: ${awaitedResponse[0]}, ${awaitedResponse[1]}`);
	}

	clientTestMethodHandler = (argumentss: System.Array<any>) => {
		return [argumentss[0] + " " + argumentss[1], argumentss[2]];
	}
}

API.onResourceStart.connect(() => {
	const main = new Main();
});