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
    public int maxClientCount = 999;
    private Socket serverSocket;
    private Dictionary<string, ClientSocket> clientSocketsDic = new Dictionary<string, ClientSocket>();
    private bool isClose;

    [Header("广播消息")] public string broadcastInfo;
    public bool send;

    public Queue<ClientSocket> willRemoveClientSockets = new Queue<ClientSocket>();


    // Start is called before the first frame update
    public void Start(string ip, ENetworkType type)
    {
        switch (type)
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
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 8800);
        serverSocket.Bind(ipEndPoint);
        // max client count
        serverSocket.Listen(maxClientCount);

        ThreadPool.QueueUserWorkItem(AcceptThread);
        ThreadPool.QueueUserWorkItem(ReceiveThread);
        ThreadPool.QueueUserWorkItem(RemoveClientSocket);

        Debug.Log("server start success at " + ip + ":" + 8800);
    }

    private void RemoveClientSocket(object args)
    {
        while (!isClose)
        {
            if (willRemoveClientSockets.Count > 0)
            {
                ClientSocket clientSocket = willRemoveClientSockets.Dequeue();
                lock (clientSocketsDic)
                {
                    if (clientSocketsDic.ContainsValue(clientSocket))
                    {
                        clientSocketsDic.Remove(clientSocket.guid);
                        clientSocket.Close();
                    }
                }
            }
        }
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
                ClientSocket clientSocket = new ClientSocket(this, client);
                lock (clientSocketsDic)
                {
                    clientSocketsDic.Add(clientSocket.guid, clientSocket);
                    Debug.Log($"accept client {clientSocket.guid} success");
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

    public void Broadcast(BaseNetworkData data)
    {
        lock (clientSocketsDic)
        {
            foreach (ClientSocket clientSocket in clientSocketsDic.Values)
            {
                clientSocket.Send(data);
            }
        }
    }

    public void Close()
    {
        lock (clientSocketsDic)
        {
            foreach (ClientSocket clientSocket in clientSocketsDic.Values)
            {
                clientSocket.socket.Disconnect(false);
                clientSocket.socket.Close();
            }
        }

        serverSocket.Close();
        isClose = true;
    }
}