using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Script.NetworkManager;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public string serverIp;
    public int serverPort;
    public bool isConnect;

    private IPEndPoint ipEndPoint;
    public ENetworkType clientType;
    public Socket socket;

    private Queue<BaseNetworkData> sendMessageQueue = new();
    private Queue<BaseNetworkData> receiveMessageQueue = new();

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

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

        ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
    }

    // Update is called once per frame
    void Update()
    {
        if (receiveMessageQueue.Count > 0)
        {
            Debug.Log($"server: {receiveMessageQueue.Dequeue()}");
        }
    }

    public void Connect()
    {
        if (isConnect)
        {
            return;
        }

        try
        {
            socket.Connect(ipEndPoint);
            isConnect = true;

            Debug.Log("connect success");
            // new thread check queue then send message
            // ThreadPool.QueueUserWorkItem(SendMessageThread);
            // ThreadPool.QueueUserWorkItem(ReceiveMessageThread);
            StartCoroutine(SendMessageThread());
            StartCoroutine(ReceiveMessageThread());
        }
        catch (Exception e)
        {
            Debug.LogError($"server connect fail");
            Debug.LogError(e);
            throw;
        }
    }

    private IEnumerator SendMessageThread()
    {
        while (isConnect)
        {
            if (sendMessageQueue.Count > 0)
            {
                byte[] bytes = sendMessageQueue.Dequeue().Serialize();
                socket.Send(bytes);
            }
            
            yield return null;
        }
    }

    private IEnumerator ReceiveMessageThread()
    {
        while (isConnect)
        {
            // will receive message
            if (socket.Available > 0)
            {
                byte[] bytes = new byte[socket.ReceiveBufferSize];
                socket.Receive(bytes);
                // get id
                int id = BitConverter.ToInt32(bytes, 0);
                int length = BitConverter.ToInt32(bytes, 4);
                switch (id)
                {
                    case 1:
                        receiveMessageQueue.Enqueue(new MessageTest().Deserialize<MessageTest>(bytes, 8));
                        break;
                }
                
            }

            yield return null;
        }
    }

    public void Send(BaseNetworkData data)
    {
        sendMessageQueue.Enqueue(data);
    }

    public void Disconnect()
    {
        isConnect = false;
    }
}