using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisitorHobPhaseManager : MonoBehaviour {
	public GameController gameController;
	public NetworkController networkController;
	private int serverReceiveIndex;

	private bool onReceive;
	private Packet receivePacket;

	public Text visitorsList_text;

	[HideInInspector] public List<WaitingRoomHandler> waitingRoomHandlers;
	[HideInInspector] public CreateRoomHandler createRoomHandler;
	public Transform waitingRoomFolder;
	public GameObject waitingRoom_Prefab;
	public GameObject createRoom_Prefab;
	public float waitingRoomDistance;

	void OnEnable(){
		onReceive = false;
		waitingRoomHandlers = new List<WaitingRoomHandler> ();
		serverReceiveIndex = networkController.AddSubscriptor (new Subscriptor(OnReceive, new Command[3]{Command.M2C_HOB, Command.M2C_JOIN, Command.M2C_CREATE}));
		change_Hob ();
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
		case Command.M2C_HOB:
			M2C_HOB (packet);
			break;
		case Command.M2C_CREATE:
			M2C_CREATE (packet);
			break;
		case Command.M2C_JOIN:
			M2C_JOIN (packet);
			break;
		default:
			break;
		}
	}

	void M2C_HOB(Packet packet){
		gameController.hob_String = packet.s_datas [0];	
		change_Hob ();
	}

	void M2C_CREATE(Packet packet){
		if(packet.datas[0] == 1)gameController.Change_Phase (Phase.RoomWaitingPhase);
	}

	void M2C_JOIN(Packet packet){
		if(packet.datas[0] == 1)gameController.Change_Phase (Phase.RoomWaitingPhase);
	}



	// ------------- end of receive data ------------- //

	void change_Hob(){
		string hob_String = gameController.hob_String;
		string[] struct_string = hob_String.Split ('!');
		string[] visitors = struct_string [0].Split (';');
		string visitorsList = "";
		foreach (string s in visitors) {
			visitorsList += s + "\n";
		}
		visitorsList_text.text = visitorsList;

		foreach (WaitingRoomHandler wrh in waitingRoomHandlers) Destroy (wrh.gameObject);
		waitingRoomHandlers.Clear ();

		for (int i = 1; i < struct_string.Length; i++) {
			WaitingRoomHandler wrh = Instantiate (waitingRoom_Prefab, Vector3.zero, Quaternion.identity, waitingRoomFolder).GetComponent<WaitingRoomHandler>();
			wrh.gameObject.transform.localPosition = new Vector3(0, (2-i) * waitingRoomDistance, 0);
			wrh.room_string = struct_string [i];
			waitingRoomHandlers.Add (wrh);
		}
		if(createRoomHandler != null)Destroy (createRoomHandler.gameObject);
		createRoomHandler = Instantiate (createRoom_Prefab, Vector3.zero, Quaternion.identity, waitingRoomFolder).GetComponent<CreateRoomHandler>();
		createRoomHandler.gameObject.transform.localPosition = new Vector3 (0, (2 - struct_string.Length) * waitingRoomDistance, 0);
	}

	public void Create_Room(string room_Name, string room_Description, int max_Player){
		networkController.SendToServer (new Packet(Command.C2M_CREATE, new int[1] {max_Player}, new string[2]{room_Name, room_Description}));
	}

	public void Join_Room(string room_Name){
		networkController.SendToServer (new Packet(Command.C2M_JOIN, new string[1]{room_Name}));
	}
}
