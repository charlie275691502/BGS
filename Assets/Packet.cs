using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

static class Constants{
	public const int version = 1;
}

public enum Data_Type{
	Byte,
	Short,
	Unsigned_Short,
	SP_Float, /* single_precision_float */
	String
}

public enum NUM{
	Integer,
	Decimal,
	String
}

public class Pair{
	public int first;
	public int second;
	public int third;
}

public enum Command{
	Unsigned = 0x00,

	C2M_PING = 0x10,
	C2M_VISIT = 0x20,
	C2M_CREATE = 0x21,
	C2M_JOIN = 0x22,
	C2M_ROOM = 0x24,

	M2C_PING = 0x90,
	M2C_HOB = 0xA0,
	M2C_CREATE = 0xA1,
	M2C_JOIN = 0xA2,
	M2C_ROOM = 0xA4
}

public class Packet{
	public int version;
	public Command command;
	public int[] datas;
	public float[] f_datas;
	public string[] s_datas;
	public byte[] b_datas;

	public Packet(){

	}

	// 輸入數值，並自動建立bytes[]
	public Packet(Command c, int[] d, float[] f_d, string[] s_d){	new_Packet (c, d, f_d, s_d);	}
	public Packet(Command c, int[] d, float[] f_d              ){	new_Packet (c, d, f_d, new string[0]);	}
	public Packet(Command c, int[] d,              string[] s_d){	new_Packet (c, d, new float[0], s_d);	}
	public Packet(Command c, int[] d                           ){	new_Packet (c, d, new float[0], new string[0]);	}
	public Packet(Command c,          float[] f_d, string[] s_d){	new_Packet (c, new int[0], f_d, s_d);	}
	public Packet(Command c,          float[] f_d              ){	new_Packet (c, new int[0], f_d, new string[0]);	}
	public Packet(Command c,					   string[] s_d){	new_Packet (c, new int[0], new float[0], s_d);	}
	public Packet(Command c                      	    	   ){	new_Packet (c, new int[0], new float[0], new string[0]);	}

	public void new_Packet(Command c, int[] d, float[] f_d, string[] s_d){
		version = Constants.version;
		command = c;
		datas = d;
		f_datas = f_d;
		s_datas = s_d;
		b_datas = Generate_b_datas();
	}

	public Packet(byte[] b_d){
		b_datas = b_d;
		version = b_d [1];
		command = (Command)b_d[2];
		Data_Type[] slices = Get_Packet_Slices ();
		Pair pair = Get_Slices_Length (slices);
		datas = new int[pair.first];
		f_datas = new float[pair.second];
		s_datas = new string[pair.third];

		int d_pointer = 0;
		int f_d_pointer = 0;
		int s_d_pointer = 0;
		int b_d_pointer = 5;
		for(int i=0;i<slices.Length;i++){
			int num = 0; 
			byte[] bytes = new byte[0];
			switch (slices [i]) {
			case Data_Type.Byte:
				num = b_d [b_d_pointer++];
				datas [d_pointer++] = (num >= 128) ? num - 256 : num;
				break;
			case Data_Type.Short:
				num =  b_d [b_d_pointer++];
				num += b_d [b_d_pointer++] * 256;
				datas [d_pointer++] = (num >= 32768) ? num - 65536 : num;
				break;
			case Data_Type.Unsigned_Short:
				num =  b_d [b_d_pointer++];
				num += b_d [b_d_pointer++] * 256;
				datas [d_pointer++] = num;
				break;
			case Data_Type.SP_Float:
				bytes = new byte[4];
				bytes [0] = b_d [b_d_pointer++];
				bytes [1] = b_d [b_d_pointer++];
				bytes [2] = b_d [b_d_pointer++];
				bytes [3] = b_d [b_d_pointer++];
				f_datas [f_d_pointer++] = Bytes_To_SP_Float (bytes);
				break;
			case Data_Type.String:
				num = b_d [b_d_pointer++];
				bytes = new byte[num];
				for (int j = 0; j < num; j++) bytes [j] = b_d [b_d_pointer++];
				s_datas [s_d_pointer++] = System.Text.Encoding.ASCII.GetString (bytes);
				break;
				//hi_string
			default:
				break;
			}
		}

	}

	public byte[] Generate_b_datas(){
		Data_Type[] slices = Get_Packet_Slices ();

		if (slices.Length != (datas.Length + f_datas.Length + s_datas.Length)) {
			N_Print ("封包長度或參數數量錯誤\n" + command.ToString() + " ");
			return null;
		}

		int data_length = Get_Bytes_Amount (slices, s_datas);
		byte[] ret = new byte[5 + data_length];
		ret[0] = 0xa5;
		ret[1] = System.Convert.ToByte(version);
		ret[2] = System.Convert.ToByte((int)command);
		ret[3] = System.Convert.ToByte(data_length % 256);
		ret[4] = System.Convert.ToByte(data_length / 256);

		int d_pointer = 0;
		int f_d_pointer = 0;
		int s_d_pointer = 0;
		int b_d_pointer = 5;
		for(int i=0;i<slices.Length;i++){
			int num = 0;
			float f = 0.0f;
			string s = "";
			Byte[] temp_bytes;
			switch (slices [i]) {
			case Data_Type.Byte:
				num = datas [d_pointer] + ((datas [d_pointer] > 0) ? 0 : 256);
				d_pointer++;
				ret [b_d_pointer++] = System.Convert.ToByte (num);
				break;
			case Data_Type.Short:
				num = datas [d_pointer] + ((datas [d_pointer] > 0) ? 0 : 65536);
				d_pointer++;
				ret [b_d_pointer++] = System.Convert.ToByte (num % 256);
				ret [b_d_pointer++] = System.Convert.ToByte (num / 256);
				break;
			case Data_Type.Unsigned_Short:
				num = datas [d_pointer++];
				ret [b_d_pointer++] = System.Convert.ToByte (num % 256);
				ret [b_d_pointer++] = System.Convert.ToByte (num / 256);
				break;
			case Data_Type.SP_Float:
				f = f_datas [f_d_pointer++];
				temp_bytes = SP_Float_To_Bytes (f);
				ret [b_d_pointer++] = temp_bytes [0];
				ret [b_d_pointer++] = temp_bytes [1];
				ret [b_d_pointer++] = temp_bytes [2];
				ret [b_d_pointer++] = temp_bytes [3];
				break;
			case Data_Type.String:
				s = s_datas [s_d_pointer++];
				temp_bytes = System.Text.Encoding.ASCII.GetBytes (s);
				ret [b_d_pointer++] = System.Convert.ToByte (s.Length);
				for (int j = 0; j < s.Length; j++) ret [b_d_pointer++] = temp_bytes [j];
				break;
			default:
				break;
			}
		}
		return ret;
	}

	public Pair Get_Slices_Length(Data_Type[] slices){
		Pair ret = new Pair ();
		foreach (Data_Type slice in slices) {
			if (Decide_NUM (slice) == NUM.Integer) ret.first++;
			else if(Decide_NUM (slice) == NUM.Decimal)ret.second++;
			else ret.third++;
		}
		return ret;
	}

	public int Get_Bytes_Amount(Data_Type[] slices, string[] s_s){
		int ret = 0;
		foreach (Data_Type slice in slices) {
			switch (slice) {
			case Data_Type.Byte:			ret += 1;	break;
			case Data_Type.Short:			ret += 2;	break;
			case Data_Type.Unsigned_Short:	ret += 2;	break;
			case Data_Type.SP_Float:		ret += 4;	break;
			case Data_Type.String:						break;
			}
		}
		foreach (string s in s_s) ret += s.Length + 1;
		return ret;
	}

	NUM Decide_NUM(Data_Type data_Type){
		switch (data_Type) {
		case Data_Type.Byte:			return NUM.Integer;
		case Data_Type.Short:			return NUM.Integer;
		case Data_Type.Unsigned_Short:	return NUM.Integer;
		case Data_Type.SP_Float:		return NUM.Decimal;
		case Data_Type.String:			return NUM.String;
		}
		return NUM.Integer;
	}

	Data_Type[] Get_Packet_Slices(){
		switch (command) {
		case Command.C2M_VISIT:			return new Data_Type[1]{Data_Type.String};
		case Command.C2M_CREATE:		return new Data_Type[3]{Data_Type.String, Data_Type.String, Data_Type.Byte};
		case Command.C2M_JOIN:			return new Data_Type[1]{Data_Type.String};
		case Command.C2M_ROOM:			return new Data_Type[1]{Data_Type.String};
		case Command.C2M_PING:			return new Data_Type[0];

		case Command.M2C_HOB:			return new Data_Type[1]{Data_Type.String};
		case Command.M2C_CREATE:		return new Data_Type[1]{Data_Type.Byte};
		case Command.M2C_JOIN:			return new Data_Type[1]{Data_Type.Byte};
		case Command.M2C_ROOM:			return new Data_Type[1]{Data_Type.String};
		case Command.M2C_PING:			return new Data_Type[0];
		default:
			break;
		}
		N_Print ("Command type not found");
		return null;
	}

	public void Print(string s){
		if (datas == null || b_datas == null || f_datas == null || s_datas == null) {
			N_Print ("datas == null || b_datas == null || f_datas == null || s_datas == null");
			return;
		}

		string d_string = "";
		Data_Type[] data_Types = Get_Packet_Slices ();
		int d_pointer = 0;
		int f_d_pointer = 0;
		int s_d_pointer = 0;
		foreach (Data_Type data_Type in data_Types) {
			if (d_pointer + f_d_pointer > 0) d_string += ",";
			if (Decide_NUM (data_Type) == NUM.Integer) d_string += datas [d_pointer++].ToString ();
			else if (Decide_NUM (data_Type) == NUM.Decimal) d_string += f_datas [f_d_pointer++].ToString ();
			else d_string += s_datas [s_d_pointer++];
		}
		string b_d_string = "";
		foreach (byte b_d in b_datas)b_d_string += String.Format ("{0:X2}", b_d) + " ";
		N_Print(s + "\nversion: " + version.ToString() + "\nCommand: " + command + "\ndatas: " + d_string + "\nb_datas: " + b_d_string);
	}

	private void N_Print(string s){
		Debug.Log (s);
	}

	float Bytes_To_SP_Float(byte[] bytes){
		MemoryStream stream = new MemoryStream();
		BinaryReader br = new BinaryReader (stream);
		br.Read (bytes, 0, 4);
		return BitConverter.ToSingle (bytes , 0);
	}

	byte[] SP_Float_To_Bytes(float f){
		MemoryStream stream = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(stream);
		bw.Write(f);
		bw.Flush();
		return stream.ToArray();
	}
}