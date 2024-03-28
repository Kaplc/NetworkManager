using System.Collections;
using System.Collections.Generic;
using Script.NetworkManager;
using UnityEngine;
using UnityEngine.UI;

public class FtpPanel : MonoBehaviour
{
    public Button button;
    
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            NetworkManager networkManager = new NetworkManager();
            networkManager.InitFtpManager("127.0.0.1", "kaplc", "123456");
            networkManager.UploadFile(Application.dataPath + "/2.jpg", "12.jpg", () =>
            {
                Debug.Log("upload success");
            });
        });
    }
}
