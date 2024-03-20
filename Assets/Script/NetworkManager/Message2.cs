using System;
using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
    [Serializable]
    public class Message2 : BaseNetworkData
    {
        public int a;
        public float b;
        public bool c;
        public string d;
        public List<string> strList = new List<string>();
        public Message1 message1;

        public override int GetSize()
        {
            int size = 0;

            size += sizeof(int);
            size += sizeof(float);
            size += sizeof(bool);
            size += sizeof(int) + Encoding.UTF8.GetByteCount(d);
            size += message1.GetSize();

            return size;
        }

        public override byte[] Serialize()
        {
            byte[] bytes = new byte[GetSize()];
            int index = 0;

            WriteInt(a, bytes, ref index);
            WriteFloat(b, bytes, ref index);
            WriteBool(c, bytes, ref index);
            WriteString(d, bytes, ref index);
            message1.Serialize().CopyTo(bytes, index);

            WriteInt(strList.Count, bytes, ref index);
            foreach (var item in strList)
            {
                WriteString(item, bytes, ref index);
            }

            return bytes;
        }

        public override T Deserialize<T>(byte[] bytes, ref int index)
        {
            a = ReadInt(bytes, ref index);
            b = ReadFloat(bytes, ref index);
            c = ReadBool(bytes, ref index);
            d = ReadString(bytes, ref index);

            int count = ReadInt(bytes, ref index);
            for (int i = 0; i < count; i++)
            {
                strList.Add(ReadString(bytes, ref index));
            }


            message1 = new Message1().Deserialize<Message1>(bytes, ref index);

            return this as T;
        }
    }
}