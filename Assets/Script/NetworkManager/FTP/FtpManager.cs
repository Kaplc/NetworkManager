using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Script.NetworkManager.FTP
{
    public class FtpManager
    {
        private string ftpServerIP;
        private string ftpUserID;
        private string ftpPassword;

        public FtpManager(string ftpServerIP, string ftpUserID, string ftpPassword)
        {
            this.ftpServerIP = ftpServerIP;
            this.ftpUserID = ftpUserID;
            this.ftpPassword = ftpPassword;
        }
        
        public async void UploadFile(string localFile, string remoteFile, UnityAction callBack) 
        {
            // Task async upload file
            await Task.Run(() =>
            {
                try
                {
                    FtpWebRequest request = FtpWebRequest.Create($"ftp://{ftpServerIP}/{remoteFile}") as FtpWebRequest;
                    request.Credentials = new NetworkCredential(ftpUserID, ftpPassword); // ftp server credentials
                    request.KeepAlive = false; // close the connection after the request is done
                    request.UseBinary = true; // binary mode
                    request.Method = WebRequestMethods.Ftp.UploadFile; // upload file
                    request.Proxy = null; // no proxy
                    
                    // start upload file
                    Stream requestStream = request.GetRequestStream();
                    FileStream fileStream = File.OpenRead(localFile);
                    fileStream.CopyTo(requestStream); // copy file to request stream
                    requestStream.Close();
                    fileStream.Close();
                    
                    callBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("upload file error");
                    Debug.LogError(e);
                    throw;
                }
            });
        }
    }
}