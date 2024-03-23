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
            int id;
            int length = 0;

            bytes.CopyTo(cacheBuffer, cacheIndex);
            cacheIndex += byteLength;
            while (true)
            {
                id = BitConverter.ToInt32(cacheBuffer, 0);
                length = BitConverter.ToInt32(cacheBuffer, 4);

                if (cacheIndex <= 8 || cacheIndex < length)
                {
                    // 被分包继续接收
                    break;
                }

                byte[] messageBytes = new byte[length];
                Array.Copy(cacheBuffer, messageBytes, length);
                ThreadPool.QueueUserWorkItem(HandlerThread, messageBytes);

                if (cacheIndex - length > 0)
                {
                    // 未处理移动到缓存区开头
                    Array.Copy(cacheBuffer, length, cacheBuffer, 0, cacheIndex - length);
                    cacheIndex -= length;
                }
                else
                {
                    // 已经处理完所有数据
                    cacheIndex = 0;
                    break;
                }

                // if (cacheIndex == length)
                // {
                //     // 刚好完整数据
                //     byte[] messageBytes = new byte[length];
                //     Array.Copy(cacheBuffer, messageBytes, length);
                //     cacheIndex = 0;
                //         
                //     ThreadPool.QueueUserWorkItem(HandlerThread, messageBytes);
                // }
                // else if (cacheIndex > length)
                // {
                //     // 黏包
                //     // 先处理前面完整数据
                //     byte[] messageBytes = new byte[length];
                //     Array.Copy(cacheBuffer, messageBytes, length);
                //
                //     ThreadPool.QueueUserWorkItem(HandlerThread, messageBytes);
                //
                //     // 未处理移动到缓存区开头
                //     int remainingLength = cacheIndex - length;
                //     Array.Copy(cacheBuffer, length, cacheBuffer, 0, remainingLength);
                //     cacheIndex = remainingLength;
                // }
            }
        }

        private void HandlerThread(object state)
        {
            switch (BitConverter.ToInt32((byte[])state, 0))
            {
                case 1:
                    MessageTest test = new MessageTest().Deserialize<MessageTest>((byte[])state, 8);
                    Debug.Log($"client: {test.data}");
                    break;
                case 2:
                    MessageTest2 test2 = new MessageTest2().Deserialize<MessageTest2>((byte[])state, 8);
                    Debug.Log($"client: {test2.data2} {test2.t5.enumTest}");
                    break;
            }
        }
    }
}