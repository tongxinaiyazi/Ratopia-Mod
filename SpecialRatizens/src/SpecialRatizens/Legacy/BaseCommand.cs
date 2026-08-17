using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RatopiaMod
{
    internal class BaseCommand
    {
        /// <summary>
        /// 字段访问类型
        /// </summary>
        public static BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static TextureWrapMode TexWrapMode = TextureWrapMode.Clamp;

        public static FilterMode FilMode = FilterMode.Bilinear;

        /// <summary>
        /// 加载Sprite
        /// </summary>
        /// <param name="path"></param>
        /// <param name="customPath"></param>
        /// <returns></returns>
        public static Sprite LoadSprite(string path, string customPath = null)
        {
            Sprite sprite = null;

            //优先从Resources中加载
            sprite = Resources.Load<Sprite>(path);

            if (sprite != null)
                return sprite;

            sprite = LoadSpriteFromTexture2D(LoadTextureFromFile(customPath ?? path));

            Debug.Log($"从自定义路径 {path} 加载了图片 {sprite}");

            if (sprite == null)
                sprite = Resources.Load<Sprite>("Missing");

            return sprite;

            //Debug.Log(SceneItemEditor.imgPath + pro.ID);
        }

        /// <summary>
        /// 从Texture2D中加载Sprite
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        public static Sprite LoadSpriteFromTexture2D(Texture2D tex)
        {
            return tex == null ? null : Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 加载Texture2D图片文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static Texture2D LoadTextureFromFile(string path, string suffix = ".png")
        {
            string fileName = path.IndexOf(suffix) >= 0 ? path : $"{path}{suffix}";

            byte[] bytes = LoadFile(fileName);

            if (bytes != null)
            {
                Texture2D tex = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);

                tex.wrapMode = TexWrapMode;

                tex.filterMode = FilMode;

                tex.LoadImage(bytes, false);

                int index = fileName.LastIndexOf('/');

                string fullName = fileName.Substring(index != -1 ? index + 1 : 0);

                tex.name = FileNameWithoutExtensions(fullName);

                return tex;
            }
            else
            {
                Debug.LogWarning($"load non byte from {fileName}");

                return null;
            }
        }

        /// <summary>
        /// 获取文件名（去后缀名）
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static string FileNameWithoutExtensions(string fileName)
        {
            if (fileName.LastIndexOf(".") == -1)
                return fileName;

            return fileName.Substring(0, fileName.LastIndexOf("."));
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        /// <param name="path">路径（包含后缀名）</param>
        /// <returns></returns>
        public static byte[] LoadFile(string path)
        {
            try
            {
                byte[] bytes = null;

                if (File.Exists(path))
                {
                    bytes = File.ReadAllBytes(path);

                    //Debug.LogWarning(path + " - data length : " + bytes.Length);
                }

                return bytes;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);

                return null;
            }
        }

        /// <summary>
        /// 字符串转枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T StringToEnum<T>(string str, bool firstCharUpper = false)
        {
            TryParseStringToEnum(str, out T result, firstCharUpper);

            return result;
        }

        /// <summary>
        /// 尝试转换string为enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <param name="firstCharUpper"></param>
        /// <returns></returns>
        public static bool TryParseStringToEnum<T>(string str, out T result, bool firstCharUpper = false)
        {
            if (firstCharUpper)
                str = FirstCharToUpper(str);

            try
            {
                result = (T)Enum.Parse(typeof(T), str);
            }
            catch
            {
                result = default;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstCharToUpper(string str)
        {
            if (str == null || str.Equals(""))
                return str;

            return $"{str.Substring(0, 1).ToUpper()}{str.Substring(1)}";
        }

        public static void SetFieldValue<T>(T obj, string field, object value)
        {
            typeof(T).GetField(field).SetValue(obj, value);
        }

        public static bool SaveObjectToJson(string path, object obj, string foldPath = "")
        {
            if (obj == null)
                return false;

            try
            {
                return SaveFile(path, foldPath, JsonConvert.SerializeObject(obj));
            }
            catch
            {
                Debug.LogError($"Save Data {obj.GetType()} Faild");

                return false;
            }
        }

        /// <summary>
        /// CSV文本转类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool LoadObjectByJson<T>(string path, out T result)
        {
            result = default;

            try
            {
                string text = File.ReadAllText(path, Encoding.UTF8);
                
                result = JsonConvert.DeserializeObject<T>(text);

                return true;
            }
            catch
            {
                Debug.Log($"Load Data {typeof(T)} Faild, From {path}");

                return false;
            }
        }

        /// <summary>
        /// 加载csv数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="list"></param>
        /// <param name="fieldLine"></param>
        /// <param name="idFormat"></param>
        /// <returns></returns>
        public static bool LoadCsvData<T>(string path, out List<T> list, int fieldLine = 0, string idFormat = "")
        {
            List<List<string>> heads = new List<List<string>>();

            LoadCsvData(typeof(T), path, fieldLine, ref heads, idFormat, out _, out List<object> result);

            //Debug.Log(result.Count);

            list = result != null ? result.Select(t => (T)t).ToList() : null;

            if (list == null)
                Debug.Log($"{typeof(T)} {path} 读取失败");

            return list != null;
        }

        /// <summary>
        /// 加载CSV数据为字典
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="path"></param>
        /// <param name="fieldLine"></param>
        /// <param name="heads"></param>
        /// <param name="idFormat"></param>
        /// <returns></returns>
        public static Dictionary<string, object> LoadCsvDataToDic(Type dataType, string path, int fieldLine, ref List<List<string>> heads, string idFormat)
        {
            LoadCsvData(dataType, path, fieldLine, ref heads, idFormat, out Dictionary<string, object> results, out _);

            return results;
        }

        /// <summary>
        /// 加载CSV数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="path"></param>
        /// <param name="fieldLine"></param>
        /// <param name="heads"></param>
        /// <param name="idFormat"></param>
        /// <param name="returnDic"></param>
        /// <returns></returns>
        public static void LoadCsvData(Type dataType, string path, int fieldLine, ref List<List<string>> heads, string idFormat, out Dictionary<string, object> results, out List<object> list)
        {
            CsvData data = LoadCsvData(path);

            results = new Dictionary<string, object>();

            list = new List<object>();

            object result = Activator.CreateInstance(dataType);

            FieldInfo fieldInfo;

            PropertyInfo proInfo = null;

            Type type;

            //Debug.Log(data.data[0].Count + " / " + data.data[1].Count);

            //Debug.Log($"{path} {result.GetType()} {data.data[0].Count}/{data.data[1].Count} {fieldLine}/{data.DataCount} {data.data.Count}");

            //try
            //{
            //变量名
            string fieldName, firstFieldValue;

            object value;

            heads = new List<List<string>>();

            int intID;

            object id;

            for (int r = 0; r < data.data.Count; r++)
            {
                if (r < fieldLine + 1)
                {
                    heads.Add(data.data[r]);

                    //Debug.Log(string.Join(",", data.data[r]));

                    continue;
                }

                firstFieldValue = data.data[r][0].Trim();

                //列头为空时跳过整行数据（通常列头为ID）
                if (firstFieldValue.Equals(""))
                    continue;

                id = firstFieldValue;

                result = Activator.CreateInstance(dataType);

                for (int c = 0; c < data.data[r].Count; c++)
                {
                    //列值为空时跳过此列数据（即使用默认数据）
                    if (data.data[r][c].Trim().Equals(""))
                    {
                        //Debug.Log("跳过 " + r + " - " + c + " / " + data.data[r][c] + " / " + data.data[r].Count);

                        continue;
                    }

                    //Debug.Log($"{r} - {c}: {data.data[r][c]}");

                    fieldName = data.data[fieldLine][c];

                    //最后一列疑似因File.ReadAllText读出换行符，所以进行Trim处理
                    fieldInfo = result.GetType().GetField(fieldName.Trim(), Flags);

                    //Debug.Log(fieldName + " / " + fieldName.Trim() + " / " + fieldInfo);

                    if (fieldInfo == null)
                    {
                        proInfo = result.GetType().GetProperty(fieldName.Trim(), Flags);

                        if (proInfo == null)
                        {
                            Debug.Log(result.GetType().ToString() + " 获得字段 " + fieldName + " 失败！");

                            continue;
                        }

                        type = proInfo.PropertyType;
                    }
                    else
                        type = fieldInfo.FieldType;

                    //Debug.Log($"当前值 {data.data[r][c]}，目标类型 {type}");

                    try
                    {
                        //只有字符串类型可以写为"1.0f"，浮点只能转换"1.0"，否则转换失败
                        value = Convert.ChangeType(data.data[r][c], type);
                    }
                    catch
                    {
                        value = JsonConvert.DeserializeObject(data.data[r][c], type);

                        //Debug.Log($"{data.data[r][c]} 进行Json格式化");
                    }

                    if (value != null)
                    {
                        //列头视为ID
                        if (c == 0 && !idFormat.Equals(""))
                        {
                            //如果是数字类型，则进行格式化
                            if (int.TryParse(firstFieldValue, out intID))
                                id = intID.ToString(idFormat);

                            value = id;
                        }

                        if (fieldInfo != null)
                            fieldInfo.SetValue(result, value);
                        else if (proInfo != null)
                            proInfo.SetValue(result, value);
                    }
                }

                list.Add(result);

                if (!results.ContainsKey(id.ToString()))
                    results.Add(id.ToString(), result);
            }
            //}
            //catch (Exception ex)
            //{
            //    Debug.LogWarning(path + " (" + result.GetType().ToString() + "): " + ex);
            //}

            //Debug.Log(results.Count + " / " + list.Count + " / " + data.data.Count);
        }

        /// <summary>
        /// 读取CSV数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static CsvData LoadCsvData(string path)
        {
            CsvData data = new CsvData();

            string str;

            //读取外部资源数据
            try
            {
                if (File.Exists(path))
                    str = File.ReadAllText(path, Encoding.UTF8);
                else
                    str = Resources.Load(path.Substring(0, path.IndexOf("."))).ToString();

                data.LoadData(str);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }

            return data;
        }

        public static bool SaveCsvData<T>(string path, string folderPath, List<T> list)
        {
            try
            {
                string str = "", objStr = "";

                foreach (FieldInfo field in typeof(T).GetFields())
                {
                    str += $"{field.Name},";
                }

                str += "\n";

                object tempObj = null;

                Type type = typeof(T);

                foreach (T obj in list)
                {
                    objStr = "";

                    if (obj == null)
                        continue;

                    if (obj.GetType() != typeof(string))
                    {
                        foreach (FieldInfo field in typeof(T).GetFields())
                        {
                            if (field.GetType().Equals(type))
                                continue;

                            tempObj = type.GetField(field.Name).GetValue(obj);

                            if (tempObj == null)
                                objStr += "NULL";
                            else if (!tempObj.GetType().IsPrimitive && tempObj.GetType() != typeof(string))
                                objStr += $"{ReplaceCsvText(ObjectToJson(tempObj))},";
                            else
                                objStr += $"{ReplaceCsvText(tempObj.ToString())},";
                        }
                    }
                    else
                        objStr = obj.ToString();

                    str += $"{objStr}\n";
                }

                SaveFile(path, folderPath, str);
            }
            catch (Exception ex)
            {
                Debug.Log($"{typeof(T)} {ex}");

                return false;
            }

            return true;
        }

        public static string ObjectToJson(object obj)
        {
            if (obj == null)
                return "";

            return string.Format("\"{0}\"", JsonConvert.SerializeObject(obj).Replace("\"", "\"\""));
        }

        /// <summary>
        /// Json转类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ReplaceCsvText(string str)
        {
            return str.Replace(',', '，').Replace("\n", "").Replace("\r", "");
        }

        public static bool SaveFile(string path, string folderPath, string content)
        {
            try
            {
                //Debug.Log($"Save Data {content} In Path {path}");

                if (content == null)
                    return false;

                if (!folderPath.Equals("") && !Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                FileStream fs = new FileStream(path, FileMode.Create);

                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(content);

                    sw.Flush();

                    sw.Close();
                }

                return true;
            }
            catch (Exception ex)
            {

                Debug.LogWarning($"{path} 保存错误 {ex}");

                return false;
            }
        }

        /// <summary>
        /// 保存CSV数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="heads"></param>
        public static bool SaveCsvData<T>(string path, List<T> objs, List<List<string>> heads)
        {
            if (heads == null || heads.Count == 0)
                return false;

            List<string> datas = new List<string>();

            //添加表头
            for (int i = 0; i < heads.Count; i++)
            {
                string str = string.Join(",", heads[i].ToArray());

                datas.Add(str);
            }

            //获得字段名列表
            List<string> fields = heads[heads.Count - 1];

            T obj;

            FieldInfo fieldInfo;

            PropertyInfo proInfo;

            List<string> data;

            object value = null;

            //迭代实体（行数据）
            for (int i = 0; i < objs.Count; i++)
            {
                obj = objs[i];

                data = new List<string>();

                //反射字段值（列数据）
                for (int c = 0; c < fields.Count; c++)
                {
                    fieldInfo = obj.GetType().GetField(fields[c].Trim(), Flags);

                    if (fieldInfo == null)
                    {
                        proInfo = obj.GetType().GetProperty(fields[c].Trim(), Flags);

                        if (proInfo != null)
                            value = proInfo.GetValue(obj);
                    }
                    else
                        value = fieldInfo.GetValue(obj);

                    data.Add(value == null ? "" : value.ToString());
                }

                datas.Add(string.Join(",", data));

                //Debug.Log(data.ToString());
            }

            return SaveFileData(path, datas);
        }
        
        /// <summary>
         /// 类转CSV文本
         /// </summary>
         /// <param name="obj"></param>
         /// <returns></returns>
        public static string ObjectToCsvText(object obj)
        {
            if (obj == null)
                return "";

            //单列数据前后添加双引号，数据中单个双引变两个双引
            return string.Format("\"{0}\"", JsonConvert.SerializeObject(obj).Replace("\"", "\"\""));
        }

        /// <summary>
        /// CSV文本转类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
        public static T CsvTextToObject<T>(string text)
        {
            if (text.Trim().Length == 0)
                return default;

            text = text.Replace("\"\"", "\"");

            int leftIndex = text.IndexOf('[');

            int rightIndex = text.LastIndexOf(']');

            return JsonToObject<T>(text.Substring(leftIndex, rightIndex - leftIndex + 1));
        }

        /// <summary>
        /// 保存文本数据
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="path"></param>
        public static bool SaveFileData(string path, List<string> strs = null, string str = null)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }

                //UTF-8方式保存
                using (StreamWriter stream = new StreamWriter(path, false, Encoding.UTF8))
                {
                    if (strs != null)
                    {
                        for (int i = 0; i < strs.Count; i++)
                        {
                            if (strs[i] != null)
                                stream.Write(strs[i] + "\n");
                        }
                    }
                    else if (str != null)
                        stream.Write(str);
                }

                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}
