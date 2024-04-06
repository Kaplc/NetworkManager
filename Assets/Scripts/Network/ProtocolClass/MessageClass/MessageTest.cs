using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Network;
using Network.Base;

namespace Network.ProtocolClass
{
	public class MessageTest : BaseMessage
	{
		public int messageID = 1;
		public int data = 1;

		public override int GetSize()
		{
			int size = 0;
			size += sizeof(int); // message id
			size += sizeof(int); // message length

			size += sizeof(int);

			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;
			WriteInt(messageID, bytes, ref index);
			WriteInt(GetSize(), bytes, ref index);

			WriteInt(data, bytes, ref index);

			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, int index)
		{
			data = ReadInt(bytes, ref index);

			return this as T;
		}
	}
}
