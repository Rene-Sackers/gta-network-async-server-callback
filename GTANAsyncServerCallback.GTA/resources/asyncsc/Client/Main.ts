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
		const stringToSend = "Add this: ";

		API.sendChatMessage(`Sending: ${stringToSend}`);

		const awaitedResponse = await this.asyncServerCallback.getFromServer("server-test", stringToSend);

		API.sendChatMessage(`Response: ${awaitedResponse[0]}`);
	}

	clientTestMethodHandler = (argumentss: System.Array<any>) => {
		return [argumentss[0] + "to that!"];
	}
}

API.onResourceStart.connect(() => {
	const main = new Main();
});