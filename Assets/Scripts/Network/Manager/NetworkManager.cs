using System;
using System.Net;
using Network.ProtocolClass;
using ZFramework;

namespace Network.Manager
{
    public enum EAsyncType
    {
        Thread,
        AsyncFunc,
    }
    public class NetworkManager: BaseMonoAutoSingleton<NetworkManager>
    {
        public TcpManager TcpManager {get; private set;}
        public UdpManager UDPManager {get; private set;}
        public FtpManager FtpManager {get; private set;}
        public HttpManager HttpManager {get; private set;}
        public UnityWebManager UnityWebManager {get; private set;}
        public UnityWWWManager UnityWWWManager {get; private set;}
        private MessagePool messagePool;

        private void Awake()
        {
            messagePool = new MessagePool();
        }

        #region http

        public void InitHttpManager()
        {
            HttpManager = new HttpManager();
        }

        #endregion

        #region ftp

        public void InitFtpManager(string ftpServerIP, string ftpUserID, string ftpPassword)
        {
            FtpManager = new FtpManager(ftpServerIP, ftpUserID, ftpPassword);
        }

        #endregion

        #region tcp
        
        public void InitTcpManager(ENetworkType type, EAsyncType asyncType, IPEndPoint remoteIPEndPoint)
        {
            TcpManager = new TcpManager(type, asyncType, remoteIPEndPoint, messagePool);
        }

        #endregion

        #region udp
        
        public void InitUdpManager(ENetworkType type, EAsyncType asyncType)
        {
            UDPManager = new UdpManager(type, asyncType, messagePool);
        }

        #endregion
        
        public void CloseTcp()
        {
            TcpManager?.Disconnect();
            TcpManager = null;
        }
        
        public void CloseFtp()
        {
            FtpManager = null;
        }
        
        public void CloseUdp()
        {
            UDPManager?.Close();
            UDPManager = null;
        }
    }
}