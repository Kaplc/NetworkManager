using System.Net;
using Script.NetworkManager.TCP;
using Script.NetworkManager.UDP;
using UnityEngine;

namespace Script.NetworkManager
{
    public enum EAsyncType
    {
        Thread,
        AsyncFunc,
    }
    public class NetworkManager
    {
        private TcpManager tcpManager;
        private UdpManager udpManager;


        public NetworkManager(ENetworkType type, EAsyncType asyncType, IPEndPoint remoteIPEndPoint = null)
        {
            switch (type)
            {
                case ENetworkType.TcpV4:
                    tcpManager = new TcpManager(ENetworkType.TcpV4, asyncType, remoteIPEndPoint);
                    break;
                case ENetworkType.TcpV6:
                    tcpManager = new TcpManager(ENetworkType.TcpV6, asyncType, remoteIPEndPoint);
                    break;
                case ENetworkType.UdpV4:
                    udpManager = new UdpManager(ENetworkType.UdpV4, asyncType);
                    break;
                case ENetworkType.UdpV6:
                    udpManager = new UdpManager(ENetworkType.UdpV6, asyncType);
                    break;
            }
        }

        #region tcp

        public void Connect()
        {
            if (tcpManager != null)
            {
                tcpManager.Connect();
            }
            else
            {
                Debug.LogError("udp manager have not connect method");
            }
        }

        public void Disconnect()
        {
            tcpManager?.Disconnect();
            udpManager?.Close();
        }

        public void Send(BaseNetworkData data)
        {
            tcpManager.Send(data);
        }

        #endregion

        #region udp
        
        public void Start()
        {
            udpManager?.Start();
        }

        public void SendTo(BaseNetworkData data, IPEndPoint ipEndPoint)
        {
            udpManager.SendTo(data, ipEndPoint);
        }

        #endregion
    }
}