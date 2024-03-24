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
    public TMP_InputField ipInputField;
    public TMP_InputField inputField;
    public TMP_Dropdown dropdown;
    public Button sendButton;
    public Button start;

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
        
        start.onClick.AddListener(() =>
        {
            server = new Server();
            server.Start(ipInputField.text, type);
        });

        sendButton.onClick.AddListener(() =>
        {
            if (inputField.text == "")
            {
                return;
            }

            TextMessage message = new TextMessage();
            message.text = inputField.text;
            server.Broadcast(message);
        });
    }

    private void OnDestroy()
    {
        server?.Close();
    }
}