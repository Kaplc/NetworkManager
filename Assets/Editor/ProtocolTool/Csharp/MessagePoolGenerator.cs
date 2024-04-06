using System.IO;
using System.Xml;

namespace Editor.ProtocolTool.Csharp
{
    public class MessagePoolGenerator
    {
        private string MESSAGE_POOL_PATH;

        public MessagePoolGenerator(string messagePoolPath)
        {
            MESSAGE_POOL_PATH = messagePoolPath;
        }

        public void Generate(XmlNodeList messageNodeList)
        {
            string usingText = "using System;\n" +
                               "using System.Collections.Generic;\n" +
                               "using Network.Base;\n" +
                               "using Network.ProtocolClass.Handler;\n" +
                               "using UnityEngine;\n";

            string funcText = "\t\tprivate void RegisterMessage(int id, Type messageType, Type handlerType)\n" +
                              "\t\t{\n" +
                              "\t\t\tmessagePool.Add(id, messageType);\n" +
                              "\t\t\thandlerPool.Add(id, handlerType);\n" +
                              "\t\t}\n" +
                              "\n" +
                              "\t\tpublic BaseMessage GetMessage(int id)\n" +
                              "\t\t{\n" +
                              "\t\t\tif (messagePool.ContainsKey(id) == false)\n" +
                              "\t\t\t{\n" +
                              "\t\t\t\tDebug.Log(\"not found message id: \" + id);\n" +
                              "\t\t\t\treturn null;\n" +
                              "\t\t\t}\n" +
                              "\n" +
                              "\t\t\treturn Activator.CreateInstance(messagePool[id]) as BaseMessage;\n" +
                              "\t\t}\n" +
                              "\n" +
                              "\t\tpublic BaseHandler GetHandler(int id)\n" +
                              "\t\t{\n" +
                              "\t\t\tif (handlerPool.ContainsKey(id) == false)\n" +
                              "\t\t\t{\n" +
                              "\t\t\t\tDebug.Log(\"not found handler id: \" + id);\n" +
                              "\t\t\t\treturn null;\n" +
                              "\t\t\t}\n" +
                              "\n" +
                              "\t\t\treturn Activator.CreateInstance(handlerPool[id]) as BaseHandler;\n" +
                              "\t\t}\n";

            string classText = "\tpublic class MessagePool\n" +
                               "\t{\n" +
                               "\t\tprivate Dictionary<int, Type> messagePool = new Dictionary<int, Type>();\n" +
                               "\t\tprivate Dictionary<int, Type> handlerPool = new Dictionary<int, Type>();\n" +
                               "\n" +
                               "\t\tpublic MessagePool()\n" +
                               "\t\t{\n" +
                               "\t\t\t// register message\n" +
                               GenerateRegisterText(messageNodeList) +
                               "\t\t}\n" +
                               "\n" +
                               funcText +
                               "\t}\n";


            string namespaceText = "\nnamespace Network.ProtocolClass\n" +
                                   "{\n" +
                                   classText +
                                   "}\n";

            string text = usingText + namespaceText;

            if (File.Exists(MESSAGE_POOL_PATH))
            {
                File.Delete(MESSAGE_POOL_PATH);
            }

            File.WriteAllText(MESSAGE_POOL_PATH + "MessagePool.cs", text);
        }

        private string GenerateRegisterText(XmlNodeList messageNodeList)
        {
            string text = "";


            foreach (XmlNode node in messageNodeList)
            {
                string id = node.Attributes["id"].Value;
                string name = node.Attributes["name"].Value;
                text += $"\t\t\tRegisterMessage({id}, typeof({name}), typeof({name}Handler));\n";
            }

            return text;
        }
    }
}