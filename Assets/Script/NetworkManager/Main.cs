using System;
using System.Collections;
using System.Collections.Generic;
using Script.NetworkManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Server server;
    public NetworkManager networkManager;
    public TMP_InputField inputField;
    public Button connectButton;
    public Button disconnectButton;
    public Button sendButton;

    // Start is called before the first frame update
    void Start()
    {
        connectButton.onClick.AddListener(() =>
        {
            networkManager.Connect();
        });
        
        disconnectButton.onClick.AddListener(() =>
        {
            networkManager.Disconnect();
        });
        
        sendButton.onClick.AddListener(() =>
        {
            // networkManager.Send(inputField.text);
            
            MessageTest t1 = new MessageTest(); 
            t1.data = 1;
            byte[] bytes = t1.Serialize();
            
            byte[] bytes1 = new byte[10];
            Array.Copy(bytes, 0, bytes1, 0, bytes1.Length);
            
            byte[] bytes2 = new byte[bytes.Length - 10];
            Array.Copy(bytes, 10, bytes2, 0, bytes2.Length);
            
            MessageTest2 t2 = new MessageTest2();
            t2.data2 = 2;
            t2.t5 = new DataTest5();
            byte[] bytes3 = t2.Serialize();
            byte[] b4 = new byte[5];
            Array.Copy(bytes3, b4, 5);
            byte[] b5 = new byte[bytes3.Length - 5];
            Array.Copy(bytes3, 5, b5, 0, b5.Length);

            networkManager.socket.Send(bytes1);
            networkManager.socket.Send(bytes2);
            
            networkManager.socket.Send(bytes1);
            networkManager.socket.Send(bytes2);
            
            networkManager.socket.Send(b4);
            networkManager.socket.Send(b5);
            
            networkManager.socket.Send(bytes1);
            networkManager.socket.Send(bytes2);
        });

        server = new Server();
        server.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (networkManager.isConnect)
        {
            disconnectButton.gameObject.SetActive(true);
            connectButton.gameObject.SetActive(false);
            
        }
        else
        {
            connectButton.gameObject.SetActive(true);
            disconnectButton.gameObject.SetActive(false);
        }
    }
}
