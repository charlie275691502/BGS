using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectSetupPhaseManager : MonoBehaviour {
	public GameController gameController;

	public InputField serverIP_IF;
	public InputField serverPort_IF;

	void Start(){
		serverIP_IF.text = PlayerPrefs.GetString ("serverIP");
		serverPort_IF.text = PlayerPrefs.GetString ("serverPort");
	}

	public void ConnectToServer(){
		PlayerPrefs.SetString ("serverIP", serverIP_IF.text);
		PlayerPrefs.SetString ("serverPort", serverPort_IF.text);
		gameController.networkController.StartConnection(serverIP_IF.text, int.Parse(serverPort_IF.text));
		gameController.Change_Phase (Phase.EnterNamePhase);
	}
}
