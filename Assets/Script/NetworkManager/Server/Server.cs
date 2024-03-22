using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Script.NetworkManager;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public enum ENetworkType
{
    TcpV4,
    TcpV6,
    UdpV4,
    UdpV6,
}

public class Server
{
    public string ip = "127.0.0.1";
    public int port = 8800;
    public int maxClientCount = 999;
    
    public ENetworkType eNetworkType = ENetworkType.TcpV4;
    private Socket serverSocket;
    private Dictionary<string, ClientSocket> clientSocketsDic = new Dictionary<string, ClientSocket>();
    private bool isClose;

    [Header("广播消息")] public string broadcastInfo;
    public bool send;

    // Start is called before the first frame update
    public void Start()
    {
        switch (eNetworkType)
        {
            case ENetworkType.TcpV4:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                break;
            case ENetworkType.TcpV6:
                serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                break;
            case ENetworkType.UdpV4:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                break;
            case ENetworkType.UdpV6:
                serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                break;
        }
        
        // bind server ip
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        serverSocket.Bind(ipEndPoint);
        // max client count
        serverSocket.Listen(maxClientCount);

        ThreadPool.QueueUserWorkItem(AcceptThread);
        ThreadPool.QueueUserWorkItem(ReceiveThread);
    }

    private void AcceptThread(object state)
    {
        while (!isClose)
        {
            try
            {
                // Accept() is blocking func
                Socket client = serverSocket.Accept();
                //
                ClientSocket clientSocket = new ClientSocket(client);
                lock (clientSocketsDic)
                {
                    clientSocketsDic.Add(clientSocket.guid, clientSocket);
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"accept error");
                Debug.LogError(e);
                throw;
            }
        }
    }

    private void ReceiveThread(object state)
    {
        while (!isClose)
        {
            try
            {
                lock (clientSocketsDic)
                {
                    foreach (ClientSocket clientSocket in clientSocketsDic.Values)
                    {
                        clientSocket.Receive();
                    }
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"receive fail");
                Debug.LogError(e);
                throw;
            }
        }
    }

    private void Broadcast()
    {
        foreach (ClientSocket clientSocket in clientSocketsDic.Values)
        {
            clientSocket.Send(broadcastInfo);
        }
    }

    private void OnDestroy()
    {
        isClose = true;
    }
}