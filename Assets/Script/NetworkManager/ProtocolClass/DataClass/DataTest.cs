using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
	public class DataTest : BaseNetworkData
	{
		public EnumTest enumTest = EnumTest.Android;
		public DataTest2 dataTest2;

		public override int GetSize()
		{
			int size = 0;
			size += 4;
			size += dataTest2.GetSize();

			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;

			WriteInt((int)enumTest, bytes, ref index);
			WriteClass(dataTest2, bytes, ref index);

			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, int index)
		{
			enumTest = (EnumTest)ReadInt(bytes, ref index);
			dataTest2 = ReadClass<DataTest2>(bytes, ref index);

			return this as T;
		}
	}
}
