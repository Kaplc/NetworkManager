using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Script.NetworkManager
{
    public class HttpManager
    {
        public async void DownloadFile(string remoteFileName, string localFileName, UnityAction<HttpStatusCode> callBack)
        {
            await Task.Run(() =>
            {
                try
                {
                    // check exist remote file
                    HttpWebRequest request = HttpWebRequest.CreateHttp(remoteFileName);
                    request.Method = WebRequestMethods.Http.Head;
                    request.Timeout = 5000; // set timeout 5s
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        response.Close();
                        // start download file
                        request = HttpWebRequest.CreateHttp(remoteFileName);
                        request.Method = WebRequestMethods.Http.Get;
                        request.Timeout = 5000; // set timeout 5s
                        response = request.GetResponse() as HttpWebResponse;
                        using (var responseStream = response.GetResponseStream())
                        {
                            using (var fileStream = System.IO.File.Create(localFileName))
                            {
                                responseStream.CopyTo(fileStream);
                                fileStream.Close();
                            }

                            responseStream.Close();
                        }

                        response.Close();
                        callBack?.Invoke(HttpStatusCode.OK);
                    }
                    else
                    {
                        Debug.Log("remote file no exist");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("download file error");
                    Debug.LogError(e);
                    throw;
                }
            });
        }

        public async void UploadFile(string remoteFileName, string localFileName, UnityAction<HttpStatusCode> callBack)
        {
            await Task.Run(() =>
            {
                try
                {
                        // start upload file
                        HttpWebRequest request = HttpWebRequest.CreateHttp(remoteFileName);
                        request.Method = WebRequestMethods.Http.Post;
                        request.Timeout = 5000; // set timeout 5s
                        request.ContentType = "multipart/form-data; charset=utf-8";
                        request.Credentials = new NetworkCredential("kaplc", "123456");
                        request.PreAuthenticate = true;
                        request.Proxy = null!;

                        string contentHead = $"\r\n--" + "kaplc" + "\r\n" +
                                             $"Content-Disposition: form-data; name=\"file\"; filename=\"{localFileName}\"\r\n" +
                                             $"Content-Type:application/octet-stream\r\n\r\n";
                        byte[] contentHeadBytes = Encoding.UTF8.GetBytes(contentHead);

                        string contentEnd = "\r\n--" + "kaplc" + "--\r\n";
                        byte[] contentEndBytes = Encoding.UTF8.GetBytes(contentEnd);
                        
                        using (var fileStream = File.OpenRead(localFileName))
                        {
                            request.ContentLength = contentHeadBytes.Length + fileStream.Length + contentEndBytes.Length;
                            
                            // start upload file
                            using (var requestStream = request.GetRequestStream())
                            {
                                requestStream.Write(contentHeadBytes, 0, contentHeadBytes.Length);
                                fileStream.CopyTo(requestStream);
                                requestStream.Write(contentEndBytes, 0, contentEndBytes.Length);

                                fileStream.Close();
                                requestStream.Close();
                            }
                        }
                        
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        callBack?.Invoke(response.StatusCode);
                        response.Close();
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