using System.IO;
using System.Xml;

namespace Editor.ProtocolTool.Csharp
{
    public class MessageHandlerGenerator
    {
        private string MESSAGE_HANDLER_PATH;
        
        public MessageHandlerGenerator(string messageHandlerPath)
        {
            MESSAGE_HANDLER_PATH = messageHandlerPath;
        }
        
        
        public void Generate(XmlNodeList messageNodeList)
        {
            if (!Directory.Exists(MESSAGE_HANDLER_PATH))
            {
                Directory.CreateDirectory(MESSAGE_HANDLER_PATH);
            }

            foreach (XmlNode node in messageNodeList)
            {
                string usingText = "using System;\n" +
                                   "using Network;\n" +
                                   "using Network.Base;\n" +
                                   "using UnityEngine;\n";

                string fieldText = $"\t\tpublic {node.Attributes["name"].Value} Message=> message as {node.Attributes["name"].Value};\n\n";
            
                string funcText = "\t\tpublic override void Handle()\n" +
                                  "\t\t{\n" +
                                  "\n" +
                                  "\t\t}\n";
                
                string classText = $"\tpublic class {node.Attributes["name"].Value}Handler: BaseHandler\n" +
                                   "\t{\n" +
                                   fieldText +
                                   funcText +
                                   "\t}\n";
                
                string nameSpace = "\nnamespace Network.ProtocolClass.Handler\n" +
                                   "{\n" +
                                      classText +
                                   "}\n";
                
                string text = usingText + nameSpace;

                if (!File.Exists(MESSAGE_HANDLER_PATH + node.Attributes["name"].Value + "Handler.cs"))
                {
                    File.WriteAllText(MESSAGE_HANDLER_PATH + node.Attributes["name"].Value + "Handler.cs", text);

                }
            }
        }
    }
}