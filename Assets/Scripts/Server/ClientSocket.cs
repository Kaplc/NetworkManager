using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using Network.Base;
using Network.ProtocolClass;
using UnityEngine;
using Timer = System.Timers.Timer;

namespace Script.NetworkManager
{
    public class ClientSocket
    {
        public string guid;
        private bool isClose;
        public Socket socket;
        private Server server;
        public IPEndPoint ipEndPoint;
        public ENetworkType type;
        private MessagePool messagePool;

        #region handle package

        private byte[] handlePackageCacheBuffer = new byte[1024 * 1024]; // 分包粘包处理
        private int cacheIndex = 0;

        #endregion

        #region heart message

        private int heartTime = 0;
        private int outTime = 3;
        private Timer timer;

        #endregion

        #region async

        private byte[] asyncCacheBuffer = new byte[1024 * 1024];

        #endregion
        
        public ClientSocket(Server server, Socket socket, MessagePool messagePool)
        {
            type = ENetworkType.TcpV4;
            
            guid = Guid.NewGuid().ToString();
            this.socket = socket;
            this.server = server;
            this.messagePool = messagePool;

            // 创建一个定时器对象，设置时间间隔为1秒
            timer = new Timer(1000);

            // 定义定时器的Elapsed事件处理方法
            timer.Elapsed += (obj, args) =>
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
            // timer.Start();
        }
        
        // new udp client
        public ClientSocket(Server server, IPEndPoint ipEndPoint, MessagePool messagePool)
        {
            type = ENetworkType.UdpV4;
            this.server = server;
            this.ipEndPoint = ipEndPoint;
            this.messagePool = messagePool;
        }

        public void Close()
        {
            isClose = true;
            socket.Disconnect(false);
            socket.Close();
            Debug.Log("remove client socket at " + guid);
        }

        #region sync

        public void Send(BaseNetworkData data)
        {
            if (type == ENetworkType.TcpV4 || type == ENetworkType.TcpV6)
            {
                socket.Send(data.Serialize());
            }
        }

        public void Receive()
        {
            if (type == ENetworkType.TcpV4 || type == ENetworkType.TcpV6)
            {
                if (socket.Available > 0)
                {
                    byte[] bytes = new byte[socket.Available];
                    socket.Receive(bytes);

                    HandlePackage(bytes, bytes.Length);
                    // HandlerProtobufThread(bytes);
                }
            }
        }

        #endregion

        #region api async

        public void ReceiveAsync()
        {
            try
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                // third args is receive data max length 
                args.SetBuffer(asyncCacheBuffer, 0, asyncCacheBuffer.Length);
                args.Completed += (sender, eventArgs) =>
                {
                    if (args.SocketError == SocketError.Success)
                    {
                        HandlePackage(args.Buffer, args.BytesTransferred);
                        // receive next data
                    }
                    else
                    {
                        Debug.LogWarning("receive fail " + args.SocketError);
                    }

                    if (!isClose)
                    {
                        socket.ReceiveAsync(args);
                    } 
                    
                };

                if (!isClose)
                {
                    socket.ReceiveAsync(args);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("receive error");
                Debug.LogError(e);
                throw;
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
                // ThreadPool.QueueUserWorkItem(HandlerProtobufThread, messageBuffer);

                if (cacheIndex >  messageLength + readIndex)
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
            int id = BitConverter.ToInt32((byte[])data, 0);
            // use message pool to handle
            BaseMessage message = messagePool.GetMessage(id).Deserialize<BaseMessage>((byte[])data, 8);
            BaseHandler handler = messagePool.GetHandler(id);
            handler.message = message;
            handler.Handle();
        }

        private void HandlerProtobufThread(object data)
        {
            Protobuf.HeartMessage heartMessage = new Protobuf.HeartMessage();
            heartMessage = Protobuf.HeartMessage.Parser.ParseFrom((byte[])data);
            Debug.Log("heart " + heartMessage.Id);
        }
        #endregion
    }
}