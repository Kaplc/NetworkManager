using System.Collections;
using System.Collections.Generic;
using Script.NetworkManager;
using UnityEngine;
using UnityEngine.UI;

public class FtpPanel : MonoBehaviour
{
    public Button button;
    public Button downloadButton;
    
    private NetworkManager networkManager;
    
    // Start is called before the first frame update
    void Start()
    {
        networkManager = new NetworkManager();
        networkManager.InitFtpManager("127.0.0.1", "kaplc", "123456");
        
        button.onClick.AddListener(() =>
        {
            networkManager.ftpManager.UploadFile(Application.dataPath + "/2.jpg", "12.jpg", () =>
            {
                Debug.Log("upload success");
            });
        });
        
        downloadButton.onClick.AddListener(() =>
        {
            networkManager.ftpManager.DownloadFile("12.jpg", Application.dataPath + "/3.jpg", () =>
            {
                Debug.Log("download success");
            });
            
        });
    }
}
