using System;
using System.Net;
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
                        request.ContentType = "application/octet-stream; charset=utf-8";
                        request.Credentials = new NetworkCredential("kaplc", "123456");
                        request.PreAuthenticate = true;

                        string contentHead = "Content-Disposition: form-data; name=\"file\"; filename=\"" + localFileName + "\"\r\n";
                        byte[] contentHeadBytes = System.Text.Encoding.UTF8.GetBytes(contentHead);

                        string contentEnd = "\r\n--" + "kaplc" + "--\r\n";
                        byte[] contentEndBytes = System.Text.Encoding.UTF8.GetBytes(contentEnd);
                        
                        using (var fileStream = System.IO.File.OpenRead(localFileName))
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

                        callBack?.Invoke(HttpStatusCode.OK);
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