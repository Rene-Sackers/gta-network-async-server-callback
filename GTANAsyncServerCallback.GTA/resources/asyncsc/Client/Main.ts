/// <reference path="../../../types-gtanetwork/index.d.ts" />
class Main {
	constructor() {
		//var list = new List(String);
		//list.Add("test");

		API.onUpdate.connect(this.onUpdate);
	}

	private onUpdate = () => {
		if (API.isControlJustPressed(Enums.Controls.Context)) {
			this.getFromServerAsync();
		}
	}

	private getFromServerAsync = async () => {
		const stringToSend = "Add this: ";

		API.sendChatMessage(`Sending: ${stringToSend}`);

		const awaitedResponse = await AsyncServerCallback.getFromServer("test", stringToSend);

		API.sendChatMessage(`Response: ${awaitedResponse[0]}`);
	}
}

API.onResourceStart.connect(() => {
	const main = new Main();
});