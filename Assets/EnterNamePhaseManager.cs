using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterNamePhaseManager : MonoBehaviour {

	public GameController gameController;
	public NetworkController networkController;
	private int serverReceiveIndex;

	private bool onReceive;
	private Packet receivePacket;

	public InputField nickName_InputField;

	void OnEnable(){
		nickName_InputField.text = PlayerPrefs.GetString ("NickName");
		onReceive = false;
		serverReceiveIndex = networkController.AddSubscriptor (new Subscriptor(OnReceive, new Command[1]{Command.M2C_HOB}));
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

	public void SetNickName(){
		nickName_InputField.interactable = false;
		networkController.SendToServer (new Packet (Command.C2M_VISIT, new string[1]{ nickName_InputField.text }));
	}

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
		default:
			break;
		}
	}

	void M2C_HOB(Packet packet){
		nickName_InputField.interactable = true;
		string hob_String = packet.s_datas [0];
		if (hob_String == "") {
			gameController.Start_Dialog (null, "錯誤", "已被使用的暱稱", 1);
		} else {
			gameController.Start_Dialog (null, "歡迎", nickName_InputField.text, 1);
			PlayerPrefs.SetString ("NickName", nickName_InputField.text);
			gameController.hob_String = hob_String;
			gameController.Change_Phase (Phase.VisitorHobPhase);
		}
	}


	// ------------- end of receive data ------------- //




}
