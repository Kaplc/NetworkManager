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
        public Socket socket;

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
                byte[] bytes = new byte[socket.ReceiveBufferSize];
                socket.Receive(bytes);
                int messageID = BitConverter.ToInt32(bytes, 0);
                int length = BitConverter.ToInt32(bytes, 4);
                
                BaseNetworkData data = null;
                switch (messageID)
                {
                    case 1:
                        data = new MessageTest();
                        data.Deserialize<MessageTest>(bytes, 8);
                        break;
                }
                
                // new thread handler
                ThreadPool.QueueUserWorkItem(Handler, data);
            }
        }

        private void Handler(object state)
        {
            BaseNetworkData data = state as BaseNetworkData;
            switch (data)
            {
                case MessageTest messageTest:
                    Debug.Log($"receive message id: {messageTest.messageID}, data: {messageTest.data}");
                    break;
            }
        }
    }
}