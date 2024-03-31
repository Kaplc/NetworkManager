using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomDownloadHandler : DownloadHandlerScript
{
    protected override float GetProgress()
    {
        return base.GetProgress();
    }
    
    protected override byte[] GetData()
    {
        return base.GetData();
    }
    
    protected override string GetText()
    {
        return base.GetText();
    }
    
    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        return base.ReceiveData(data, dataLength);
    }
    
    protected override void CompleteContent()
    {
        base.CompleteContent();
    }
}

