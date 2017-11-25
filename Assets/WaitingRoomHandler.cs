using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomHandler : MonoBehaviour {
	public VisitorHobPhaseManager visitorHobPhaseManager;

	public Text roomName_text;
	public Text roomDescription_text;
	public Text roomChief_text;
	public Text playerCount_text;
	public Text playerMax_text;

	public string room_string;

	void Start(){
		visitorHobPhaseManager = GameObject.Find ("VisitorHobPhase").GetComponent<VisitorHobPhaseManager> ();
		change_Room ();
	}

	void change_Room(){
		string[] struct_string = room_string.Split (';');
		roomName_text.text = struct_string [0];
		roomDescription_text.text = struct_string [1];
		roomChief_text.text = struct_string [2];
		playerCount_text.text = (struct_string.Length - 2).ToString();
		playerMax_text.text = "99";
	}

	void OnMouseUp(){
		Debug.Log ("click join");
		visitorHobPhaseManager.Join_Room (roomName_text.text);
	}
}
