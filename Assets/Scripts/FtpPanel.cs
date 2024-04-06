using System.Collections;
using System.Collections.Generic;
using Network;
using Network.Manager;
using Script.NetworkManager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FtpPanel : MonoBehaviour
{
    public Button button;
    public Button btnDownload;
    public Button btnGetFileSize;
    public Button btnGetFileList;
    public Button btnCreateDirectory;
    
    private NetworkManager networkManager;
    
    // Start is called before the first frame update
    void Start()
    {
        networkManager = new NetworkManager();
        networkManager.InitFtpManager("127.0.0.1", "kaplc", "123456");
        
        button.onClick.AddListener(() =>
        {
            networkManager.FtpManager.UploadFile(Application.dataPath + "/2.jpg", "12.jpg", () =>
            {
                Debug.Log("upload success");
            });
        });
        
        btnDownload.onClick.AddListener(() =>
        {
            networkManager.FtpManager.DownloadFile("12.jpg", Application.dataPath + "/3.jpg", () =>
            {
                Debug.Log("download success");
            });
            
        });
        
        btnGetFileSize.onClick.AddListener(() =>
        {
            networkManager.FtpManager.GetFileSize("12.jpg", size =>
            {
                Debug.Log("size: " + size);
            });
        });
        
        btnCreateDirectory.onClick.AddListener(() =>
        {
            networkManager.FtpManager.CreateDirectory("Save2", () =>
            {
                Debug.Log("create directory success");
            });
        });
        
        btnGetFileList.onClick.AddListener(() =>
        {
            networkManager.FtpManager.GetFileList("Save2", list =>
            {
                foreach (var item in list)
                {
                    Debug.Log(item);
                }
            });
        });
    }
}
