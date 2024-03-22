using System.Collections.Generic;
using System.Text;

namespace Script.NetworkManager
{
    public class Message1: BaseNetworkData
    {
        public string abc;
        public List<string> strList = new List<string>();
        public Dictionary<string, string> strDic = new Dictionary<string, string>();

        public override int GetSize()
        {
            int size = 0;
            size += sizeof(int) + Encoding.UTF8.GetByteCount(abc);
            
            size += sizeof(int);
            foreach (string item in strList)
            {
                size += sizeof(int) + Encoding.UTF8.GetByteCount(item);
            }
            
            size += sizeof(int);
            foreach (KeyValuePair<string, string> item in strDic)
            {
                size += sizeof(int) + Encoding.UTF8.GetByteCount(item.Key);
                size += sizeof(int) + Encoding.UTF8.GetByteCount(item.Value);
            }
            
            return size;
        }

        public override byte[] Serialize()
        {
            byte[] bytes = new byte[GetSize()];
            int index = 0;
            WriteString(abc, bytes, ref index);
            return bytes;
        }

        public override T Deserialize<T>(byte[] bytes, int index)
        {
            abc = ReadString(bytes, ref index);
            return this as T;
        }
    }
}