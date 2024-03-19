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
                string usingText = "using System;\n" +
                                   "using System.Collections;\n" +
                                   "using System.Collections.Generic;\n" +
                                   "using UnityEngine;\n";
                string namespaceText = node.Attributes["namespace"].Value;
                string classNameText = node.Attributes["name"].Value;
                string extendText = "";
                if (node.Attributes["extend"].Value != "")
                {
                    extendText = " : " + node.Attributes["extend"].Value;
                }

                
                string text = $"{usingText}" +
                              $"\n" +
                              $"namespace {namespaceText}\n" +
                              "{\n" +
                              $"\tpublic class {classNameText}{extendText}\n" +
                              "\t{\n" +
                              // get fields
                              GetClassFieldText(node.SelectNodes("field")) +
                              "\t}\n" +
                              "}\n";


                string classPath = dataClassPath + classNameText + ".cs";

                File.WriteAllText(classPath, text);
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
                            $"\t\tpublic List<{field.Attributes["valueType"].Value}> {field.Attributes["name"].Value} " +
                            $"= new List<{field.Attributes["valueType"].Value}>();\n";
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
                            fieldsText +=
                                $"\t\tpublic {field.Attributes["enumType"].Value} {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";
                        }

                        break;
                }
            }

            return fieldsText;
        }


        private void GenerateEnum(XmlNodeList nodeList)
        {
            string enumPath = CLASS_PATH + "Enum/";
            // delete old file
            if (Directory.Exists(enumPath))
            {
                Directory.Delete(enumPath, true);
            }

            Directory.CreateDirectory(enumPath);

            foreach (XmlNode node in nodeList)
            {
                string usingText = "using System;\n" +
                                   "using System.Collections;\n" +
                                   "using System.Collections.Generic;\n" +
                                   "using UnityEngine;\n";
                string namespaceText = node.Attributes["namespace"].Value;
                string enumName = node.Attributes["name"].Value;
                string text = $"{usingText}" +
                              $"\n" +
                              $"namespace {namespaceText}\n" +
                              "{\n" +
                              $"\tpublic enum {enumName}\n" +
                              "\t{\n" +
                              // get fields
                              GetEnumFieldsText(node.SelectNodes("field")) +
                              "\t}\n" +
                              "}\n";
                
                string path = enumPath + enumName + ".cs";

                File.WriteAllText(path, text);
            }
        }
        
        private string GetEnumFieldsText(XmlNodeList fields)
        {
            string fieldsText = "";
            
            foreach (XmlNode field in fields)
            {
                if (field.Attributes["value"].Value == "")
                {
                    fieldsText += $"\t\t{field.Attributes["name"].Value},\n";
                }
                else
                {
                    fieldsText += $"\t\t{field.Attributes["name"].Value} = {field.Attributes["value"].Value},\n";
                }
            }
            
            return fieldsText;
        }

        private void GenerateMessageClass(XmlNodeList nodeList)
        {
        }
    }
}