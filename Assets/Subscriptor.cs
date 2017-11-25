using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void Subscriptor_Delegate(Packet packet);

public class Subscriptor{
	public Subscriptor_Delegate subscriptor_Delegate;
	public Command[] commands;

	public Subscriptor(Subscriptor_Delegate s, Command[] m){
		subscriptor_Delegate = s;
		commands = m;
	}
}