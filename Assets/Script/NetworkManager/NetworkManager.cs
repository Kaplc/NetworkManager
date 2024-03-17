using System.Collections;
using System.Collections.Generic;
using Script.NetworkManager;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Message2 message = new Message2();
        message.a = 1;
        message.b = 2.0f;
        message.c = true;
        message.d = "hello world";
        message.message1 = new Message1()
        {
            abc = "Kaplc"
        };
        
        byte[] bytes = message.Serialize();
        int index = 0;
        Message2 newMessage = new Message2();
        newMessage.Deserialize(bytes, ref index);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
