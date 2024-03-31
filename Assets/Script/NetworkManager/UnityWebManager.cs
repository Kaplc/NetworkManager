using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Script.NetworkManager
{
    public class UnityWebManager : MonoBehaviour
    {
        private static UnityWebManager instance;
        public static UnityWebManager Instance => instance;

        private void Awake()
        {
            instance = this;
        }

        public void UploadFile(string url, string localFile, string remoteFileName, UnityAction<bool> callBack)
        {
            StartCoroutine(UploadFileCoroutine(url, localFile, remoteFileName, callBack));
        }

        private IEnumerator UploadFileCoroutine(string url, string localFile, string remoteFileName, UnityAction<bool> callBack)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            // 
            if (!File.Exists(localFile))
            {
                Debug.LogError("local file no exist: " + localFile);
                yield break;
            }

            // add file to form data
            formData.Add(new MultipartFormFileSection(remoteFileName, File.ReadAllBytes(localFile)));
            // create request
            UnityWebRequest request = UnityWebRequest.Post(url, formData);

            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                callBack?.Invoke(true);
            }
            else
            {
                Debug.LogError("upload file error: " + request.responseCode + request.error + request.result);
                callBack?.Invoke(false);
            }
            
            request.Dispose();
        }

        #region put

        public void UploadFilePut(string url, string localFile, string remoteFileName, UnityAction<bool> callBack)
        {
            StartCoroutine(UploadFilePutCoroutine(url, localFile, remoteFileName, callBack));
        }

        private IEnumerator UploadFilePutCoroutine(string url, string localFile, string remoteFileName, UnityAction<bool> callBack)
        {
            if (!File.Exists(localFile))
            {
                Debug.LogError("local file no exist: " + localFile);
                yield break;
            }
            
            // create request
            UnityWebRequest request = UnityWebRequest.Put(url + remoteFileName, File.ReadAllBytes(localFile));

            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                callBack?.Invoke(true);
            }
            else
            {
                Debug.LogError("upload file error: " + request.responseCode + request.error + request.result);
                callBack?.Invoke(false);
            }
            
            request.Dispose();
        }

        #endregion

        public void Download<T>(string url, UnityAction<T> callBack) where T : class
        {
            StartCoroutine(DownloadCoroutine<T>(url, callBack));
        }

        private IEnumerator DownloadCoroutine<T>(string url, UnityAction<T> callBack) where T : class
        {
            Type type = typeof(T);

            UnityWebRequest request;
            
            if (type == typeof(Texture))
            {
                request = UnityWebRequestTexture.GetTexture(url);
            }
            else if (type == typeof(AudioClip))
            {
                request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            }
            else if (type == typeof(string))
            { 
                request = UnityWebRequest.Get(url);
            }
            else if (type == typeof(byte[]))
            {
               request = UnityWebRequest.Get(url);
            }
            else if (type == typeof(AssetBundle))
            {
                request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            }
            else
            {
                request = UnityWebRequest.Get(url);
            }
            

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                
                if (type == typeof(Texture))
                {
                    callBack?.Invoke(DownloadHandlerTexture.GetContent(request) as T);
                }
                else if (type == typeof(AudioClip))
                {
                    callBack?.Invoke(DownloadHandlerAudioClip.GetContent(request) as T);
                }
                else if (type == typeof(string))
                {
                    callBack?.Invoke(request.downloadHandler.text as T);
                }
                else if (type == typeof(byte[]))
                {
                    callBack?.Invoke(request.downloadHandler.data as T);
                }
                else if (type == typeof(AssetBundle))
                {
                    callBack?.Invoke(DownloadHandlerAssetBundle.GetContent(request) as T);
                }
            }
            else
            {
                Debug.LogError("download file error: " + request.error);
                callBack?.Invoke(null);
            }
            
            request.Dispose();
        }
    }
}