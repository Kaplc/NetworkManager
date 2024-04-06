using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable CS0618

namespace Network.Manager
{
    public class UnityWWWManager : MonoBehaviour
    {
        public static UnityWWWManager instance;
        public static UnityWWWManager Instance => instance;

        private void Awake()
        {
            instance = this;
        }

        public void WWWDownload<T>(string url, UnityAction<T> callBack) where T : class
        {
            StartCoroutine(WWWDownloadCoroutine<T>(url, callBack));
        }

        private IEnumerator WWWDownloadCoroutine<T>(string url, UnityAction<T> callBack) where T : class
        {
            WWW www = new WWW(url);

            yield return www;

            if (www.error is null)
            {
                Type type = typeof(T);

                if (type == typeof(AssetBundle))
                {
                    callBack?.Invoke(www.assetBundle as T);
                }
                else if (type == typeof(Texture))
                {
                    callBack?.Invoke(www.texture as T);
                }
                else if (type == typeof(AudioClip))
                {
                    callBack?.Invoke(www.GetAudioClip() as T);
                }
                else if (type == typeof(string))
                {
                    callBack?.Invoke(www.text as T);
                }
                else if (type == typeof(byte[]))
                {
                    callBack?.Invoke(www.bytes as T);
                }
            }
            else
            {
                Debug.LogError("www load error: " + www.error);
            }
        }

        public void WWWFormUploadFile(string url, string localFileName, UnityAction<bool> callBack)
        {
            StartCoroutine(WWWFormUploadCoroutine(url, localFileName, callBack));
        }

        private IEnumerator WWWFormUploadCoroutine(string url, string localFileName, UnityAction<bool> callBack)
        {
            WWWForm wwwForm = new WWWForm();

            if (!File.Exists(localFileName))
            {
                Debug.LogError("file not exist " + localFileName);
                yield break;
            }

            // set data
            using (FileStream fileStream = File.Open(localFileName, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                wwwForm.AddBinaryData("file", bytes, "123.jpg", "application/octet-stream");
            }

            wwwForm.headers["Authorization"] =
                "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("kaplc" + ":" + "123456"));

            WWW www = new WWW(url, wwwForm);
            yield return www;

            if (www.error is null)
            {
                Debug.Log("www form upload success");
                callBack?.Invoke(true);
            }
            else
            {
                Debug.LogError("www form upload error: " + www.error);
                callBack?.Invoke(false);
            }

            www.Dispose();
        }

        public void WWWFormSendField(string url, string fieldName, string value, UnityAction<bool> callBack)
        {
            StartCoroutine(WWWFormSendFieldCoroutine(url, fieldName, value, callBack));
        }
        
        private IEnumerator WWWFormSendFieldCoroutine(string url, string fieldName, string value, UnityAction<bool> callBack)
        {
            WWWForm wwwForm = new WWWForm();
            // set data
            wwwForm.AddField(fieldName, value);

            WWW www = new WWW(url, wwwForm);
            yield return www;

            if (www.error is not null)
            {
                Debug.Log("www form send field success");

                callBack?.Invoke(true);
            }
            else
            {
                Debug.LogError("www form send field error");

                callBack?.Invoke(false);
            }

            www.Dispose();
        }
    }
}