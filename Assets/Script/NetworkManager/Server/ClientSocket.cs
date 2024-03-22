using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Script.NetworkManager
{
    public class ClientSocket
    {
        public string guid;
        private Socket socket;

        private byte[] cacheBuffer = new byte[1024 * 1024]; // 分包粘包处理
        private int cacheIndex = 0;
        private int remainingLength = -1;

        public ClientSocket(Socket socket)
        {
            guid = Guid.NewGuid().ToString();
            this.socket = socket;
        }

        public void Send(string info)
        {
            socket.Send(Encoding.UTF8.GetBytes(info));
        }

        public void Receive()
        {
            if (socket.Available > 0)
            {
                byte[] bytes = new byte[socket.Available];
                socket.Receive(bytes);

                HandlePackage(bytes, bytes.Length);
            }
        }

        private void HandlePackage(byte[] bytes, int byteLength)
        {
            int nowIndex = 0;
            int id;
            int length = 0;

            bytes.CopyTo(cacheBuffer, cacheIndex);
            cacheIndex += byteLength;

            if (cacheIndex > 8)
            {
                id = BitConverter.ToInt32(cacheBuffer, 0);
                nowIndex += 4;
                length = BitConverter.ToInt32(cacheBuffer, 4);
                nowIndex += 4;
            }

            if (cacheIndex > nowIndex && cacheIndex <= length && nowIndex != 0)
            {
                if (cacheIndex == length)
                {
                    // 有完整数据
                    byte[] messageBytes = new byte[length];
                    Array.Copy(cacheBuffer, messageBytes, length);
                    cacheIndex = 0;

                    ThreadPool.QueueUserWorkItem(HandlerThread, messageBytes);
                }
            }
        }

        private void HandlerThread(object state)
        {
            MessageTest test = new MessageTest().Deserialize<MessageTest>((byte[])state, 8);
            Debug.Log($"client: {test.data}");
        }
    }
}