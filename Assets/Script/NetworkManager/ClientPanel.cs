using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Script.NetworkManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientPanel : MonoBehaviour
{
    public NetworkManager networkManager;
    public TMP_InputField inputField;
    public TMP_InputField ipInputField;
    public TMP_Text textMeshPro;
    public Button connectButton;
    public Button disconnectButton;
    public Button sendButton;

    private int lineCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        connectButton.onClick.AddListener(() =>
        {
            bool success = networkManager.Connect(ipInputField.text, ENetworkType.UdpV4);
            if (success)
            {
                textMeshPro.text = "connect success\n";
            }
            else
            {
                textMeshPro.text = "connect failed\n";
            }
            
            // networkManager.ConnectAsync(ipInputField.text, ENetworkType.UdpV4);
        });

        disconnectButton.onClick.AddListener(() =>
        {
            networkManager.Disconnect(); 
            
        });

        sendButton.onClick.AddListener(() =>
        {
            // if (inputField.text == "")
            // {
            //     return;
            // }
            //
            // TextMessage message = new TextMessage();
            // message.text = inputField.text;
            // networkManager.Send(message);
            // if (lineCount > 5)
            // {
            //     textMeshPro.text = "";
            //     lineCount = 0;
            // }
            //
            // textMeshPro.text += "You:" + message.text + "\n";
            // lineCount++;
            
            // send string test
            // networkManager.Send(inputField.text);
            
            // send package test
            // MessageTest t1 = new MessageTest(); 
            // t1.data = 1;
            // byte[] bytes = t1.Serialize();
            //
            // byte[] bytes1 = new byte[10];
            // Array.Copy(bytes, 0, bytes1, 0, bytes1.Length);
            //
            // byte[] bytes2 = new byte[bytes.Length - 10];
            // Array.Copy(bytes, 10, bytes2, 0, bytes2.Length);
            //
            MessageTest2 t2 = new MessageTest2();
            t2.data2 = 2;
            t2.t5 = new DataTest5();
            // byte[] bytes3 = t2.Serialize();
            // byte[] b4 = new byte[5];
            // Array.Copy(bytes3, b4, 5);
            // byte[] b5 = new byte[bytes3.Length - 5];
            // Array.Copy(bytes3, 5, b5, 0, b5.Length);
            
            // networkManager.socket.Send(bytes1);
            // networkManager.socket.Send(bytes2);
            //
            // networkManager.socket.Send(bytes1);
            // networkManager.socket.Send(bytes2);
            //
            // networkManager.socket.Send(b4);
            // networkManager.socket.Send(b5);
            //
            // networkManager.socket.Send(bytes1);
            // networkManager.socket.Send(bytes2);
            
            // =====async
            // networkManager.SendAsync(bytes1);
            // networkManager.SendAsync(bytes2);
            //
            // networkManager.SendAsync(bytes1);
            // networkManager.SendAsync(bytes2);
            //
            // networkManager.SendAsync(b4);
            // networkManager.SendAsync(b5);
            //
            // networkManager.SendAsync(bytes1);
            // networkManager.SendAsync(bytes2);
            
            // =====udp 
            networkManager.SendTo(t2, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8800));
        });
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

        if (networkManager.receiveMessageQueue.Count > 0)
        {
            BaseNetworkData data = networkManager.receiveMessageQueue.Dequeue();
            switch (data)
            {
                case TextMessage textMessage:
                    if (lineCount > 5)
                    {
                        textMeshPro.text = "";
                        lineCount = 0;
                    }

                    textMeshPro.text += "server: " + textMessage.text + "\n";
                    lineCount++;
                    break;
                case HeartMessage heartMessage:
                    Debug.Log("heart");
                    break;
                default:
                    Debug.Log($"server: {data}");
                    break;
            }
        }
    }
}