using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
	public class DataTest5 : BaseNetworkData
	{
		public EnumTest enumTest = EnumTest.Windows;

		public override int GetSize()
		{
			int size = 0;
			size += 4;

			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;

			WriteInt((int)enumTest, bytes, ref index);

			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, ref int index)
		{
			enumTest = (EnumTest)ReadInt(bytes, ref index);

			return this as T;
		}
	}
}
