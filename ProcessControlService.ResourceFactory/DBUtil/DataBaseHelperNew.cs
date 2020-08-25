using Dapper;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace ProcessControlService.ResourceFactory
{
    public static class DataBaseHelperNew
    {
        private static readonly Dictionary<string, Type> NameTypeList = new Dictionary<string, Type>();
        private static readonly Dictionary<string, string> NameConnStrList = new Dictionary<string, string>();

        private static void AddNameType(string name, string type)
        {
            if (NameTypeList.ContainsKey(name))
            {
                throw new Exception("Repeated DbConnection Name!");
            }

            switch (type.ToLower())
            {
                case "sqlserver":
                    NameTypeList.Add(name, typeof(SqlConnection));
                    break;
                case "mysql":
                    NameTypeList.Add(name, typeof(MySqlConnection));
                    break;
                case "oracle":
                    NameTypeList.Add(name, typeof(OracleConnection));
                    break;
                default:
                    break;
            }
        }

        public static List<string> GetConnectionNames()
        {
            return NameConnStrList.Keys.ToList();
        }

        public static int ExecuteNonQuery(string databaseName, CommandType cmdType, string cmdText)
        {

            using (IDbConnection db = (IDbConnection)Activator.CreateInstance(NameTypeList[databaseName], new object[] { NameConnStrList[databaseName] }))
            {
                return db.Execute(cmdText, commandType: cmdType);
            }
        }

        public static DataTable GetDataTable(string databaseName, CommandType cmdType, string cmdText)
        {
            using (IDbConnection db = (IDbConnection)Activator.CreateInstance(NameTypeList[databaseName], new object[] { NameConnStrList[databaseName] }))
            {
                DataTable table = new DataTable();
                IDataReader sqlDataReader = db.ExecuteReader(cmdText, commandType: cmdType);
                table.Load(sqlDataReader, LoadOption.Upsert);
                sqlDataReader.Dispose();
                
                return table;
            }
        }

        public static string ExecuteOne(string databaseName, string strSql)
        {
            using (IDbConnection db = (IDbConnection)Activator.CreateInstance(NameTypeList[databaseName], new object[] { NameConnStrList[databaseName] }))
            {
                return db.ExecuteScalar(strSql).ToString();
            }
        }

        public static bool LoadFromConfig(XmlNode node)
        {
            XmlElement level0_item = (XmlElement)node;

            string Name = level0_item.GetAttribute("Name");
            string DBType = level0_item.GetAttribute("Type");
            string ConnStr = level0_item.GetAttribute("ConnectionString");
            AddNameType(Name, DBType);
            NameConnStrList.Add(Name, ConnStr);
            return true;
        }

    }
}
