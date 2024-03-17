using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Editor.ProtocolTool
{
    public class CsharpGenerator
    {
        private static string XML_PATH;
        private static string CLASS_PATH;

        public CsharpGenerator(string xmlPath, string classPath)
        {
            XML_PATH = xmlPath;
            CLASS_PATH = classPath;
        }
        
        public void Start()
        {
            // check protocol.xml
            if (!File.Exists(XML_PATH))
            {
                Debug.LogError("Protocol.xml not found");
                return;
            }

            // check class folder
            if (!Directory.Exists(CLASS_PATH))
            {
                Directory.CreateDirectory(CLASS_PATH);
            }

            // parse protocol.xml
            XmlDocument doc = new XmlDocument();
            doc.Load(XML_PATH);
            XmlNode root = doc.SelectSingleNode("NetworkData");
            // generate data
            XmlNodeList dataList = root.SelectNodes("data");
            GenerateDataClass(dataList);

            // generate enum
            XmlNodeList enumList = root.SelectNodes("enum");
            GenerateEnum(enumList);

            // generate message
            XmlNodeList messageList = root.SelectNodes("message");
            GenerateMessageClass(messageList);
            
            Debug.Log("Generate Csharp Success!");
            AssetDatabase.Refresh();
        }

        private void GenerateDataClass(XmlNodeList nodeList)
        {
            string dataClassPath = CLASS_PATH + "DataClass/";
            // delete old file
            if (Directory.Exists(dataClassPath))
            {
                Directory.Delete(dataClassPath, true);
            }
            Directory.CreateDirectory(dataClassPath);
            
            foreach (XmlNode node in nodeList)
            {
                string classNamespace = node.Attributes["namespace"].Value;
                string className = node.Attributes["name"].Value; 
                string classExtends = "";
                if (node.Attributes["extend"].Value != "")
                {
                    classExtends = " : " + node.Attributes["extend"].Value;
                }

                string classStart = "using System;\n" +
                                    "using System.Collections;\n" +
                                    "using System.Collections.Generic;\n" +
                                    "using UnityEngine;\n" +
                                    "\n" +
                                    "namespace " + classNamespace + "\n" +
                                    "{\n" +
                                    "\tpublic class " + className +  classExtends + "\n" +
                                    "\t{\n";
                // get fields
                string fields = GetClassFieldText(node.SelectNodes("field"));

                string classEnd = "\t}\n" +
                                  "}";

                string classText = classStart + fields + classEnd;
                
                
                string classPath = dataClassPath + className + ".cs";
            
                File.WriteAllText(classPath, classText);
            }
        }

        private string GetClassFieldText(XmlNodeList fields)
        {
            string fieldsText = "";
            foreach (XmlNode field in fields)
            {
                switch (field.Attributes["type"].Value)
                {
                    case "int":
                        if (field.Attributes["value"].Value == "")
                            fieldsText += $"\t\tpublic int {field.Attributes["name"].Value};\n";
                        else
                            fieldsText += $"\t\tpublic int {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";
                        break;
                    case "float":
                        
                        if (field.Attributes["value"].Value == "")
                            fieldsText += $"\t\tpublic float {field.Attributes["name"].Value};\n";
                        else
                            fieldsText += $"\t\tpublic float {field.Attributes["name"].Value} = {field.Attributes["value"].Value}f;\n";
                        break;
                    case "bool":
                        
                        if (field.Attributes["value"].Value == "")
                            fieldsText += $"\t\tpublic bool {field.Attributes["name"].Value};\n";
                        else
                            fieldsText += $"\t\tpublic bool {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";

                        break;
                    case "string":

                        if (field.Attributes["value"].Value == "")
                        {
                            fieldsText += $"\t\tpublic string {field.Attributes["name"].Value};\n";
                        }
                        else
                        {
                            fieldsText += $"\t\tpublic string {field.Attributes["name"].Value} = \"{field.Attributes["value"].Value}\";\n";
                        }

                        break;
                    case "list":
                        fieldsText +=
                            $"\t\tpublic List<{field.Attributes["dataType"].Value}> {field.Attributes["name"].Value} " +
                            $"= new List<{field.Attributes["dataType"].Value}>();\n";
                        break;
                    case "dic":

                        fieldsText +=
                            $"\t\tpublic Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value}> {field.Attributes["name"].Value}" +
                            $" = new Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value}>();\n";
                        break;
                    case "enum":
                        if (field.Attributes["value"].Value == "")
                        {
                            fieldsText += $"\t\tpublic {field.Attributes["enumType"].Value} {field.Attributes["name"].Value};\n";
                        }
                        else
                        {
                            fieldsText += $"\t\tpublic {field.Attributes["enumType"].Value} {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";
                        }
                        
                        break;
                }
            }

            return fieldsText;
        }


        private void GenerateEnum(XmlNodeList nodeList)
        {
        }

        private void GenerateMessageClass(XmlNodeList nodeList)
        {
        }
    }
}