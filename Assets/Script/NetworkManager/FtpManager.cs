using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Script.NetworkManager.FTP
{
    public enum EFtpMethod
    {
        UploadFile,
        DownloadFile,
        DeleteFile,
        ListDirectory, // 简略文件列表
        ListDirectoryDetails, // 详细文件列表
        MakeDirectory,
        RemoveDirectory,
        GetFileSize,
    }

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

        private FtpWebRequest GenerateFtpWebRequest(string remoteFile, EFtpMethod method)
        {
            FtpWebRequest request = FtpWebRequest.Create($"ftp://{ftpServerIP}/{remoteFile}") as FtpWebRequest;
            request.Credentials = new NetworkCredential(ftpUserID, ftpPassword); // ftp server credentials
            request.KeepAlive = false; // close the connection after the request is done
            request.UseBinary = true; // binary mode
            request.Proxy = null; // no proxy

            // method
            switch (method)
            {
                case EFtpMethod.UploadFile:
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    break;
                case EFtpMethod.DownloadFile:
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    break;
                case EFtpMethod.DeleteFile:
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                    break;
                case EFtpMethod.ListDirectory:
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    break;
                case EFtpMethod.ListDirectoryDetails:
                    request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    break;
                case EFtpMethod.MakeDirectory:
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    break;
                case EFtpMethod.RemoveDirectory:
                    request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                    break;
                case EFtpMethod.GetFileSize:
                    request.Method = WebRequestMethods.Ftp.GetFileSize;
                    break;
            }

            return request;
        }

        public async void UploadFile(string localFile, string remoteFile, UnityAction callBack = null)
        {
            Debug.Log("start upload " + localFile);
            // Task async upload file
            await Task.Run(() =>
            {
                try
                {
                    FtpWebRequest request = GenerateFtpWebRequest(remoteFile, EFtpMethod.UploadFile);
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

        public async void DownloadFile(string remoteFile, string localFile, UnityAction callBack = null)
        {
            Debug.Log("start download " + remoteFile);
            await Task.Run(() =>
            {
                try
                {
                    FtpWebRequest request = GenerateFtpWebRequest(remoteFile, EFtpMethod.DeleteFile);
                    // start download file
                    FtpWebResponse response = request.GetResponse() as FtpWebResponse;
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        FileStream fileStream = File.Create(localFile);
                        responseStream.CopyTo(fileStream);

                        fileStream.Close();
                        responseStream.Close();
                    }

                    response.Close();
                    callBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("download file error");
                    Debug.LogError(e);
                    throw;
                }
            });
        }

        public async void DeleteFile(string remoteFile, UnityAction callBack = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    FtpWebRequest request = GenerateFtpWebRequest(remoteFile, EFtpMethod.DeleteFile);
                    FtpWebResponse response = request.GetResponse() as FtpWebResponse;
                    response.Close();

                    callBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("delete file fail " + remoteFile);
                    Debug.LogError(e);
                    throw;
                }
            });
        }


        public async void GetFileSize(string remoteFile, UnityAction<long> callBack)
        {
            await Task.Run(() =>
            {
                try
                {
                    FtpWebRequest request = GenerateFtpWebRequest(remoteFile, EFtpMethod.GetFileSize);
                    FtpWebResponse response = request.GetResponse() as FtpWebResponse;

                    callBack?.Invoke(response.ContentLength);
                }
                catch (Exception e)
                {
                    Debug.LogError("get file size fail " + remoteFile);
                    Debug.LogError(e);
                    throw;
                }
            });
        }

        public async void GetFileList(string remoteDirectory, UnityAction<List<string>> callBack, bool isDetail = false)
        {
            await Task.Run(() =>
            {
                try
                {
                    FtpWebRequest request =
                        GenerateFtpWebRequest(remoteDirectory, isDetail ? EFtpMethod.ListDirectoryDetails : EFtpMethod.ListDirectory);
                    FtpWebResponse response = request.GetResponse() as FtpWebResponse;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream())) // Converting to StreamReader can read each line
                    {
                        List<string> fileList = new List<string>();
                        string line = reader.ReadLine();
                        while (!string.IsNullOrEmpty(line))
                        {
                            fileList.Add(line);
                            line = reader.ReadLine();
                        }

                        reader.Close();
                        callBack?.Invoke(fileList);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("get file list fail " + remoteDirectory);
                    Debug.LogError(e);
                    throw;
                }
            });
        }

        public async void CreateDirectory(string remoteDirectory, UnityAction callBack = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    FtpWebRequest request = GenerateFtpWebRequest(remoteDirectory, EFtpMethod.MakeDirectory);
                    FtpWebResponse response = request.GetResponse() as FtpWebResponse;
                    response.Close();

                    callBack?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("create directory fail " + remoteDirectory);
                    Debug.LogError(e);
                    throw;
                }
            });
        }
    }
}