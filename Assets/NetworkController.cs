using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class NetworkController : MonoBehaviour {
	private string serverIp = "127.0.0.1";
	private int serverPort = 6805;
	public GameController gameController;

	private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	private byte[] _recieveBuffer = new byte[2048];
	public bool hide_ping_msg;

	private List<Subscriptor> subscriptors = new List<Subscriptor> {};

	void Start () {
		StartConnection(serverIp, serverPort);
		StartCoroutine (Start_Ping ());
		AddSubscriptor (new Subscriptor (OnReceive, new Command[1]{ Command.M2C_PING }));
	}

	void Update(){
		if (onReceive) {
			AnalysisReceive (receivePacket);
			onReceive = false;
		}
	}

	void OnApplicationQuit() {
		CloseConnection();
	}

	public void StartConnection(string IP, int Port) {
		try
		{
			_clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(IP), Port), new AsyncCallback(OnConnect), _clientSocket);
		}
		catch(SocketException ex)
		{
			Debug.Log(ex.Message);
		}
	}
	public void SendToServer(Packet packet) {
		if (packet.command != Command.C2M_PING || !hide_ping_msg)packet.Print ("SEND");

		byte[] data = packet.b_datas;
		try
		{
			byte[] byteArray = data;
			SendData(byteArray);
		}
		catch(SocketException ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}
	private void SendData(byte[] data)
	{
		SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
		socketAsyncData.SetBuffer(data,0,data.Length);
		_clientSocket.SendAsync(socketAsyncData);
	}
	private void OnConnect(IAsyncResult iar)
	{
		Debug.Log ("On Server Connected");
		_clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback), null);
		is_connect = true;
	}

	private bool onReceive;
	private Packet receivePacket;
	public void OnReceive(Packet packet) {
		onReceive = true;
		receivePacket = packet;
	}

	public void AnalysisReceive(Packet packet){
		switch (packet.command) {
		case Command.M2C_PING:
			ping_time = 0;
			break;
		default:
			break;
		}
	}

	bool is_connect = false;
	int ping_time = 0;
	IEnumerator Start_Ping(){
		Packet ping = new Packet (Command.C2M_PING);
		while(true){
			if (is_connect) {
				SendToServer (ping);
				if (++ping_time >= 3) {
					gameController.Start_Dialog (null, "Error", "Disconnected from server.", 1);
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	/// 接收封包.
	private void ReceiveCallback(IAsyncResult AR)
	{
		int recieved = _clientSocket.EndReceive(AR);
//		Debug.Log ("received " + recieved.ToString () + " bytes");
		if(recieved <= 0)
			return;

		byte[] recData = new byte[recieved];
		Buffer.BlockCopy(_recieveBuffer,0,recData,0,recieved);
		Packet packet = new Packet(recData);
		if (packet.command != Command.M2C_PING || !hide_ping_msg)packet.Print ("RECEIVED");

		// Notify other managers when receiving data from server
		foreach (Subscriptor subscriptor in subscriptors) {
			foreach (Command command in subscriptor.commands) {
				if (command == packet.command) {
					subscriptor.subscriptor_Delegate (packet);
					break;
				}
			}
		}
		_clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback), null);
	}

	public int AddSubscriptor(Subscriptor subscriptor) {
		int index = subscriptors.Count;
		subscriptors.Add (subscriptor);
		return index;
	}

	public void RemoveSubscriptor(int index) {
		subscriptors.RemoveAt (index);
	}
		
	/// 關閉 Socket 連線.
	public void CloseConnection() {
		_clientSocket.Shutdown(SocketShutdown.Both);
		_clientSocket.Close();
	}
}