using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Network.Base;
using Network.ProtocolClass;
using UnityEngine;

namespace Network.Manager
{
    public class UdpManager
    {
        private bool isStart;

        private IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
        private IPEndPoint sendToIPEndPoint;
        private EndPoint receiveFromEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private IPEndPoint remoteIPEndPoint;
        private Socket socket;
        private MessagePool messagePool;
        private EAsyncType asyncType;

        private byte[] receiveCacheBuffer = new byte[1024 * 1024];

        #region container

        private Queue<BaseNetworkData> sendMessageQueue = new();

        #endregion

        public UdpManager(ENetworkType type, EAsyncType asyncType, MessagePool messagePool)
        {
            this.asyncType = asyncType;
            this.messagePool = messagePool;
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
            Debug.Log("udp btnStart success");
        }

        public void Close()
        {
            isStart = false;
            socket.Close();
            socket.Dispose();
            socket = null;
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

        private void HandlerThread(object data)
        {
            try
            {
                int id = BitConverter.ToInt32((byte[])data, 0);
                // use message pool to handle
                BaseMessage message = messagePool.GetMessage(id).Deserialize<BaseMessage>((byte[])data, 8);
                BaseHandler handler = messagePool.GetHandler(id);
                handler.message = message;
                handler.Handle();
               
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