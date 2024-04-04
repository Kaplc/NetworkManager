using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
	public class DataTest3 : BaseNetworkData
	{
		public List<EnumTest> lis3 = new List<EnumTest>();
		public EnumTest enumTest = EnumTest.Windows;

		public override int GetSize()
		{
			int size = 0;

			size += sizeof(int);
			foreach (var item in lis3)
			{
				size += 4;
			}
			size += 4;

			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;


			WriteInt(lis3.Count, bytes, ref index);
			foreach (var item in lis3)
			{
				WriteInt((int)item, bytes, ref index);
			}
			WriteInt((int)enumTest, bytes, ref index);

			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, int index)
		{

			int listCount = ReadInt(bytes, ref index);
			for (int i = 0; i < listCount; i++)
			{
				lis3.Add((EnumTest)ReadInt(bytes, ref index));
			}
			enumTest = (EnumTest)ReadInt(bytes, ref index);

			return this as T;
		}
	}
}
