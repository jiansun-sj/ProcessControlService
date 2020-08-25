using System;
using System.Data;
using System.Data.SqlClient;
using log4net;

namespace ProcessControlService.ResourceFactory.DBUtil
{
    /// <summary>
    ///SQLSERVERHelper 的摘要说明
    /// </summary>  
    public class SqlServerUtil
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SqlServerUtil));

        //数据库连接字符串
        //public static string Conn = "";

        /// <summary>
        ///  给定连接的数据库用假设参数执行一个sql命令（不返回数据集）
        /// </summary>
        /// <param name="connectionString">一个有效的连接字符串</param>
        /// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        /// <param name="cmdText">存储过程名称或者sql命令语句</param>
        /// <returns>执行命令所影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandTimeout = 60;
                    cmd.CommandText = cmdText;
                    var selectAdapter = new SqlDataAdapter {SelectCommand = cmd}; //定义一个数据适配器
                    //定义数据适配器的操作指令
                    conn.Open();//打开数据库连接
                    var val = selectAdapter.SelectCommand.ExecuteNonQuery();//执行数据库查询指令
                    conn.Close();//关闭数据库
                    return val;
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message+"\n"+ cmdText);
                return -1;
            }

        }


        public static string ExecuteOne(string connString, string strSql)
        {
            SqlConnection connection = new SqlConnection(connString);
            using (SqlCommand cmd = new SqlCommand(strSql, connection))
            {
                try
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    //ExecuteScalar() 执行查询方法,返回单个值
                    string result;
                    if (cmd.ExecuteScalar() != null)
                    {

                        result = cmd.ExecuteScalar().ToString();
                    }
                    else
                    {
                        result = "";
                    }

                    connection.Close();
                    return result;
                }
                catch (SqlException ex)
                {
                    LOG.Info(ex.Message + "\n" + strSql);
                    return "";
                }
                finally
                {
                    connection.Close();
                }
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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = cmdText;
                    SqlDataAdapter SelectAdapter = new SqlDataAdapter();//定义一个数据适配器
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
                    LOG.Info(ex.Message + "\n" + cmdText);
                    return null;
                }
               
            }
        }

        public static DataSet ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] paras)
        {
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;


                PrepareCommand(cmd,paras);

                string x = cmd.CommandText;

                SqlDataAdapter SelectAdapter = new SqlDataAdapter();//定义一个数据适配器
                SelectAdapter.SelectCommand = cmd;//定义数据适配器的操作指令
                conn.Open();//打开数据库连接
                int val = SelectAdapter.SelectCommand.ExecuteNonQuery();//执行数据库查询指令
                conn.Close();//关闭数据库
                DataSet MyDataSet = new DataSet();//定义一个数据集
                SelectAdapter.Fill(MyDataSet);
                return MyDataSet;
            }
        }

        private static void PrepareCommand(SqlCommand cmd, SqlParameter[] cmdParms)
        {
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
    }
}
