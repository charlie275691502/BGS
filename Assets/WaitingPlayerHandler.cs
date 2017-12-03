using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingPlayerHandler : MonoBehaviour {
	[HideInInspector] public RoomWaitingPhaseManager roomWaitingPhaseManager;

	public Text playerName_text;
	public SpriteRenderer playerFrame_sr;
	public Text playerPing_text;
	public GameObject playerPrepare_gmo;
	public Sprite not_ready_image;
	public Sprite ready_image;
	private bool is_ready_hidden;
	public bool is_ready {
		get{
			return is_ready_hidden;
		}
		set{
			is_ready_hidden = value;
			playerFrame_sr.color = (value) ? Color.green : Color.white;
			playerPrepare_gmo.GetComponent<SpriteRenderer> ().sprite = (value) ? ready_image : not_ready_image;
		}
	}

	void Start(){
		is_ready = false;
		roomWaitingPhaseManager = GameObject.Find ("RoomWaitingPhase").GetComponent<RoomWaitingPhaseManager> ();
	}

	public void Click_Ready(){
		roomWaitingPhaseManager.networkController.SendToServer (new Packet (Command.C2M_READY, new int[1]{(!is_ready) ? 1 : 0}));
	}
}
