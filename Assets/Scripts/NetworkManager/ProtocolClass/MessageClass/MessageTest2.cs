using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
	public class MessageTest2 : BaseNetworkData
	{
		public int messageID = 2;
		public int data2 = 1;
		public DataTest5 t5;

		public override int GetSize()
		{
			int size = 0;
			size += sizeof(int); // message id
			size += sizeof(int); // message length

			size += sizeof(int);
			size += t5.GetSize();

			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;
			WriteInt(messageID, bytes, ref index);
			WriteInt(GetSize(), bytes, ref index);

			WriteInt(data2, bytes, ref index);
			WriteClass(t5, bytes, ref index);

			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, int index)
		{
			data2 = ReadInt(bytes, ref index);
			t5 = ReadClass<DataTest5>(bytes, ref index);

			return this as T;
		}
	}
}
