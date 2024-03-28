using System.Net;
using Script.NetworkManager.FTP;
using Script.NetworkManager.TCP;
using Script.NetworkManager.UDP;
using UnityEngine;
using UnityEngine.Events;

namespace Script.NetworkManager
{
    public enum EAsyncType
    {
        Thread,
        AsyncFunc,
    }
    public class NetworkManager
    {
        public TcpManager tcpManager;
        public UdpManager udpManager;
        public FtpManager ftpManager;

        #region ftp

        public void InitFtpManager(string ftpServerIP, string ftpUserID, string ftpPassword)
        {
            ftpManager = new FtpManager(ftpServerIP, ftpUserID, ftpPassword);
        }
        
        public void UploadFile(string localFile, string remoteFile, UnityAction callBack = null)
        {
            if (ftpManager == null)
            {
                Debug.LogError("ftp manager is null");
                return;
            }
            
            ftpManager.UploadFile(localFile, remoteFile, callBack);
        }

        #endregion
        

        #region tcp
        
        public void InitTcpManager(ENetworkType type, EAsyncType asyncType, IPEndPoint remoteIPEndPoint)
        {
            tcpManager = new TcpManager(type, asyncType, remoteIPEndPoint);
        }

        #endregion

        #region udp
        
        public void InitUdpManager(ENetworkType type, EAsyncType asyncType)
        {
            udpManager = new UdpManager(type, asyncType);
        }

        #endregion
        
        public void CloseTcp()
        {
            tcpManager?.Disconnect();
            tcpManager = null;
        }
        
        public void CloseFtp()
        {
            ftpManager = null;
        }
        
        public void CloseUdp()
        {
            udpManager?.Close();
            udpManager = null;
        }
    }
}