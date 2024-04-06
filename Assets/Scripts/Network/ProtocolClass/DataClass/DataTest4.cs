using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Network;
using Network.Base;

namespace Network.ProtocolClass
{
	public class DataTest4 : BaseNetworkData
	{
		public DataTest5 t5;
		public EnumTest enumTest = EnumTest.Windows;

		public override int GetSize()
		{
			int size = 0;
			size += t5.GetSize();
			size += 4;

			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;

			WriteClass(t5, bytes, ref index);
			WriteInt((int)enumTest, bytes, ref index);

			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, int index)
		{
			t5 = ReadClass<DataTest5>(bytes, ref index);
			enumTest = (EnumTest)ReadInt(bytes, ref index);

			return this as T;
		}
	}
}
