using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Oracle.ManagedDataAccess.Client;

namespace ProcessControlService.ResourceFactory.DBUtil
{
    /// <summary>
    ///SQLSERVERHelper 的摘要说明
    /// </summary>  
    public class OracleUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OracleUtil));


        //数据库连接字符串
        //public static string Conn = "";


        /// <summary>
        ///  给定连接的数据库用假设参数执行一个sql命令（不返回数据集）
        /// </summary>
        /// <param name="connectionString">一个有效的连接字符串</param>
        /// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        /// <param name="cmdText">存储过程名称或者sql命令语句</param>
        /// <param name="commandParameters">执行命令所用参数的集合</param>
        /// <returns>执行命令所影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                OracleDataAdapter SelectAdapter = new OracleDataAdapter();//定义一个数据适配器
                SelectAdapter.SelectCommand = cmd;//定义数据适配器的操作指令
                conn.Open();//打开数据库连接
                int val = SelectAdapter.SelectCommand.ExecuteNonQuery();//执行数据库查询指令
                conn.Close();//关闭数据库
                return val;
            }
        }

        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <param name="connectionString">一个有效的连接字符串</param>
        /// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        /// <param name="cmdText">存储过程名称或者sql命令语句</param>
        /// <returns></returns>
        public static DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = cmdText;
                    OracleDataAdapter SelectAdapter = new OracleDataAdapter();//定义一个数据适配器
                    SelectAdapter.SelectCommand = cmd;//定义数据适配器的操作指令
                    conn.Open();//打开数据库连接
                    int val = SelectAdapter.SelectCommand.ExecuteNonQuery();//执行数据库查询指令
                    conn.Close();//关闭数据库
                    DataSet MyDataSet = new DataSet();//定义一个数据集
                    SelectAdapter.Fill(MyDataSet);
                    return MyDataSet;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return null;
                }
               
            }
        }
        private static void PrepareCommand(OracleCommand cmd, OracleParameter[] cmdParms)
        {
            if (cmdParms != null)
            {
                foreach (OracleParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>
        /// 调用存储过程
        /// </summary>
        /// <param name="conStr">连接字符串</param>
        /// <param name="name">存储过程名</param>
        /// <param name="paraValues">OracleParameter[]</param>
        public static List<String> StoredProcedure(String conStr, String name, OracleParameter[] paraValues)
        {
            using (OracleConnection con = new OracleConnection(conStr))
            {
                try
                {
                    OracleCommand comm = con.CreateCommand();
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.CommandText = name;
                    foreach (OracleParameter param in paraValues)
                        comm.Parameters.Add(param);
                    con.Open();
                    comm.ExecuteNonQuery();
                    //LOG.Error(comm.Parameters["var_out"].Value.ToString());
                    List<string> carInfoList = new List<string>();
                    carInfoList.Add(comm.Parameters["CO_SHORT_CODE"].Value.ToString());
                    Log.Info("存储过程返回******"+comm.Parameters["CO_SHORT_CODE"].Value.ToString());
                    carInfoList.Add(comm.Parameters["CO_SERIES"].Value.ToString());
                    Log.Info("存储过程返回******" + comm.Parameters["CO_SERIES"].Value.ToString());
                    carInfoList.Add(comm.Parameters["CO_MODEL"].Value.ToString());
                    carInfoList.Add(comm.Parameters["CO_BODY_NO"].Value.ToString());
                    carInfoList.Add(comm.Parameters["CO_COLOR"].Value.ToString());
                    carInfoList.Add(comm.Parameters["CO_PO"].Value.ToString());
                    carInfoList.Add(comm.Parameters["CO_VIN"].Value.ToString());
                    return carInfoList;
                }
                catch (Exception ex)
                {
                    Log.Error("执行存储过程出错--------" + ex.Message);
                    return null;
                }
                finally
                {
                    con.Close();
                }
            }
        }
    }
}
