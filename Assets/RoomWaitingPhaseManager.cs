using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomWaitingPhaseManager : MonoBehaviour {
	public GameController gameController;
	public NetworkController networkController;
	private int serverReceiveIndex;

	private bool onReceive;
	private Packet receivePacket;

	public Text roomName_text;
	public List<WaitingPlayerHandler> waitingPlayerHandlers;
	public GameObject waitingPlayer_Prefab;
	public Transform waitingPlayerFolder;
	public float waitingPlayerStarY0;
	public float waitingPlayerDistance;
	public float waitingPlayerInterval;

	void OnEnable(){
		waitingPlayerHandlers = new List<WaitingPlayerHandler> ();
		onReceive = false;
		serverReceiveIndex = networkController.AddSubscriptor (new Subscriptor(OnReceive, new Command[3]{Command.M2C_ROOM, Command.M2C_PING_VALUE, Command.M2C_READY}));
		ConstructRoom (gameController.room_String);
	}
	void OnDisable () {
		networkController.RemoveSubscriptor (serverReceiveIndex);
	}

	void Update(){
		if (onReceive) {
			AnalysisReceive (receivePacket);
			onReceive = false;
		}
	}



	// -------------  start sending data ------------- //





	// ------------- end of sending data ------------- //
	// -------------  start receive data ------------- //



	public void OnReceive(Packet packet) {
		onReceive = true;
		receivePacket = packet;
	}

	public void AnalysisReceive(Packet packet){
		switch (packet.command) {
		case Command.M2C_ROOM:
			M2C_ROOM (packet);
			break;
		case Command.M2C_PING_VALUE:
			M2C_PING_VALUE (packet);
			break;
		case Command.M2C_READY:
			M2C_READY (packet);
			break;
		default:
			break;
		}
	}

	public void M2C_ROOM(Packet packet){
		ConstructRoom(packet.s_datas [0]);
	}

	public void M2C_PING_VALUE(Packet packet){
		List<int> ping_values = packet.l_datas [0];
		for (int i = 0; i < Mathf.Min (ping_values.Count, waitingPlayerHandlers.Count); i++) {
			waitingPlayerHandlers [i].playerPing_text.text = ping_values [i].ToString ();
		}
	}

	public void M2C_READY(Packet packet){
		List<int> ready_values = packet.l_datas [0];
		for (int i = 0; i < Mathf.Min (ready_values.Count, waitingPlayerHandlers.Count); i++) {
			waitingPlayerHandlers [i].is_ready = (ready_values [i] == 1) ? true : false;
		}
	}



	// ------------- end of receive data ------------- //


	void ConstructRoom(string room_String){
		string[] struct_string = room_String.Split (';');
		roomName_text.text = struct_string [0];

		foreach (WaitingPlayerHandler wph in waitingPlayerHandlers) Destroy (wph.gameObject);
		waitingPlayerHandlers.Clear ();

		for (int i = 1; i < struct_string.Length; i++) {
			string[] temp = struct_string [i].Split(':');
			WaitingPlayerHandler wph = Instantiate (waitingPlayer_Prefab, Vector3.zero, Quaternion.identity, waitingPlayerFolder).GetComponent<WaitingPlayerHandler>();
			wph.gameObject.transform.localPosition = new Vector3 ((1 - (i % 2) * 2) * waitingPlayerInterval, waitingPlayerStarY0 - ((i - 1) / 2) * waitingPlayerDistance, 0);
			wph.playerName_text.text = temp [0];
			wph.is_ready = ((temp [1] == "1") ? true : false);
			wph.playerPrepare_gmo.SetActive (PlayerPrefs.GetString ("NickName") == temp [0]);
			waitingPlayerHandlers.Add (wph);
		}
	}
}
