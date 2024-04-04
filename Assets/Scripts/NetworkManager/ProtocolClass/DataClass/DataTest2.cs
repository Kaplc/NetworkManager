using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
	public class DataTest2 : BaseNetworkData
	{
		public Dictionary<int, DataTest3> dic3 = new Dictionary<int, DataTest3>();
		public DataTest3 dataTest3;
		public EnumTest enumTest = EnumTest.Unity;

		public override int GetSize()
		{
			int size = 0;

			size += sizeof(int);
			foreach (var item in dic3)
			{
				size += sizeof(int);
				size += item.Value.GetSize();
			}
			size += dataTest3.GetSize();
			size += 4;

			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;


			WriteInt(dic3.Count, bytes, ref index);
			foreach (var item in dic3)
			{
				WriteInt(item.Key, bytes, ref index);
				WriteClass(item.Value, bytes, ref index);
			}
			WriteClass(dataTest3, bytes, ref index);
			WriteInt((int)enumTest, bytes, ref index);

			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, int index)
		{

			int dicCount = ReadInt(bytes, ref index);
			for (int i = 0; i < dicCount; i++)
			{
				dic3.Add(ReadInt(bytes, ref index),ReadClass<DataTest3>(bytes, ref index));
			}
			dataTest3 = ReadClass<DataTest3>(bytes, ref index);
			enumTest = (EnumTest)ReadInt(bytes, ref index);

			return this as T;
		}
	}
}
