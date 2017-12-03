using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Phase{
	UnsignedPhase,
	ConnectSetupPhase,
	EnterNamePhase,
	RoomWaitingPhase
}

public class GameController : MonoBehaviour {
	public NetworkController networkController;

	public ConnectSetupPhaseManager connectSetupPhaseManager;
	public EnterNamePhaseManager enterNamePhaseManager;
	public RoomWaitingPhaseManager roomWaitingPhaseManager;

	public Phase nowPhase;
	public bool phase_has_change;

	public string room_String;

	public bool has_dialog;
	public GameObject dialog_gmo;

	void Start(){
		phase_has_change = false;
		Change_Phase (Phase.ConnectSetupPhase);
	}

	void Update(){
		if (phase_has_change) {
			phase_has_change = false;

			connectSetupPhaseManager.gameObject.SetActive (false);
			enterNamePhaseManager.gameObject.SetActive (false);
			roomWaitingPhaseManager.gameObject.SetActive (false);
			if(nowPhase == Phase.ConnectSetupPhase)connectSetupPhaseManager.gameObject.SetActive (true);
			if(nowPhase == Phase.EnterNamePhase)enterNamePhaseManager.gameObject.SetActive (true);
			if(nowPhase == Phase.RoomWaitingPhase)roomWaitingPhaseManager.gameObject.SetActive (true);
		}
	}

	public void Change_Phase(Phase toPhase){
		nowPhase = toPhase;
		phase_has_change = true;
	}

	public void Start_Dialog(Dialog_Delegate d, string title, string content, int options_amount){
		if (has_dialog) return;
		has_dialog = true;
		GameObject dialog = Instantiate (dialog_gmo, Vector2.zero, Quaternion.identity);
		dialog.GetComponent<Dialog_manager> ().dialog_Delegate = d;
		dialog.GetComponent<Dialog_manager> ().options_amount = options_amount;
		dialog.transform.Find ("canvas").Find ("Title").GetComponent<Text> ().text = title;
		dialog.transform.Find ("canvas").Find ("Content").GetComponent<Text> ().text = content;
	}
}
