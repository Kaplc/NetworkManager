using System.Collections;
using System.Collections.Generic;
using System.Net;
using Script.NetworkManager;
using UnityEngine;
using UnityEngine.UI;

public class HttpPanel : MonoBehaviour
{
    public Button btnGet;
    public Button btnPost;

    private HttpManager httpManager;

    // Start is called before the first frame update
    void Start()
    {
        httpManager = new HttpManager();
        
        btnGet.onClick.AddListener(() =>
        {
            httpManager.DownloadFile("http://192.168.43.189:8080/1.jpg", Application.dataPath + "/4.jpg", (code) =>
            {
                if (code == HttpStatusCode.OK)
                {
                    Debug.Log("download success");
                }
                else
                {
                    Debug.Log("download fail " + code);
                }
            });
        });

        btnPost.onClick.AddListener(() =>
        {
            httpManager.UploadFile("http://192.168.43.189/", Application.dataPath + "/4.jpg", (code) =>
            {
                if (code == HttpStatusCode.OK)
                {
                    Debug.Log("upload success");
                }
                else
                {
                    Debug.Log("upload fail " + code);
                }
            });
            
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}