using System.Text;

namespace Script.NetworkManager
{
    public class Message1: BaseNetworkData
    {
        public string abc;

        public override int GetSize()
        {
            int size = 0;
            size += sizeof(int) + Encoding.UTF8.GetByteCount(abc);
            return size;
        }

        public override byte[] Serialize()
        {
            byte[] bytes = new byte[GetSize()];
            int index = 0;
            WriteString(abc, bytes, ref index);
            return bytes;
        }

        public override int Deserialize(byte[] bytes, ref int index)
        {
            abc = ReadString(bytes, ref index);
            return GetSize();
        }
    }
}