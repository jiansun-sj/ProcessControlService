using System;
using System.Data;
using System.Data.OleDb;
using log4net;

namespace ProcessControlService.ResourceFactory.DBUtil
{

    /// <summary>
    /// 该类适用于 Microsoft Access 2003或者更早的版本(.mdb)数据的读写.
    /// sunjian 2019-11-16
    /// </summary>
    public class AccessUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AccessUtil));

        /// <summary>
        /// Access数据库执行sql语句。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cmdText"></param>
        /// <returns>sql语句影响行数</returns>
        public static int ExecuteNonQuery(string connectionString, string cmdText)
        {
            using (var oleDbConnection=new OleDbConnection(connectionString))
            {
                try
                {
                    oleDbConnection.Open();

                    var oleDbCommand=new OleDbCommand(cmdText,oleDbConnection);

                    oleDbConnection.Close();

                    return  oleDbCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    oleDbConnection.Close();
                    Log.Error($"Access数据库执行sql语句失败，sql：{cmdText}",e);
                    return -1;
                }
            }
        }

        public static string ExecuteOne(string connString, string strSql)
        {
            return "";
        }

        /// <summary>
        /// select条件语句获得从Access表中获得DataSet。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cmdText"></param>
        /// <example>
        /// sql查询语句例子：
        /// @"SELECT * FROM agv_road_distribution WHERE(((agv_road_distribution.AGV编号) = '91'))"
        /// </example>
        /// <returns></returns>
        public static DataSet GetDataSet(string connectionString,  string cmdText)
        {
            using (var oleDbConnection=new OleDbConnection(connectionString))
            {
                var oleDbDataAdapter=new OleDbDataAdapter(cmdText,oleDbConnection);
                try
                {
                    oleDbConnection.Open();
                    var dataSet=new DataSet();
                    oleDbDataAdapter.Fill(dataSet);

                    oleDbConnection.Close();
                    return dataSet;
                }
                catch (Exception e)
                {
                    oleDbConnection.Close();
                    Log.Error($"Access获取DataSet出错,sql: {cmdText}",e);
                    return new DataSet();
                }
            }
        }
    }
}
