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

    private IPEndPoint ipEndPoint;
    public ENetworkType clientType;
    public Socket socket;

    #endregion
    
    #region container

    private Queue<BaseNetworkData> sendMessageQueue = new();
    public Queue<BaseNetworkData> receiveMessageQueue = new();

    #endregion

    #region package heandle

    private byte[] cacheBuffer = new byte[1024 * 1024]; // 分包粘包处理
    private int cacheIndex = 0;

    #endregion
    
    #region heart

    private Timer timer;

    #endregion

    #region sync

    private byte[] syncCacheBuffer = new byte[1024 * 1024];

    #endregion
    
    #region async
    
    private byte[] asyncCacheBuffer = new byte[1024 * 1024]; 

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

            ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), serverPort);
            // async connect
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
            connectArgs.RemoteEndPoint = ipEndPoint;
            connectArgs.Completed += (sender, args) =>
            {
                if (args.SocketError == SocketError.Success)
                {
                    isConnect = true;
                    Debug.Log("connect success");
                    
                    // set args
                    SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                    receiveArgs.SetBuffer(asyncCacheBuffer, 0, asyncCacheBuffer.Length);
                    receiveArgs.Completed += ReceiveAsyncCallBack;
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
        }
        catch (Exception e)
        {
            Debug.LogError($"server connect fail");
            Debug.LogError(e);
            isConnect = false;
            throw;
        }
    }

    private void ReceiveAsyncCallBack(object socket, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            HandlePackage(args.Buffer);
        }
        else
        {
              Debug.LogError("receive message fail");   
        }
    }

    #endregion

    #region sync

    public bool Connect(string ip, ENetworkType clientType)
    {
        if (isConnect || string.IsNullOrEmpty(ip))
        {
            return false;
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

            ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), serverPort);

            socket.Connect(ipEndPoint);
            isConnect = true;

            Debug.Log("connect success");
            // new thread check queue then send message
            ThreadPool.QueueUserWorkItem(SendMessageThread);
            ThreadPool.QueueUserWorkItem(ReceiveMessageThread);
            
            // heart message
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
                
                HandlePackage(bytes);
            }
        }
    }

    #endregion

    #region handle data

    private void HandlePackage(byte[] bytes)
    {
        int id;
        int length = 0;

        bytes.CopyTo(cacheBuffer, cacheIndex);
        cacheIndex += bytes.Length;
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
            case 123:
                TextMessage textMessage = new TextMessage().Deserialize<TextMessage>((byte[])state, 8);
                Debug.Log($"client: {textMessage.text}");
                break;
        }
    }

    #endregion
    
    
    public void Send(BaseNetworkData data)
    {
        sendMessageQueue.Enqueue(data);
    }

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