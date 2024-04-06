using System.IO;
using System.Xml;
using Editor.ProtocolTool.Csharp;
using UnityEditor;
using UnityEngine;

public enum EProtocolClassType
{
    Enum,
    Class
}

namespace Editor.ProtocolTool
{
    public static class ProtocolTool
    {
        private static string XML_PATH = Application.dataPath + "/Config/Network/Protocol.xml";
        private static string CSHARP_CLASS_PATH = Application.dataPath + "/Scripts/Network/ProtocolClass/";

        private static CsharpGenerator csharpGenerator;
        
        [MenuItem("Editor/ProtocolTool/GenerateCsharp")]
        public static void GenerateCsharp()
        {
            csharpGenerator = new CsharpGenerator(XML_PATH, CSHARP_CLASS_PATH);
            csharpGenerator.Start();
        }

        [MenuItem("Editor/ProtocolTool/GenerateJava")]
        public static void GenerateJava()
        {
            
        }
        
        [MenuItem("Editor/ProtocolTool/GenerateCpp")]
        public static void GenerateCpp()
        {
            
        }
    }
}