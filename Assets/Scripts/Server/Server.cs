using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Network.Base;
using Network.ProtocolClass;
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
    private MessagePool messagePool;

    [Header("广播消息")] public string broadcastInfo;
    public bool send;

    public Queue<ClientSocket> willRemoveClientSockets = new Queue<ClientSocket>();


    // Start is called before the first frame update
    public void Start(string ip, ENetworkType type)
    {
        // bind server ip
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 8800);

        switch (type)
        {
            case ENetworkType.TcpV4:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipEndPoint);
                // max client count
                serverSocket.Listen(maxClientCount);
                ThreadPool.QueueUserWorkItem(AcceptThread);
                ThreadPool.QueueUserWorkItem(RemoveClientSocket);
                break;
            case ENetworkType.TcpV6:
                serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(maxClientCount);
                ThreadPool.QueueUserWorkItem(AcceptThread);
                ThreadPool.QueueUserWorkItem(RemoveClientSocket);
                break;
            case ENetworkType.UdpV4:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                serverSocket.Bind(ipEndPoint);
                break;
            case ENetworkType.UdpV6:
                serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                serverSocket.Bind(ipEndPoint);
                break;
        }


        // thread async
        ThreadPool.QueueUserWorkItem(ReceiveThread);

        // async accept
        // ServerAcceptAsync();
        
        // 
        messagePool = new MessagePool();

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

    #region sync

    private void AcceptThread(object state)
    {
        while (!isClose)
        {
            try
            {
                // Accept() is blocking func
                Socket client = serverSocket.Accept();
                // 
                ClientSocket clientSocket = new ClientSocket(this, client, messagePool);
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

                // udp receive
                if (serverSocket.Available > 0)
                {
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] bytes = new byte[1024 * 1024];
                    serverSocket.ReceiveFrom(bytes, ref endPoint);
                    IPEndPoint ipEndPoint = endPoint as IPEndPoint;
                    lock (clientSocketsDic)
                    {
                        if (!clientSocketsDic.ContainsKey(ipEndPoint.Address + ":" + ipEndPoint.Port))
                        {
                            // save client
                            ClientSocket clientSocket = new ClientSocket(this, endPoint as IPEndPoint, messagePool);
                            // key is ip+port
                            clientSocketsDic.Add(ipEndPoint.Address + ":" + ipEndPoint.Port, clientSocket);
                        }
                    }

                    HandlerUdpData(bytes);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"receive fail");
                Debug.LogError(e);
            }
        }
    }

    #endregion

    #region async

    private void ServerAcceptAsync()
    {
        try
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += (sender, eventArgs) =>
            {
                if (args.SocketError == SocketError.Success)
                {
                    // add client
                    ClientSocket clientSocket = new ClientSocket(this, args.AcceptSocket, messagePool);
                    lock (clientSocketsDic)
                    {
                        clientSocketsDic.Add(clientSocket.guid, clientSocket);
                        Debug.Log($"accept client {clientSocket.guid} success");
                        // btnStart client socket async receive
                        clientSocket.ReceiveAsync();
                    }
                }
                else
                {
                    Debug.LogWarning("accept fail " + args.SocketError);
                }

                // next accept
                serverSocket.AcceptAsync(args);
            };

            if (!isClose)
            {
                serverSocket?.AcceptAsync(args);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("accept error");
            Debug.LogError(e);
            throw;
        }
    }

    #endregion

    #region handle

    private void HandlerUdpData(object data)
    {
        int id = BitConverter.ToInt32((byte[])data, 0);
        // use message pool to handle
        BaseMessage message = messagePool.GetMessage(id).Deserialize<BaseMessage>((byte[])data, 8);
        BaseHandler handler = messagePool.GetHandler(id);
        handler.message = message;
        handler.Handle();
    }

    #endregion

    public void Broadcast(BaseNetworkData data)
    {
        lock (clientSocketsDic)
        {
            foreach (ClientSocket clientSocket in clientSocketsDic.Values)
            {
                if (clientSocket.type == ENetworkType.TcpV4 || clientSocket.type == ENetworkType.TcpV6)
                {
                    clientSocket.Send(data);
                }
                else
                {
                    serverSocket.SendTo(data.Serialize(), clientSocket.ipEndPoint);
                }
            }
        }
    }

    public void Close()
    {
        lock (clientSocketsDic)
        {
            foreach (ClientSocket clientSocket in clientSocketsDic.Values)
            {
                clientSocket.socket?.Disconnect(false);
                clientSocket.socket?.Close();
            }
        }

        serverSocket.Close();
        isClose = true;
    }
}