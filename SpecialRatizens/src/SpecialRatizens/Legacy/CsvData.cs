
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RatopiaMod
{
    /// <summary>
    /// CSV格式数据（静态数据）
    /// </summary>
    public class CsvData
    {
        /// <summary>
        /// 行数（以行为一条数据）
        /// </summary>
        public int row = 0;

        /// <summary>
        /// 列数（最大列数，不同数据可能列数不同）
        /// </summary>
        public int column = 0;

        /// <summary>
        /// 数据
        /// </summary>
        public List<List<string>> data = null;

        /// <summary>
        /// 数据数量
        /// </summary>
        public int DataCount
        {
            get
            {
                return data != null ? data.Count : 0;
            }
        }

        string[] rowStrs;

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="str">数据文本</param>
        /// <returns></returns>
        public List<List<string>> LoadData(string str)
        {
            rowStrs = str.Split('\n');

            //Debug.Log(str);

            //Debug.Log(rowStrs.Length);

            if (rowStrs == null || rowStrs.Length < 1)
            {
                return null;
            }

            data = new List<List<string>>();

            //List<string> cloumnData = null;

            foreach (string rowStr in rowStrs)
            {
                /*
                cloumnData = new List<string>();

                string[] cloumStrs = rowStr.Split(',');

                if (cloumStrs == null || cloumStrs.Length < 1)
                {
                    cloumnData.Add("");
                }

                foreach (string cloumStr in cloumStrs)
                {
                    cloumnData.Add(cloumStr);
                }

                data.Add(cloumnData);
                */

                data.Add(LoadCloumData(rowStr));
            }

            //Debug.Log("共加载 " + DataCount + " 行数据！");

            return data;
        }

        /// <summary>
        /// 加载列数据
        /// </summary>
        /// <param name="rowStr"></param>
        /// <returns></returns>
        List<string> LoadCloumData(string rowStr)
        {
            string[] splitText = rowStr.Split(',');

            List<string> cloumData = new List<string>();

            string data = "";

            int index = 0;

            Regex reg = new Regex("\"");

            MatchCollection mc;

            bool isDouble;

            while (index < splitText.Length)
            {
                do
                {
                    if (index >= splitText.Length)
                    {
                        Debug.LogError(data + " 引号匹配异常，数据读取失败！");

                        return cloumData;
                    }

                    data = string.Format("{0}{1}{2}", data, data.Length > 0 ? "," : "", splitText[index]);

                    mc = reg.Matches(data);

                    isDouble = mc.Count == 0 || mc.Count % 2 == 0;

                    //Debug.Log(data + " 包含 " + mc.Count + " 个引号，当前下标 " + index + " / " + isDouble);

                    index++;
                }
                //包含非偶数引号
                while (!isDouble);

                cloumData.Add(data);

                data = "";
            }

            return cloumData;
        }

        /// <summary>
        /// 获得一行数据
        /// </summary>
        /// <param name="rowIndex">行数</param>
        /// <returns></returns>
        public string GetDataByRow(int rowIndex)
        {
            return rowStrs[rowIndex];
        }
    }
}
