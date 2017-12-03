using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Ready : MonoBehaviour {
	public WaitingPlayerHandler waitingPlayerHandler;

	void OnMouseUp(){
		waitingPlayerHandler.Click_Ready ();
	}
}
