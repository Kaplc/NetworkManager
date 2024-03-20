using System.IO;
using System.Text;
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


        #region data class

        private void GenerateDataClass(XmlNodeList classes)
        {
            string dataClassPath = CLASS_PATH + "DataClass/";
            // delete old file
            if (Directory.Exists(dataClassPath))
            {
                Directory.Delete(dataClassPath, true);
            }

            Directory.CreateDirectory(dataClassPath);

            foreach (XmlNode class_ in classes)
            {
                string usingText = "using System;\n" +
                                   "using System.Collections;\n" +
                                   "using System.Collections.Generic;\n" +
                                   "using System.Text;\n";
                string namespaceText = class_.Attributes["namespace"].Value;
                string classNameText = class_.Attributes["name"].Value;
                string extendText = "";
                if (class_.Attributes["extend"].Value != "")
                {
                    extendText = " : " + class_.Attributes["extend"].Value;
                }


                string text = $"{usingText}" +
                              $"\n" +
                              $"namespace {namespaceText}\n" +
                              "{\n" +
                              $"\tpublic class {classNameText}{extendText}\n" +
                              "\t{\n" +
                              // get fields
                              GetClassFieldText(class_.SelectNodes("field")) +
                              "\n" +
                              // get size func
                              GetSizeFuncText(class_.SelectNodes("field")) +
                              "\n" +
                              GetSerializeFuncText(class_.SelectNodes("field")) +
                              "\n" +
                              GetDeserializeFuncText(class_.SelectNodes("field")) +
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
                        if (field.Attributes["valueType"].Value.StartsWith("enum"))
                        {
                            fieldsText +=
                                $"\t\tpublic List<{field.Attributes["valueType"].Value.Split('_')[1]}> {field.Attributes["name"].Value} " +
                                $"= new List<{field.Attributes["valueType"].Value.Split('_')[1]}>();\n";
                        }
                        else
                        {
                            fieldsText +=
                                $"\t\tpublic List<{field.Attributes["valueType"].Value}> {field.Attributes["name"].Value} " +
                                $"= new List<{field.Attributes["valueType"].Value}>();\n";
                        }

                        break;
                    case "dic":

                        if (field.Attributes["keyType"].Value.StartsWith("enum") && field.Attributes["valueType"].Value.StartsWith("enum"))
                        {
                            fieldsText +=
                                $"\t\tpublic Dictionary<{field.Attributes["keyType"].Value.Split('_')[1]}, {field.Attributes["valueType"].Value.Split('_')[1]}> {field.Attributes["name"].Value}" +
                                $" = new Dictionary<{field.Attributes["keyType"].Value.Split('_')[1]}, {field.Attributes["valueType"].Value.Split('_')[1]}>();\n";
                        }
                        else if (field.Attributes["keyType"].Value.StartsWith("enum"))
                        {
                            fieldsText +=
                                $"\t\tpublic Dictionary<{field.Attributes["keyType"].Value.Split('_')[1]}, {field.Attributes["valueType"].Value}> {field.Attributes["name"].Value}" +
                                $" = new Dictionary<{field.Attributes["keyType"].Value.Split('_')[1]}, {field.Attributes["valueType"].Value}>();\n";
                        }
                        else if (field.Attributes["valueType"].Value.StartsWith("enum"))
                        {
                            fieldsText +=
                                $"\t\tpublic Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value.Split('_')[1]}> {field.Attributes["name"].Value}" +
                                $" = new Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value.Split('_')[1]}>();\n";
                        }
                        else
                        {
                            fieldsText +=
                                $"\t\tpublic Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value}> {field.Attributes["name"].Value}" +
                                $" = new Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value}>();\n";
                        }
                       
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
                    default:
                        fieldsText +=
                            $"\t\tpublic {field.Attributes["type"].Value} {field.Attributes["name"].Value};\n";
                            // $" = new {field.Attributes["type"].Value}();\n";
                        break;
                }
            }

            return fieldsText;
        }


        private string GetSizeFuncText(XmlNodeList fields)
        {
            string sizeFuncText = "\t\tpublic override int GetSize()\n" +
                                  "\t\t{\n" +
                                  "\t\t\tint size = 0;\n";

            foreach (XmlNode field in fields)
            {
                string fieldType = field.Attributes["type"].Value;


                switch (fieldType)
                {
                    case "list":
                        string valueType = field.Attributes["valueType"].Value;
                        // list count
                        sizeFuncText += "\n\t\t\tsize += sizeof(int);\n";

                        // func
                        string listForeachText = "";
                        listForeachText += $"\t\t\tforeach (var item in {field.Attributes["name"].Value})\n";
                        listForeachText += "\t\t\t{\n";

                        if (valueType.StartsWith("enum"))
                        {
                            listForeachText += $"\t\t\t\tsize += 4;\n";
                        }
                        else
                        {
                            listForeachText += $"\t\t\t\tsize += {GetBaseTypeSize(valueType, "item")};\n";
                        }

                        listForeachText += "\t\t\t}\n";
                        sizeFuncText += listForeachText;

                        break;
                    case "dic":
                        string dicKeyType = field.Attributes["keyType"].Value;
                        string dicValueType = field.Attributes["valueType"].Value;
                        // dict count
                        sizeFuncText += "\n\t\t\tsize += sizeof(int);\n";
                        // func
                        string dicForeachText = "";
                        dicForeachText += $"\t\t\tforeach (var item in {field.Attributes["name"].Value})\n";
                        dicForeachText += "\t\t\t{\n";
                        
                        if (dicKeyType.StartsWith("enum") && !dicValueType.StartsWith("enum"))
                        {
                            dicForeachText += $"\t\t\t\tsize += 4;\n";
                            dicForeachText += $"\t\t\t\tsize += {GetBaseTypeSize(dicValueType, "item.Value")};\n";
                        }
                        else if (dicValueType.StartsWith("enum") && !dicKeyType.StartsWith("enum"))
                        {
                            dicForeachText += $"\t\t\t\tsize += {GetBaseTypeSize(dicKeyType, "item.Key")};\n";
                            dicForeachText += $"\t\t\t\tsize += 4;\n";
                        }
                        else if (dicValueType.StartsWith("enum") && dicKeyType.StartsWith("enum"))
                        {
                            dicForeachText += $"\t\t\t\tsize += 4;\n";
                            dicForeachText += $"\t\t\t\tsize += 4;\n";
                        }
                        else
                        {
                            dicForeachText += $"\t\t\t\tsize += {GetBaseTypeSize(dicKeyType, "item.Key")};\n";
                            dicForeachText += $"\t\t\t\tsize += {GetBaseTypeSize(dicValueType, "item.Value")};\n";
                        }

                        dicForeachText += "\t\t\t}\n";

                        sizeFuncText += dicForeachText;
                        break;
                    default:
                        sizeFuncText += $"\t\t\tsize += {GetBaseTypeSize(field.Attributes["type"].Value, field.Attributes["name"].Value)};\n";
                        break;
                }
            }

            sizeFuncText += "\n\t\t\treturn size;\n" +
                            "\t\t}\n";
            return sizeFuncText;
        }

        private string GetSerializeFuncText(XmlNodeList fields)
        {
            string funcText = "\t\tpublic override byte[] Serialize()\n" +
                              "\t\t{\n" +
                              "\t\t\tbyte[] bytes = new byte[GetSize()];\n" +
                              "\t\t\tint index = 0;\n" +
                              "\n";

            foreach (XmlNode field in fields)
            {
                switch (field.Attributes["type"].Value)
                {
                    case "int":
                        funcText += $"\t\t\tWriteInt({field.Attributes["name"].Value}, bytes, ref index);\n";
                        break;
                    case "float":
                        funcText += $"\t\t\tWriteFloat({field.Attributes["name"].Value}, bytes, ref index);\n";
                        break;
                    case "bool":
                        funcText += $"\t\t\tWriteBool({field.Attributes["name"].Value}, bytes, ref index);\n";
                        break;
                    case "string":
                        funcText += $"\t\t\tWriteString({field.Attributes["name"].Value}, bytes, ref index);\n";
                        break;
                    case "enum":
                        funcText += $"\t\t\tWriteInt((int){field.Attributes["name"].Value}, bytes, ref index);\n";
                        break;
                    case "list":
                        funcText += $"\n\t\t\tWriteInt({field.Attributes["name"].Value}.Count, bytes, ref index);\n";
                        funcText += $"\t\t\tforeach (var item in {field.Attributes["name"].Value})\n";
                        funcText += "\t\t\t{\n";

                        if (field.Attributes["valueType"].Value.StartsWith("enum"))
                        {
                            funcText += $"\t\t\t\tWriteInt((int)item, bytes, ref index);\n";
                        }
                        else
                        {
                            funcText += $"\t\t\t\t{GetBaseTypeSerialize(field.Attributes["valueType"].Value, "item")};\n";
                        }

                        funcText += "\t\t\t}\n";
                        break;
                    case "dic":

                        funcText += $"\n\t\t\tWriteInt({field.Attributes["name"].Value}.Count, bytes, ref index);\n";
                        funcText += $"\t\t\tforeach (var item in {field.Attributes["name"].Value})\n";
                        funcText += "\t\t\t{\n";
                        if (field.Attributes["keyType"].Value.StartsWith("enum") && !field.Attributes["valueType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\tWriteInt((int)item.Key, bytes, ref index);\n";
                        }
                        else if (field.Attributes["valueType"].Value.StartsWith("enum") && !field.Attributes["keyType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\t{GetBaseTypeSerialize(field.Attributes["keyType"].Value, "item.Key")};\n";
                        }
                        else if (field.Attributes["valueType"].Value.StartsWith("enum") && field.Attributes["keyType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\tWriteInt((int)item.Key, bytes, ref index);\n";
                            funcText +=
                                $"\t\t\t\tWriteInt((int)item.Value, bytes, ref index);\n";
                        }
                        else if (field.Attributes["valueType"].Value.StartsWith("enum") && field.Attributes["keyType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\tWriteInt((int)item.Key, bytes, ref index);\n";
                            funcText +=
                                $"\t\t\t\tWriteInt((int)item.Value, bytes, ref index);\n";
                        }
                        else
                        {
                            funcText += $"\t\t\t\t{GetBaseTypeSerialize(field.Attributes["keyType"].Value, "item.Key")};\n";
                            funcText += $"\t\t\t\t{GetBaseTypeSerialize(field.Attributes["valueType"].Value, "item.Value")};\n";
                        }

                        funcText += "\t\t\t}\n";
                        break;
                    default:
                        funcText += $"\t\t\t{GetBaseTypeSerialize(field.Attributes["type"].Value, field.Attributes["name"].Value)};\n";
                        break;
                }
            }

            funcText += "\n\t\t\treturn bytes;\n" +
                        "\t\t}\n";
            return funcText;
        }

        private string GetDeserializeFuncText(XmlNodeList fields)
        {
            string funcText = "\t\tpublic override T Deserialize<T>(byte[] bytes, ref int index)\n" +
                              "\t\t{\n";

            foreach (XmlNode field in fields)
            {
                switch (field.Attributes["type"].Value)
                {
                    case "list":
                        funcText += $"\n\t\t\tint listCount = ReadInt(bytes, ref index);\n";
                        funcText += $"\t\t\tfor (int i = 0; i < listCount; i++)\n";
                        funcText += "\t\t\t{\n";
                        if (field.Attributes["valueType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\t{field.Attributes["name"].Value}.Add({GetBaseTypeDeserialize(field.Attributes["valueType"].Value.Split('_')[1], true)});\n";
                        }
                        else
                        {
                            funcText +=
                                $"\t\t\t\t{field.Attributes["name"].Value}.Add({GetBaseTypeDeserialize(field.Attributes["valueType"].Value)});\n";
                        }

                        funcText += "\t\t\t}\n";

                        break;
                    case "dic":
                        funcText += $"\n\t\t\tint dicCount = ReadInt(bytes, ref index);\n";
                        funcText += $"\t\t\tfor (int i = 0; i < dicCount; i++)\n";
                        funcText += "\t\t\t{\n";

                        if (field.Attributes["keyType"].Value.StartsWith("enum") && !field.Attributes["valueType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\t{field.Attributes["name"].Value}.Add({GetBaseTypeDeserialize(field.Attributes["keyType"].Value.Split('_')[1], true)}," +
                                $"{GetBaseTypeDeserialize(field.Attributes["valueType"].Value)});\n";
                        }
                        else if (field.Attributes["valueType"].Value.StartsWith("enum") && !field.Attributes["keyType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\t{field.Attributes["name"].Value}.Add({GetBaseTypeDeserialize(field.Attributes["keyType"].Value)}," +
                                $"{GetBaseTypeDeserialize(field.Attributes["valueType"].Value.Split('_')[1], true)});\n";
                        }
                        else if (field.Attributes["valueType"].Value.StartsWith("enum") && field.Attributes["keyType"].Value.StartsWith("enum"))
                        {
                            funcText +=
                                $"\t\t\t\t{field.Attributes["name"].Value}.Add({GetBaseTypeDeserialize(field.Attributes["keyType"].Value.Split('_')[1], true)}," +
                                $"{GetBaseTypeDeserialize(field.Attributes["valueType"].Value.Split('_')[1], true)});\n";
                        }
                        else
                        {
                            funcText +=
                                $"\t\t\t\t{field.Attributes["name"].Value}.Add({GetBaseTypeDeserialize(field.Attributes["keyType"].Value)}," +
                                $"{GetBaseTypeDeserialize(field.Attributes["valueType"].Value)});\n";
                        }

                        funcText += "\t\t\t}\n";

                        break;
                    case "enum":
                        funcText +=
                            $"\t\t\t{field.Attributes["name"].Value} = {GetBaseTypeDeserialize(field.Attributes["enumType"].Value, true)};\n";
                        break;
                    default:
                        funcText +=
                            $"\t\t\t{field.Attributes["name"].Value} = {GetBaseTypeDeserialize(field.Attributes["type"].Value)};\n";
                        break;
                }
            }

            funcText += "\n\t\t\treturn this as T;\n" +
                        "\t\t}\n";

            return funcText;
        }

        private string GetBaseTypeDeserialize(string type, bool isEnum = false)
        {
            string deserialize = "";
            if (isEnum)
            {
                deserialize = $"({type})ReadInt(bytes, ref index)";
                return deserialize;
            }

            switch (type)
            {
                case "int":
                    deserialize = $"ReadInt(bytes, ref index)";
                    break;
                case "float":
                    deserialize = $"ReadFloat(bytes, ref index)";
                    break;
                case "bool":
                    deserialize = $"ReadBool(bytes, ref index)";
                    break;
                case "string":
                    deserialize = $"ReadString(bytes, ref index)";
                    break;
                default:
                    deserialize = $"ReadClass<{type}>(bytes, ref index)";
                    break;
            }

            return deserialize;
        }

        private string GetBaseTypeSerialize(string type, string name = "")
        {
            string serialize = "";
            switch (type)
            {
                case "int":
                    serialize = $"WriteInt({name}, bytes, ref index)";
                    break;
                case "float":
                    serialize = $"WriteFloat({name}, bytes, ref index)";
                    break;
                case "bool":
                    serialize = $"WriteBool({name}, bytes, ref index)";
                    break;
                case "string":
                    serialize = $"WriteString({name}, bytes, ref index)";
                    break;
                case "enum":
                    serialize = $"WriteInt((int){name}, bytes, ref index)";
                    break;
                default:
                    serialize = $"WriteClass({name}, bytes, ref index)";
                    break;
            }

            return serialize;
        }

        private string GetBaseTypeSize(string type, string name = "")
        {
            string size = "";
            switch (type)
            {
                case "int":
                    size = "sizeof(int)";
                    break;
                case "float":
                    size = "sizeof(float)";
                    break;
                case "bool":
                    size = "sizeof(bool)";
                    break;
                case "string":
                    size = $"sizeof(int) + Encoding.UTF8.GetByteCount({name})";
                    break;
                case "enum":
                    size = "4";
                    break;
                default:
                    size = $"{name}.GetSize()";
                    break;
            }

            return size;
        }

        #endregion

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

        private void GenerateMessageClass(XmlNodeList messages)
        {
            string messageClassPath = CLASS_PATH + "MessageClass/";
            // delete old file
            if (Directory.Exists(messageClassPath))
            {
                Directory.Delete(messageClassPath, true);
            }

            Directory.CreateDirectory(messageClassPath);

            foreach (XmlNode message in messages)
            {
                string usingText = "using System;\n" +
                                   "using System.Collections;\n" +
                                   "using System.Collections.Generic;\n" +
                                   "using UnityEngine;\n";
                string namespaceText = message.Attributes["namespace"].Value;
                string classNameText = message.Attributes["name"].Value;
                string extendText = "";
                if (message.Attributes["extend"].Value != "")
                {
                    extendText = " : " + message.Attributes["extend"].Value;
                }

                string text = $"{usingText}" +
                              $"\n" +
                              $"namespace {namespaceText}\n" +
                              "{\n" +
                              $"\tpublic class {classNameText}{extendText}\n" +
                              "\t{\n" +
                              $"\t\tpublic int id = {message.Attributes["id"].Value};\n" +
                              // get fields
                              GetClassFieldText(message.SelectNodes("field")) +
                              "\t}\n" +
                              "}\n";


                string classPath = messageClassPath + classNameText + ".cs";

                File.WriteAllText(classPath, text);
            }
        }
    }
}