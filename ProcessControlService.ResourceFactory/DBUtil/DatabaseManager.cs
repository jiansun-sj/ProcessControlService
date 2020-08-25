using Oracle.ManagedDataAccess.Client;
using ProcessControlService.ResourceFactory.DBUtil;
using System.Collections.Generic;
using System.Data;

namespace ProcessControlService.ResourceFactory
{
    public class DatabaseManager
    {

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DatabaseManager));

        private static readonly Dictionary<string, string> NameTypeList = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> NameConnStrList = new Dictionary<string, string>();

        public static Dictionary<string, string> GetNameTypeList()
        {
            return NameTypeList;
        }

        public static Dictionary<string, string> GetNameConnStrList()
        {
            return NameConnStrList;
        }

        public static void AddNameType(string name, string type)
        {
            NameTypeList.Add(name, type);
        }

        public static void AddNameConnStr(string name, string nameConnStr)
        {
            NameConnStrList.Add(name, nameConnStr);
        }

        public static int ExecuteNonQuery(string databaseName, CommandType cmdType, string cmdText)
        {
            switch (NameTypeList[databaseName].ToLower())
            {
                case "sqlserver":
                    return SqlServerUtil.ExecuteNonQuery(NameConnStrList[databaseName], cmdType, cmdText);
                case "mysql":
                    return MySqlUtil.ExecuteNonQuery(NameConnStrList[databaseName], cmdType, cmdText);
                case "oracle":
                    return OracleUtil.ExecuteNonQuery(NameConnStrList[databaseName], cmdType, cmdText);
                default:
                    return -1;
            }
        }

        public static DataSet GetDataSet(string databaseName, CommandType cmdType, string cmdText)
        {
            switch (NameTypeList[databaseName].ToLower())
            {
                case "sqlserver":
                    return SqlServerUtil.GetDataSet(NameConnStrList[databaseName], cmdType, cmdText);
                case "mysql":
                    return MySqlUtil.GetDataSet(NameConnStrList[databaseName], cmdType, cmdText);
                case "oracle":
                    return OracleUtil.GetDataSet(NameConnStrList[databaseName], cmdType, cmdText);
                default:
                    return null;
            }
        }

        public static List<string> StoredProcedure(string databaseName, string name, OracleParameter[] paraValues)
        {
            switch (NameTypeList[databaseName].ToLower())
            {
                case "oracle":
                    return OracleUtil.StoredProcedure(NameConnStrList[databaseName], name, paraValues);
                default:
                    return null;
            }
        }


    }
}
