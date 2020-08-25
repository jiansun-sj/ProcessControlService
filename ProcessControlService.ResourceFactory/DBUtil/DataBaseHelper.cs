using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using DapperExtensions.Sql;
using log4net;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace ProcessControlService.ResourceFactory.DBUtil
{
    public class DataBaseHelper
    {
        public static readonly Dictionary<string, DatabaseType> NameType = new Dictionary<string, DatabaseType>();
        public static readonly Dictionary<string, string> NameConnStr = new Dictionary<string, string>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(DataBaseHelper));

        private string Name;
        private DatabaseType _dbType;

        public string DbConnectionString { get; private set; }

        public DataBaseHelper(string name)
        {
            Name = name;
            _dbType = NameType[name];
            DbConnectionString = NameConnStr[Name];
        }
        public DataBaseHelper(DatabaseType dbType, string connectionString)
        {
            Name = "Default";
            _dbType = dbType;
            DbConnectionString = connectionString;
        }

        public bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level0Item = (XmlElement)node;

                _dbType = ConvertDatabaseType(level0Item.GetAttribute("DBType"));
                DbConnectionString = level0Item.GetAttribute("ConnectionString");
                return true;

            }
            catch (Exception ex)
            {
                Log.Error("装载DataBaseHelper配置出错：" + ex);
                return false;
            }
        }

        public static Dictionary<string, string> GetNameConnStrList()
        {
            return NameConnStr;
        }
        public int ExecuteNonQuery(CommandType cmdType, string cmdText)
        {
            switch (_dbType)
            {
                case DatabaseType.Sqlserver:
                    return SqlServerUtil.ExecuteNonQuery(DbConnectionString, cmdType, cmdText);
                case DatabaseType.Mysql:
                    return MySqlUtil.ExecuteNonQuery(DbConnectionString, cmdType, cmdText);
                case DatabaseType.Oracle:
                    return OracleUtil.ExecuteNonQuery(DbConnectionString, cmdType, cmdText);
                case DatabaseType.Access:
                    return AccessUtil.ExecuteNonQuery(DbConnectionString, cmdText);
                default:
                    return -1;
            }
        }

        public string ExecuteOne(string strSql)
        {
            switch (_dbType)
            {
                case DatabaseType.Sqlserver:
                    return SqlServerUtil.ExecuteOne(DbConnectionString, strSql);
                //sunjian 2020/2/13 增加MySql的ExecuteOne方法
                case DatabaseType.Mysql:
                    return MySqlUtil.ExecuteOne(DbConnectionString, strSql);

                default:
                    return "";
            }
        }

        public DataSet GetDataSet(CommandType cmdType, string cmdText)
        {
            switch (_dbType)
            {
                case DatabaseType.Sqlserver:
                    return SqlServerUtil.GetDataSet(DbConnectionString, cmdType, cmdText);
                case DatabaseType.Mysql:
                    return MySqlUtil.GetDataSet(DbConnectionString, cmdType, cmdText);
                case DatabaseType.Oracle:
                    return OracleUtil.GetDataSet(DbConnectionString, cmdType, cmdText);
                case DatabaseType.Access:
                    return AccessUtil.GetDataSet(DbConnectionString, cmdText);
                default:
                    return null;
            }
        }

        public static DatabaseType ConvertDatabaseType(string strDbType)
        {
            switch (strDbType.ToLower())
            {
                // ReSharper disable once StringLiteralTypo
                case "sqlserver":
                    return DatabaseType.Sqlserver;
                case "mysql":
                    return DatabaseType.Mysql;
                case "oracle":
                    return DatabaseType.Oracle;
                case "access":
                    return DatabaseType.Access;
                default:
                    return DatabaseType.Unknown;

            }
        }

        public IDbConnection GetConnection()
        {
            switch (_dbType)
            {
                case DatabaseType.Sqlserver:
                    DapperExtensions.DapperExtensions.SqlDialect = new SqlServerDialect();
                    return new SqlConnection() { ConnectionString = DbConnectionString };
                case DatabaseType.Mysql:
                    DapperExtensions.DapperExtensions.SqlDialect = new MySqlDialect();
                    return new MySqlConnection() { ConnectionString = DbConnectionString };
                case DatabaseType.Oracle:
                    DapperExtensions.DapperExtensions.SqlDialect = new OracleDialect();
                    return new OracleConnection() { ConnectionString = DbConnectionString };
                case DatabaseType.Access:
                    throw new NotImplementedException();
                case DatabaseType.Unknown:
                    return null;
                default:
                    return null;
            }
        }


        private static IFreeSql _mysql;
        private static IFreeSql _oracle;
        private static IFreeSql _sqlserver;
        public IFreeSql GetFreeSql(bool autoSyncStructure=true)
        {
            switch (_dbType)
            {
                case DatabaseType.Sqlserver:
                    if (_sqlserver == null)
                    {
                        _sqlserver = new FreeSql.FreeSqlBuilder()
                                    .UseConnectionString(FreeSql.DataType.SqlServer, DbConnectionString)
                                    .UseAutoSyncStructure(autoSyncStructure)
                                    .Build();
                    }
                    return _sqlserver;
                case DatabaseType.Mysql:
                    if (_mysql == null)
                    {
                        _mysql = new FreeSql.FreeSqlBuilder()
                                    .UseConnectionString(FreeSql.DataType.MySql, DbConnectionString, typeof(FreeSql.MySql.MySqlProvider<>))
                                    .UseAutoSyncStructure(autoSyncStructure)
                                    .Build();
                    }
                    return _mysql;
                case DatabaseType.Oracle:
                    if (_oracle == null)
                    {
                        _oracle = new FreeSql.FreeSqlBuilder()
                                    .UseConnectionString(FreeSql.DataType.Oracle, DbConnectionString)
                                    .UseAutoSyncStructure(autoSyncStructure)
                                    .Build();
                    }
                    return _oracle;
                case DatabaseType.Access:
                    throw new NotImplementedException();
                case DatabaseType.Unknown:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

    }

    public enum DatabaseType
    {
        // ReSharper disable once IdentifierTypo
        Sqlserver = 0,
        Mysql = 1,
        Oracle = 2,
        Access = 3,
        Unknown = 4
    }
}
