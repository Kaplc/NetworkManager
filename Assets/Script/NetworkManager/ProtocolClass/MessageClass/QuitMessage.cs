using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
	public class QuitMessage : BaseNetworkData
	{
		public int messageID = 500;

		public override int GetSize()
		{
			int size = 0;
			size += sizeof(int); // message id
			size += sizeof(int); // message length


			return size;
		}

		public override byte[] Serialize()
		{
			byte[] bytes = new byte[GetSize()];
			int index = 0;
			WriteInt(messageID, bytes, ref index);
			WriteInt(GetSize(), bytes, ref index);


			return bytes;
		}

		public override T Deserialize<T>(byte[] bytes, int index)
		{

			return this as T;
		}
	}
}
