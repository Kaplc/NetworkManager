using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Script.NetworkManager;
using UnityEngine;
using Timer = System.Timers.Timer;

public class NetworkManager : MonoBehaviour
{
    #region socket args

    public string serverIp;
    public int serverPort = 8800;
    public bool isConnect;

    private ENetworkType networkType;
    public Socket socket;

    #region tcp

    private IPEndPoint serverIPEndPoint;

    #endregion

    #region udp

    private int udpCacheBytesLength = 1024 * 1024;

    private IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
    private IPEndPoint sendToIPEndPoint;
    private EndPoint receiveFromEndPoint = new IPEndPoint(IPAddress.Any, 0);

    #endregion

    #endregion

    #region container

    private Queue<BaseNetworkData> sendMessageQueue = new();
    public Queue<BaseNetworkData> receiveMessageQueue = new();

    #endregion

    #region package handle

    private byte[] handlePackageCacheBuffer = new byte[1024 * 1024]; // 分包粘包处理
    private int cacheIndex;

    #endregion

    #region heart

    private Timer timer;

    #endregion

    #region sync
    
    

    #endregion

    #region async

    private byte[] asyncReceiveCacheBuffer = new byte[1024 * 1024];

    #endregion


    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        timer = new Timer(1000);
        timer.Elapsed += (obj, args) =>
        {
            HeartMessage heartMessage = new HeartMessage();
            socket.Send(heartMessage.Serialize());
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (receiveMessageQueue.Count > 0)
        {
            BaseNetworkData data = receiveMessageQueue.Dequeue();
            switch (data)
            {
                case TextMessage:

                    break;
                default:
                    Debug.Log($"server: {data}");
                    break;
            }
        }
    }

    #region async

    public void ConnectAsync(string ip, ENetworkType clientType)
    {
        if (isConnect || string.IsNullOrEmpty(ip))
        {
            return;
        }

        try
        {
            switch (clientType)
            {
                case ENetworkType.TcpV4:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    break;
                case ENetworkType.TcpV6:
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    break;
                case ENetworkType.UdpV4:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    break;
                case ENetworkType.UdpV6:
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    break;
            }

            serverIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), serverPort);
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
                    receiveArgs.Completed += (sender, args) =>
                    {
                        // receive message success
                        if (args.SocketError == SocketError.Success)
                        {
                            HandlePackage(args.Buffer, args.BytesTransferred);
                        }
                        else
                        {
                            Debug.LogWarning("receive message fail: " + args.SocketError);
                        }

                        // continue receive
                        socket.ReceiveAsync(args);
                    };
                    // start async receive
                    socket.ReceiveAsync(receiveArgs);

                    // start heart message timer
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

    public void SendAsync(BaseNetworkData data)
    {
        byte[] bytes = data.Serialize();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        sendArgs.SetBuffer(bytes, 0, bytes.Length);
        socket.SendAsync(sendArgs);
    }

    public void SendAsync(byte[] bytes)
    {
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        sendArgs.SetBuffer(bytes, 0, bytes.Length);
        socket.SendAsync(sendArgs);
    }

    public void SendToAsync(BaseNetworkData data)
    {
        
    }

    #endregion

    #region sync

    public bool Connect(string ip, ENetworkType type)
    {
        if (isConnect || string.IsNullOrEmpty(ip))
        {
            return false;
        }

        try
        {
            serverIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), serverPort);
            networkType = type;
            switch (networkType)
            {
                case ENetworkType.TcpV4:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(serverIPEndPoint);
                    break;
                case ENetworkType.TcpV6:
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(serverIPEndPoint);
                    break;
                case ENetworkType.UdpV4:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(localIPEndPoint);
                    break;
                case ENetworkType.UdpV6:
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(localIPEndPoint);
                    break;
            }


            isConnect = true;

            Debug.Log("connect success");
            // new thread check queue then send message
            ThreadPool.QueueUserWorkItem(SendMessageThread);
            ThreadPool.QueueUserWorkItem(ReceiveMessageThread);

            // send heart message
            // timer.Start();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"server connect fail");
            Debug.LogError(e);
            isConnect = false;
            throw;
        }
    }

    public void Send(BaseNetworkData data)
    {
        sendMessageQueue.Enqueue(data);
    }

    public void SendTo(BaseNetworkData data, IPEndPoint ipEndPoint)
    {
        sendToIPEndPoint = ipEndPoint;
        sendMessageQueue.Enqueue(data);
    }

    #region thread

    private void SendMessageThread(object args)
    {
        while (isConnect)
        {
            if (sendMessageQueue.Count > 0)
            {
                byte[] bytes = sendMessageQueue.Dequeue().Serialize();

                switch (networkType)
                {
                    case ENetworkType.TcpV4:
                    case ENetworkType.TcpV6:
                        socket.Send(bytes);
                        break;
                    case ENetworkType.UdpV4:
                    case ENetworkType.UdpV6:
                        socket.SendTo(bytes, sendToIPEndPoint);
                        break;
                }
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
                switch (networkType)
                {
                    case ENetworkType.TcpV4:
                    case ENetworkType.TcpV6:
                        byte[] bytes = new byte[socket.Available];
                        socket.Receive(bytes);
                        HandlePackage(bytes, bytes.Length);
                        break;
                    case ENetworkType.UdpV4:
                    case ENetworkType.UdpV6:
                        byte[] udpBytes = new byte[udpCacheBytesLength];
                        socket.ReceiveFrom(udpBytes, ref receiveFromEndPoint);
                        ThreadPool.QueueUserWorkItem(HandlerThread, udpBytes);
                        break;
                }
            }
        }
    }

    #endregion

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
                // move index to next message data start index
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


    public void Disconnect()
    {
        QuitMessage quitMessage = new QuitMessage();
        socket.Send(quitMessage.Serialize());
        isConnect = false;
        timer.Close();
        socket.Disconnect(false);
        socket.Close();
    }
}