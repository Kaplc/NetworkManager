using System;
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

            return bytes;
        }

        public override int Deserialize(byte[] bytes, ref int index)
        {
            a = ReadInt(bytes, ref index);
            b = ReadFloat(bytes, ref index);
            c = ReadBool(bytes, ref index);
            d = ReadString(bytes, ref index);
            message1 = new Message1();
            message1.Deserialize(bytes, ref index);

            return GetSize();
        }
    }
}