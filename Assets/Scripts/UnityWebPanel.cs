using System.Collections;
using System.Collections.Generic;
using Network;
using Network.Manager;
using Script.NetworkManager;
using UnityEngine;
using UnityEngine.UI;

public class UnityWebPanel : MonoBehaviour
{
    private string username = "kaplc";
    private string password = "123456";

    public Button btnUpload;
    public Button btnDownload;

    // Start is called before the first frame update
    void Start()
    {
        btnDownload.onClick.AddListener(() =>
        {
            // UnityWWWManager.Instance.WWWLoad<Texture>($"ftp://{username}:{password}@127.0.0.1/1.jpg", (texture) => { Debug.Log(texture); });
            
            // UnityWebManager.Instance.Download<Texture>($"ftp://{username}:{password}@127.0.0.1/1.jpg", (texture) => { Debug.Log(texture); });
        });

        btnUpload.onClick.AddListener(() =>
        {
            // UnityWebManager.Instance.WWWFormUploadFile($"ftp://{username}:{password}@127.0.0.1/", Application.dataPath + "/4.jpg", (isSuccess) =>
            // {
            //     if (isSuccess)
            //     {
            //         Debug.Log("success");
            //     }
            // });

            // UnityWWWManager.Instance.WWWFormUploadFile("http://192.168.43.189/", Application.dataPath + "/4.jpg", (isSuccess) =>
            // {
            //     if (isSuccess)
            //     {
            //         Debug.Log("success");
            //     }
            // });

            // UnityWebManager.Instance.UploadFile("http://192.168.43.189/", Application.dataPath + "/4.jpg", "4.jpg", (isSuccess) =>
            // {
            //     if (isSuccess)
            //     {
            //         Debug.Log("success");
            //     }
            // });
            
            UnityWebManager.Instance.UploadFilePut($"ftp://{username}:{password}@127.0.0.1/", Application.dataPath + "/4.jpg", "5.jpg",(isSuccess) =>
            {
                if (isSuccess)
                {
                    Debug.Log("success");
                }
            });
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}