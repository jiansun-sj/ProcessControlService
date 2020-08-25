using System;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using log4net;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProcessControlService.ResourceFactory
{
    /// <summary>
    ///Json 工具    
    /// </summary>  
    public class JsonUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonUtil));

        public static string CreateJsonParameters(DataTable dt)
        {
            /* /****************************************************************************
             * Without goingin to the depth of the functioning of this Method, i will try to give an overview
             * As soon as this method gets a DataTable it starts to convert it into JSON String,
             * it takes each row and in each row it grabs the cell name and its data.
             * This kind of JSON is very usefull when developer have to have Column name of the .
             * Values Can be Access on clien in this way. OBJ.HEAD[0].<ColumnName>
             * NOTE: One negative point. by this method user will not be able to call any cell by its index.
             * *************************************************************************/
            StringBuilder JsonString = new StringBuilder();
            //Message Handling        
            if (dt != null && dt.Rows.Count > 0)
            {
                JsonString.Append("{ ");
                JsonString.Append("\"Head\":[ ");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    JsonString.Append("{ ");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j < dt.Columns.Count - 1)
                        {
                            JsonString.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + dt.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == dt.Columns.Count - 1)
                        {
                            JsonString.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + dt.Rows[i][j].ToString() + "\"");
                        }
                    }
                    /*end Of String*/
                    if (i == dt.Rows.Count - 1)
                    {
                        JsonString.Append("} ");
                    }
                    else
                    {
                        JsonString.Append("}, ");
                    }
                }
                JsonString.Append("]}");
                return JsonString.ToString();
            }
            else
            {
                return null;
            }
        }

        #region dataTable转换成Json格式
        /// <summary>      
                /// dataTable转换成Json格式      
                /// </summary>      
                /// <param name="dt"></param>      
                /// <returns></returns>      
        public static string ToJson(DataTable dt)
        {
            var jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"");
           
            jsonBuilder.Append(dt.TableName.ToString());
            jsonBuilder.Append("\":[");

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (var j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                   
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\":\"");
                    jsonBuilder.Append(dt.Rows[i][j].ToString());
                    jsonBuilder.Append("\", ");
               
 }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]");
            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }

        #endregion dataTable转换成Json格式

        #region DataSet转换成Json格式
        /// <summary>      
                /// DataSet转换成Json格式      
                /// </summary>      
                /// <param name="ds">DataSet</param>      
                /// <returns></returns>      
        public static string ToJson(DataSet ds)
        {
            StringBuilder json = new StringBuilder();

            foreach (DataTable dt in ds.Tables)
            {
                json.Append("{\"");
               
                json.Append(dt.TableName);
                json.Append("\":");
               
                json.Append(ToJson(dt));
                json.Append("}");
            }
            return json.ToString();
        }
        #endregion

        public static DataTable JsonToDataTable(string strJson)
        {
            //取出表名  
            Regex rg = new Regex(@"(?<={)[^:]+(?=:/[)])", RegexOptions.IgnoreCase);/* todo: sunjian 2019-11-16 修复bug正则表达式未关闭。*/
            string strName = rg.Match(strJson).Value;
            return GetJsonDataTable(strJson, strName);
        }

        public static DataSet JsonToDataSet(string strJson)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(JsonToDataTable(strJson));

            return ds;
        }

        public static DataSet JsonToDataSet(string strJson,string arrayName)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(JsonToDataTable(strJson,arrayName));

            return ds;
        }

        /// <summary>
        /// 有些json 文本中没有定义数组名，而是指定了表名，所以重写下列函数
        /// </summary>
        /// <param name="strJson"></param>
        /// <param name="arrayName"></param>
        /// <returns></returns>
        public static DataTable JsonToDataTable(string strJson, string arrayName)
        {
            //取出表名  
            Regex rg = new Regex(@"(?<={)[^:]+(?=:/[)])", RegexOptions.IgnoreCase);/* todo: sunjian 2019-11-16 修复bug正则表达式未关闭。*/
            string strName = string.IsNullOrEmpty(rg.Match(strJson).Value) ? arrayName : rg.Match(strJson).Value;
            return GetJsonDataTable(strJson, strName);
        }

        private static DataTable GetJsonDataTable(string strJson, string strName)
        {
            Regex rg;
            DataTable tb = null;
            //去除表名  
            strJson = strJson.Substring(strJson.IndexOf("[", StringComparison.Ordinal) + 1);
            strJson = strJson.Substring(0, strJson.IndexOf("]", StringComparison.Ordinal));

            //获取数据  
            rg = new Regex(@"(?<={)[^}]+(?=})");
            var mc = rg.Matches(strJson);
            for (var i = 0; i < mc.Count; i++)
            {
                var strRow = mc[i].Value;
                var strRows = strRow.Split(',');

                //创建表  
                if (tb == null)
                {
                    tb = new DataTable();
                    tb.TableName = strName;
                    foreach (string str in strRows)
                    {
                        var dc = new DataColumn();
                        var strCell = str.Split(':');
                        dc.ColumnName = strCell[0].Trim('\'');
                        tb.Columns.Add(dc);
                    }

                    tb.AcceptChanges();
                }

                //增加内容  
                var dr = tb.NewRow();
                for (var r = 0; r < strRows.Length; r++)
                {
                    dr[r] = strRows[r].Split(':')[1].Trim().Trim('\'').Replace("，", ",").Replace("：", ":").Replace("\"", "");
                }

                tb.Rows.Add(dr);
                tb.AcceptChanges();
            }

            return tb;
        }
    }
}
