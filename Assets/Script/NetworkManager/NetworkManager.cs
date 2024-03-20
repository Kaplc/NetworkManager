using System.Collections;
using System.Collections.Generic;
using Script.NetworkManager;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DataTest dataTest = new DataTest();
        dataTest.dataTest2 = new DataTest2()
        {
            dic3 = new Dictionary<int, DataTest3>()
            {
                {1, new DataTest3()
                {
                    lis3 = new List<EnumTest>()
                    {
                        EnumTest.Unity
                    }
                }},
                {2, new DataTest3()
                {
                    lis3 = new List<EnumTest>()
                    {
                        EnumTest.Windows
                    }
                }},
                {3, new DataTest3()
                {
                    lis3 = new List<EnumTest>()
                    {
                        EnumTest.Android
                    }
                }},
            },
        };
        
        dataTest.dataTest2.dataTest3 = new DataTest3();
        
        
        byte[] bytes = dataTest.Serialize();
        int index = 0;
        DataTest dataTest2 = new DataTest().Deserialize<DataTest>(bytes, ref index);

        // DataTest4 dataTest4 = new DataTest4();
        // dataTest4.t5 = new DataTest5();
        // byte[] bytes = dataTest4.Serialize();
        // int index = 0;
        // DataTest4 t4 = new DataTest4().Deserialize<DataTest4>(bytes, ref index);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
