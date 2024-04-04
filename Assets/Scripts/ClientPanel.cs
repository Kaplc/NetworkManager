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
    public TMP_Dropdown dropdown;
    public Button connectButton;
    public Button disconnectButton;
    public Button sendButton;
    
    private ENetworkType type;

    // Start is called before the first frame update
    void Start()
    {
        dropdown.onValueChanged.AddListener((index) =>
        {
            switch (index)
            {
                case 0:
                    type = ENetworkType.TcpV4;
                    break;
                case 1:
                    type = ENetworkType.TcpV6;
                    break;
                case 2:
                    type = ENetworkType.UdpV4;
                    break;
                case 3:
                    type = ENetworkType.UdpV6;
                    break;
            }
        });
        
        connectButton.onClick.AddListener(() =>
        {
            switch (type)
            {
                case ENetworkType.TcpV4:
                    networkManager = new NetworkManager();
                    networkManager.InitTcpManager(ENetworkType.TcpV4, EAsyncType.Thread, new IPEndPoint(IPAddress.Parse(ipInputField.text), 8800));
                    networkManager.tcpManager.Connect();
                    break;
                case ENetworkType.TcpV6:
                    networkManager = new NetworkManager();
                    networkManager.InitTcpManager(ENetworkType.TcpV6, EAsyncType.Thread, new IPEndPoint(IPAddress.Parse(ipInputField.text), 8800));
                    networkManager.tcpManager.Connect();
                    break;
                case ENetworkType.UdpV4:
                case ENetworkType.UdpV6:
                    networkManager = new NetworkManager();
                    networkManager.InitUdpManager(type, EAsyncType.Thread);
                    networkManager.udpManager.Start();
                    break;
            }

            disconnectButton.gameObject.SetActive(true);
        });

        disconnectButton.onClick.AddListener(() =>
        {
            switch (type)
            {
                case ENetworkType.TcpV4:
                case ENetworkType.TcpV6:
                    networkManager.CloseTcp();
                    break;
                case ENetworkType.UdpV4:
                case ENetworkType.UdpV6:
                    networkManager.CloseUdp();
                    break;
            }
            
            disconnectButton.gameObject.SetActive(false);
        });
        
        disconnectButton.gameObject.SetActive(false);
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
            MessageTest t1 = new MessageTest(); 
            t1.data = 1;
            byte[] bytes = t1.Serialize();
            
            byte[] bytes1 = new byte[10];
            Array.Copy(bytes, 0, bytes1, 0, bytes1.Length);
            
            byte[] bytes2 = new byte[bytes.Length - 10];
            Array.Copy(bytes, 10, bytes2, 0, bytes2.Length);
            //
            MessageTest2 t2 = new MessageTest2();
            t2.data2 = 2;
            t2.t5 = new DataTest5();
            byte[] bytes3 = t2.Serialize();
            byte[] b4 = new byte[5];
            Array.Copy(bytes3, b4, 5);
            byte[] b5 = new byte[bytes3.Length - 5];
            Array.Copy(bytes3, 5, b5, 0, b5.Length);
            
            networkManager.udpManager.SendTo(t1, new IPEndPoint(IPAddress.Parse(ipInputField.text), 8800));
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
            // networkManager.SendTo(t2, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8800));
        });
    }

    private void OnDestroy()
    {
        disconnectButton.onClick.Invoke();
    }
}