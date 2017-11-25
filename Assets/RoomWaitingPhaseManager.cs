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
	public Text roomDescription_text;
	public Text roomList_text;

	void OnEnable(){
		onReceive = false;
		serverReceiveIndex = networkController.AddSubscriptor (new Subscriptor(OnReceive, new Command[1]{Command.M2C_ROOM}));
		C2M_ROOM ();
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

	public void C2M_ROOM(){
		networkController.SendToServer (new Packet (Command.C2M_ROOM));
	}





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
		default:
			break;
		}
	}

	public void M2C_ROOM(Packet packet){
		string room_String = packet.s_datas [0];
		string[] struct_string = room_String.Split (';');
		roomName_text.text = struct_string [0];
		roomDescription_text.text = struct_string [1];

		string member_list = "";
		for (int i = 2; i < struct_string.Length; i++) {
			member_list = struct_string [i] + "\n";
		}
		roomList_text.text = member_list;
	}



	// ------------- end of receive data ------------- //



}
