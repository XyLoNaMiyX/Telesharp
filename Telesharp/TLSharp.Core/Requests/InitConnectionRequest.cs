﻿using System;
using System.IO;
using TLSharp.Core.MTProto;

namespace TLSharp.Core.Requests
{
	//public class InitConnectionRequest : MTProtoRequest
	//{
	//	private int _apiId;
	//	public ConfigConstructor ConfigConstructor { get; set; }

	//	public InitConnectionRequest(int apiId)
	//	{
	//		_apiId = apiId;
	//	}

	//	public override void OnSend(TBinaryWriter writer)
	//	{
	//		writer.Write(0x39620c41); // invokeWithLayer10#39620c41, invokeWithLayer11#a6b88fdf
	//		writer.Write(0x69796de9); // initConnection
	//		writer.Write(_apiId); // api id
	//		Serializers.String.write(writer, "WinPhone Emulator"); // device model
	//		Serializers.String.write(writer, "WinPhone 8.0"); // system version
	//		Serializers.String.write(writer, "1.0-SNAPSHOT"); // app version
	//		Serializers.String.write(writer, "en"); // lang code

	//		writer.Write(0xc4f9186b); // help.getConfig
	//	}

	//	public override void OnResponse(TBinaryReader reader)
	//	{
	//		uint code = reader.ReadUInt32();
	//		ConfigConstructor config = new ConfigConstructor();
	//		config.Read(reader);

	//		ConfigConstructor = config;
	//	}

	//	public override void OnException(Exception exception)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public override bool Responded
	//	{
	//		get { return true; }
	//	}

	//	public override void OnSendSuccess()
	//	{

	//	}
	//	public override bool Confirmed => true;
	//}
}
