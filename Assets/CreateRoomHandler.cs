using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomHandler : MonoBehaviour {
	public VisitorHobPhaseManager visitorHobPhaseManager;

	public InputField roomName_InputField;
	public InputField roomDescription_InputField;
	public InputField playerMax_InputField;

	void Start(){
		visitorHobPhaseManager = GameObject.Find ("VisitorHobPhase").GetComponent<VisitorHobPhaseManager> ();
	}

	public void Create(){
		visitorHobPhaseManager.Create_Room (roomName_InputField.text, roomDescription_InputField.text, int.Parse(playerMax_InputField.text));
	}
}
