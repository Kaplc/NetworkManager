using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Script.NetworkManager
{
    [Serializable]
    public abstract class BaseNetworkData
    {
        
        public abstract int GetSize();

        public abstract byte[] Serialize();

        public abstract T Deserialize<T>(byte[] bytes, ref int index) where T : class;

        #region 转字节

        public void WriteInt(int value, byte[] bytes, ref int index)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            intBytes.CopyTo(bytes, index);
            index += sizeof(int);
        }

        public void WriteFloat(float value, byte[] bytes, ref int index)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);
            floatBytes.CopyTo(bytes, index);
            index += sizeof(float);
        }
        
        public void WriteBool(bool value, byte[] bytes, ref int index)
        {
            byte[] boolBytes = BitConverter.GetBytes(value);
            boolBytes.CopyTo(bytes, index);
            index += sizeof(bool);
        }

        public void WriteString(string value, byte[] bytes, ref int index)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(value);
            WriteInt(strBytes.Length, bytes, ref index);
            strBytes.CopyTo(bytes, index);
            index += strBytes.Length;
        }
        
        public void WriteClass<T>(T value, byte[] bytes, ref int index) where T : BaseNetworkData
        {
            byte[] classBytes = value.Serialize();
            classBytes.CopyTo(bytes, index);
            index += classBytes.Length;
        }

        #endregion
        
        #region 读字节

        public int ReadInt(byte[] bytes, ref int index)
        {
            int value = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);
            return value;
        }
        
        public float ReadFloat(byte[] bytes, ref int index)
        {
            float value = BitConverter.ToSingle(bytes, index);
            index += sizeof(float);
            return value;
        }
        
        public bool ReadBool(byte[] bytes, ref int index)
        {
            bool value = BitConverter.ToBoolean(bytes, index);
            index += sizeof(bool);
            return value;
        }
        
        public string ReadString(byte[] bytes, ref int index)
        {
            int strLength = ReadInt(bytes, ref index);
            string value = Encoding.UTF8.GetString(bytes, index, strLength);
            index += strLength;
            return value;
        }
        
        public T ReadClass<T>(byte[] bytes, ref int index) where T : BaseNetworkData, new()
        {
            T value = new T().Deserialize<T>(bytes, ref index);
            return value;
        }

        #endregion
        
        
        // public int GetSize()
        // {
        //     int size = 0;
        //     
        //     FieldInfo[] fields = this.GetType().GetFields();
        //     foreach (var field in fields)
        //     {
        //         if (field.FieldType == typeof(int))
        //         {
        //             size += sizeof(int);
        //         }
        //         else if (field.FieldType == typeof(float))
        //         {
        //             size += sizeof(float);
        //         }
        //         else if (field.FieldType == typeof(bool))
        //         {
        //             size += sizeof(bool);
        //         }
        //         else if (field.FieldType == typeof(string))
        //         {
        //             string str = (string)field.GetValue(this);
        //             
        //             // int保存字符串长度
        //             size += sizeof(int);
        //             // 字符串字节数组长度
        //             size += Encoding.UTF8.GetBytes(str).Length;
        //         }
        //     }
        //
        //     return size;
        // }
        
        // public byte[] Serialize()
        // {
        //     byte[] bytes = new byte[GetSize()];
        //     int index = 0;
        //     
        //     FieldInfo[] fields = this.GetType().GetFields();
        //     foreach (FieldInfo field in fields)
        //     {
        //         if (field.FieldType == typeof(int))
        //         {
        //             byte[] intBytes = BitConverter.GetBytes((int)field.GetValue(this));
        //             intBytes.CopyTo(bytes, index);
        //             index += sizeof(int);
        //         }
        //         else if (field.FieldType == typeof(float))
        //         {
        //             byte[] floatBytes = BitConverter.GetBytes((float)field.GetValue(this));
        //             floatBytes.CopyTo(bytes, index);
        //             index += sizeof(float);
        //         }
        //         else if (field.FieldType == typeof(bool))
        //         {
        //             byte[] boolBytes = BitConverter.GetBytes((bool)field.GetValue(this));
        //             boolBytes.CopyTo(bytes, index);
        //             index += sizeof(bool);
        //         }
        //         else if (field.FieldType == typeof(string))
        //         {
        //             string str = (string)field.GetValue(this);
        //             byte[] strBytes = Encoding.UTF8.GetBytes(str);
        //             byte[] strLengthBytes = BitConverter.GetBytes(strBytes.Length);
        //             strLengthBytes.CopyTo(bytes, index);
        //             index += sizeof(int);
        //             strBytes.CopyTo(bytes, index);
        //             index += strBytes.Length;
        //         }
        //     }
        //
        //     return bytes;
        // }
        //
        // public void Deserialize(byte[] bytes)
        // {
        //     int index = 0;
        //     // 反序列化
        //     FieldInfo[] fields = GetType().GetFields();
        //     foreach (var field in fields)
        //     {
        //         if (field.FieldType == typeof(int))
        //         {
        //             field.SetValue(this, BitConverter.ToInt32(bytes, index));
        //             index += sizeof(int);
        //         }else if (field.FieldType == typeof(float))
        //         {
        //             field.SetValue(this, BitConverter.ToSingle(bytes, index));
        //             index += sizeof(float);
        //         }else if (field.FieldType == typeof(bool))
        //         {
        //             field.SetValue(this, BitConverter.ToBoolean(bytes, index));
        //             index += sizeof(bool);
        //         }else if (field.FieldType == typeof(string))
        //         {
        //             int strLength = BitConverter.ToInt32(bytes, index);
        //             index += sizeof(int);
        //             field.SetValue(this, Encoding.UTF8.GetString(bytes, index, strLength));
        //             index += strLength;
        //         }
        //     }
        // }
        }
    
}