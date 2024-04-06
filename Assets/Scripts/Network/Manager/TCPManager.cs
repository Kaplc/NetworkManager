using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Network.Base;
using Network.ProtocolClass;
using UnityEngine;
using Timer = System.Timers.Timer;

namespace Network.Manager
{
    public class TcpManager
    {
        private bool isConnect;
        private IPEndPoint serverIPEndPoint;
        private ENetworkType networkType;
        private EAsyncType asyncType;
        private Socket socket;
        private MessagePool messagePool;

        #region container

        private Queue<BaseNetworkData> sendMessageQueue = new();
        public Queue<BaseNetworkData> receiveMessageQueue = new();

        #endregion
        
        #region async

        private byte[] asyncReceiveCacheBuffer = new byte[1024 * 1024];

        #endregion

        #region package handle

        private byte[] handlePackageCacheBuffer = new byte[1024 * 1024]; // 分包粘包处理
        private int cacheIndex;

        #endregion

        #region heart
        
        private Protobuf.HeartMessage protoHeartMessage = new Protobuf.HeartMessage()
        {
            Id = 99
        };
        private HeartMessage heartMessage = new HeartMessage();
        private Timer timer;

        #endregion

        public TcpManager(ENetworkType type, EAsyncType asyncType, IPEndPoint serverIPEndPoint, MessagePool messagePool)
        {
            this.serverIPEndPoint = serverIPEndPoint;
            this.asyncType = asyncType;
            this.messagePool = messagePool;
            
            switch (type)
            {
                case ENetworkType.TcpV4:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    break;
                case ENetworkType.TcpV6:
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    break;
            }
        }

        public void Connect()
        {
            try
            {
                switch (asyncType)
                {
                    case EAsyncType.Thread:
                        socket.Connect(serverIPEndPoint);
                        Debug.Log("connect success");
                        isConnect = true;
                        // new thread check queue then send message
                        ThreadPool.QueueUserWorkItem(SendMessageThread);
                        ThreadPool.QueueUserWorkItem(ReceiveMessageThread);
                        break;
                    case EAsyncType.AsyncFunc:
                        ConnectAsync();
                        break;
                }
                
                // send heart message timer
                timer = new Timer(1000);
                timer.Elapsed += (obj, args) =>
                {
                    socket.Send(heartMessage.Serialize());
                    // socket.Send(protoHeartMessage.ToByteArray());
                };
                timer.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"server connect fail");
                Debug.LogError(e);
                isConnect = false;
                throw;
            }
        }

        #region SocketFuncAsync

        private void ConnectAsync()
        {
            try
            {
                // async connect
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.RemoteEndPoint = serverIPEndPoint;
                args.Completed += (sender, args) =>
                {
                    if (args.SocketError == SocketError.Success)
                    {
                        isConnect = true;
                        Debug.Log("connect success");

                        // set args
                        SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                        receiveArgs.SetBuffer(asyncReceiveCacheBuffer, 0, asyncReceiveCacheBuffer.Length);
                        receiveArgs.Completed += (_, receiveArgs) =>
                        {
                            if (!isConnect)
                            {
                                return;
                            }
                            // receive message success
                            if (receiveArgs.SocketError == SocketError.Success)
                            {
                                HandlePackage(receiveArgs.Buffer, receiveArgs.BytesTransferred);
                            }
                            else
                            {
                                Debug.LogWarning("receive message fail: " + receiveArgs.SocketError);
                            }

                            // continue receive
                            socket.ReceiveAsync(receiveArgs);
                        };
                        // btnStart async receive
                        socket.ReceiveAsync(receiveArgs);

                        // btnStart heart message timer
                        timer.Start();
                    }
                    else
                    {
                        Debug.LogError($"server connect fail");
                        isConnect = false;
                    }
                };
                socket.ConnectAsync(args);
            }
            catch (Exception e)
            {
                Debug.LogError($"server connect fail");
                Debug.LogError(e);
                isConnect = false;
                throw;
            }
        }

        #endregion

        #region threadAsync

        private void SendMessageThread(object args)
        {
            while (isConnect)
            {
                if (sendMessageQueue.Count > 0)
                {
                    byte[] bytes = sendMessageQueue.Dequeue().Serialize();
                    socket.Send(bytes);
                }
            }
        }

        private void ReceiveMessageThread(object args)
        {
            while (isConnect)
            {
                // will receive message
                if (socket.Available > 0)
                {
                    byte[] bytes = new byte[socket.Available];
                    socket.Receive(bytes);
                    HandlePackage(bytes, bytes.Length);
                }
            }
        }

        #endregion

        #region handle data

        private void HandlePackage(byte[] bytes, int dataLength)
        {
            int readIndex = 0;

            Array.Copy(bytes, 0, handlePackageCacheBuffer, cacheIndex, dataLength);
            cacheIndex += dataLength;
            while (true)
            {
                // length data index is 4
                int messageLength = BitConverter.ToInt32(handlePackageCacheBuffer, readIndex + 4);

                if (cacheIndex < 8 || cacheIndex < messageLength + readIndex)
                {
                    // 被分包继续接收
                    break;
                }

                byte[] messageBuffer = new byte[messageLength];
                Array.Copy(handlePackageCacheBuffer, readIndex, messageBuffer, 0, messageLength);
                ThreadPool.QueueUserWorkItem(HandlerThread, messageBuffer);

                if (cacheIndex > messageLength + readIndex)
                {
                    // move index to next message data btnStart index
                    readIndex += messageLength;
                }
                else
                {
                    // 已经处理完所有数据
                    cacheIndex = 0;
                    break;
                }
            }
        }

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

        public void Send(BaseNetworkData data)
        {
            switch (asyncType)
            {
                case EAsyncType.Thread:
                    sendMessageQueue.Enqueue(data);
                    break;
                case EAsyncType.AsyncFunc:
                    byte[] bytes = data.Serialize();
                    SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                    sendArgs.SetBuffer(bytes, 0, bytes.Length);
                    socket.SendAsync(sendArgs);
                    break;
            }
        }

        public void Disconnect()
        {
            QuitMessage quitMessage = new QuitMessage();
            socket.Send(quitMessage.Serialize());
            isConnect = false;
            timer.Close();
            socket.Disconnect(false);
            socket.Close();
            socket.Dispose();
        }
    }
}