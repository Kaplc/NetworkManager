using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Script.NetworkManager.UDP
{
    public class UdpManager
    {
        private bool isStart;

        private IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
        private IPEndPoint sendToIPEndPoint;
        private EndPoint receiveFromEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private IPEndPoint remoteIPEndPoint;
        private Socket socket;
        private EAsyncType asyncType;

        private byte[] receiveCacheBuffer = new byte[1024 * 1024];

        #region container

        private Queue<BaseNetworkData> sendMessageQueue = new();

        #endregion

        public UdpManager(ENetworkType type, EAsyncType asyncType)
        {
            this.asyncType = asyncType;
            switch (type)
            {
                case ENetworkType.UdpV4:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    break;
                case ENetworkType.UdpV6:
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    break;
            }

            if (socket != null) socket.Bind(localIPEndPoint);
        }

        public void Start()
        {
            switch (asyncType)
            {
                case EAsyncType.Thread:
                    // new thread check queue then send message
                    ThreadPool.QueueUserWorkItem(SendMessageThread);
                    ThreadPool.QueueUserWorkItem(ReceiveMessageThread);
                    break;
                case EAsyncType.AsyncFunc:
                    ReceiveFromAsync();
                    break;
            }

            isStart = true;
            Debug.Log("udp start success");
        }

        public void Close()
        {
            isStart = false;
        }

        public void SendTo(BaseNetworkData data, IPEndPoint remoteIPEndPoint)
        {
            this.remoteIPEndPoint = remoteIPEndPoint;
            switch (asyncType)
            {
                case EAsyncType.Thread:
                    sendMessageQueue.Enqueue(data);
                    break;
                case EAsyncType.AsyncFunc:
                    SendToAsync(data);
                    break;
            }
        }

        #region AsyncFunc

        private void ReceiveFromAsync()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(receiveCacheBuffer, 0, receiveCacheBuffer.Length);
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            args.Completed += (_, args) =>
            {
                if (!isStart)
                {
                    return;
                }

                if (args.SocketError == SocketError.Success)
                {
                    byte[] bytes = new byte[args.BytesTransferred];
                    Array.Copy(args.Buffer, bytes, args.BytesTransferred);
                    ThreadPool.QueueUserWorkItem(HandlerThread, bytes);
                }
                else
                {
                    Debug.LogWarning("receive fail");
                }

                socket.ReceiveFromAsync(args);
            };
            socket.ReceiveFromAsync(args);
        }

        private void SendToAsync(BaseNetworkData data)
        {
            byte[] bytes = data.Serialize();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);
            args.RemoteEndPoint = remoteIPEndPoint;
            args.Completed += (sender, eventArgs) =>
            {
                if (eventArgs.SocketError == SocketError.Success)
                {
                }
                else
                {
                    Debug.LogWarning("send fail");
                }
            };
            socket.SendToAsync(args);
        }

        #endregion

        #region ThreadAsync

        private void SendMessageThread(object args)
        {
            while (isStart)
            {
                if (sendMessageQueue.Count > 0)
                {
                    byte[] bytes = sendMessageQueue.Dequeue().Serialize();
                    socket.SendTo(bytes, remoteIPEndPoint);
                }
            }
        }

        private void ReceiveMessageThread(object args)
        {
            while (isStart)
            {
                // will receive message
                if (socket.Available > 0)
                {
                    byte[] bytes = new byte[socket.Available];
                    socket.ReceiveFrom(bytes, ref receiveFromEndPoint);
                    ThreadPool.QueueUserWorkItem(HandlerThread, bytes);
                }
            }
        }

        #endregion

        #region handle data

        private void HandlerThread(object state)
        {
            try
            {
                int id = BitConverter.ToInt32((byte[])state, 0);

                switch (id)
                {
                    case 1:
                        MessageTest test = new MessageTest().Deserialize<MessageTest>((byte[])state, 8);
                        Debug.Log($"server: {test.data}");
                        break;
                    case 2:
                        MessageTest2 test2 = new MessageTest2().Deserialize<MessageTest2>((byte[])state, 8);
                        Debug.Log($"server: {test2.data2} {test2.t5.enumTest}");
                        break;
                    case 123:
                        TextMessage textMessage = new TextMessage().Deserialize<TextMessage>((byte[])state, 8);
                        Debug.Log($"server: {textMessage.text}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log("deserialize fail");
                Debug.Log(e);
                throw;
            }
        }

        #endregion
    }
}