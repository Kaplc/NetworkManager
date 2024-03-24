using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEngine;
using Timer = System.Timers.Timer;

namespace Script.NetworkManager
{
    public class ClientSocket
    {
        public string guid;
        public Socket socket;
        private Server server;

        private byte[] cacheBuffer = new byte[1024 * 1024]; // 分包粘包处理
        private int cacheIndex = 0;
        
        private int heartTime = 0;
        private int outTime = 3;
        private Timer timer;

        public ClientSocket(Server server ,Socket socket)
        {
            guid = Guid.NewGuid().ToString();
            this.socket = socket;
            this.server = server;
            
            // 创建一个定时器对象，设置时间间隔为1秒
            timer = new Timer(1000);
        
            // 定义定时器的Elapsed事件处理方法
            timer.Elapsed += (obj,args) =>
            {
                heartTime += 1;
                if (heartTime >= outTime)
                {
                    // 超时
                    server.willRemoveClientSockets.Enqueue(this);
                    timer.Dispose();
                }
            };

            // 启动定时器
            timer.Start();
        }

        public void Close()
        {
            socket.Disconnect(false);
            socket.Close();
            Debug.Log("remove client socket at " + guid);
        }

        public void Send(BaseNetworkData data)
        {
            socket.Send(data.Serialize());
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

                if (cacheIndex < 8 || cacheIndex < length)
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
            }
        }

        private void HandlerThread(object state)
        {
            int id = BitConverter.ToInt32((byte[])state, 0);
            
            switch (id)
            {
                case 1:
                    MessageTest test = new MessageTest().Deserialize<MessageTest>((byte[])state, 8);
                    Debug.Log($"client: {test.data}");
                    break;
                case 2:
                    MessageTest2 test2 = new MessageTest2().Deserialize<MessageTest2>((byte[])state, 8);
                    Debug.Log($"client: {test2.data2} {test2.t5.enumTest}");
                    break;
                case 500:
                    server.willRemoveClientSockets.Enqueue(this);
                    break;
                case 123:
                    TextMessage textMessage = new TextMessage().Deserialize<TextMessage>((byte[])state, 8);
                    Debug.Log($"client: {textMessage.text}");
                    break;
                case 99:
                    Debug.Log("heart");
                    heartTime = 0;
                    break;
            }
        }
    }
}