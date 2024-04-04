using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug=UnityEngine.Debug;

public static class ProtobufTool
{
    private static string PROTOC_PATH = Application.dataPath + "/Plugins/Protobuf/protoc.exe";
    private static string PROTO_FILE_PATH = Application.dataPath + "/Config/Protobuf/";
    private static string CSHARP_CLASS_PATH = Application.dataPath + "/Scripts/Protobuf";

    [MenuItem("Editor/Protobuf/GenerateCsharp")]
    public static void GenerateCsharp()
    {
        Debug.Log("Start GenerateCsharp");
        Generate("csharp");
    }

    [MenuItem("Editor/Protobuf/GenerateJava")]
    public static void GenerateJava()
    {
        Debug.Log("GenerateJava");
    }

    [MenuItem("Editor/Protobuf/GenerateCpp")]
    public static void GenerateCpp()
    {
        Debug.Log("GenerateCpp");
    }

    private static void Generate(string type){

        if (!File.Exists(PROTOC_PATH))
        {
            Debug.LogError("protoc.exe not found in path: " + PROTOC_PATH);
            return;
        }

        if (!Directory.Exists(PROTO_FILE_PATH))
        {
            Directory.CreateDirectory(PROTO_FILE_PATH);
        }

        // get all proto files
        string[] filesPath = Directory.GetFiles(PROTO_FILE_PATH, "*.proto");
        if (filesPath.Length == 0)
        {
            Debug.LogError("No proto files found in path: " + PROTO_FILE_PATH);
            return;
        }

        // check csharp path
        if (!Directory.Exists(CSHARP_CLASS_PATH))
        {
            Directory.CreateDirectory(CSHARP_CLASS_PATH);
        }
        // clear csharp files
        string[] csharpFiles = Directory.GetFiles(CSHARP_CLASS_PATH, "*.cs");
        foreach (var file in csharpFiles)
        {
            File.Delete(file);
        }

        // generate csharp files
        foreach (var filePath in filesPath)
        {
            // cmd command
            string command = $"-I={PROTO_FILE_PATH} --{type}_out={CSHARP_CLASS_PATH} {filePath}";
            Process cmd = new Process();
            // 配置进程启动信息
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = PROTOC_PATH,
                Arguments = command,
                RedirectStandardError = true, // 重定向标准错误流
                UseShellExecute = false,
                CreateNoWindow = true
            };
            cmd.StartInfo = startInfo;
            cmd.Start();

            string error = cmd.StandardError.ReadToEnd();
            
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error + " at " + filePath + " generate failed!"); 
            }
            else
            {
                Debug.Log("success generate csharp file: " + filePath);
            }
                
        }

        AssetDatabase.Refresh();
        Debug.Log("Generate csharp files done!");
    }
}