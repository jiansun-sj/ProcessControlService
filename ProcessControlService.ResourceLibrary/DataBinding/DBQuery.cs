using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using ProcessControlService.ResourceFactory;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using log4net;
using Newtonsoft.Json;
using ProcessControlService.ResourceFactory.Attributes;
using ProcessControlService.ResourceFactory.DBUtil;

namespace ProcessControlService.ResourceLibrary.DataBinding
{
    public class DBQuery : Resource, IRedundancy
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DBQuery));

        public DBQuery(string name) : base(name)
        {
        }

        #region IResource 接口实现

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level0Item = (XmlElement)node;
                if (level0Item.HasAttribute("NeedDataSync"))
                {
                    var strNeedDataSync = level0Item.GetAttribute("NeedDataSync");
                    _needDataSync = (strNeedDataSync.ToLower() == "true");

                    if (_needDataSync)
                    {
                        Log.Info("DBQuery资源启动冗余同步。");
                    }
                }

                var level1Node = node.SelectSingleNode("//DBConnection");
                if (level1Node != null)
                {
                    var level1Item = (XmlElement)level1Node;

                    //string Name = level1_item.GetAttribute("Name");
                    var dbType = DataBaseHelper.ConvertDatabaseType(level1Item.GetAttribute("DBType"));
                    var connStr = level1Item.GetAttribute("ConnectionString");

                    _dbConn = new DataBaseHelper(dbType, connStr);

                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("装载DBQuery配置出错：" + ex);
                return false;
            }
        }

        public override void FreeResource()
        { }
        #endregion

        #region Redanduncy
        private RedundancyMode _mode = RedundancyMode.Unknown;

        public RedundancyMode CurrentRedundancyMode => _mode;

        public void RedundancyModeChange(RedundancyMode mode)
        {
            _mode = mode;
        }

        private bool _needDataSync = false; // add by dongmin 20191107
        public bool NeedDataSync => _needDataSync;

        private long _masterExecuteIndex = 0;
        private long _slaveExecuteIndex = 0;

        private SyncSQLCommandModel _syncSqlCommand = null;

        readonly object _buildSyncDataLocker=new object();

        public string BuildSyncData()
        {// 往从机发送数据

            //sunjian 2019-12-11 改为发送500ms内执行了的所有sql语句。
            try
            {
                string jsonStr;

                lock (_buildSyncDataLocker)
                {
                    jsonStr = JsonConvert.SerializeObject(_syncSqlCommandModels); //   重写

                    _syncSqlCommandModels.Clear();
                }
                
                return jsonStr;
            }
            catch (Exception e)
            {
               Log.Error(e.StackTrace);
               return "";
            }
            
        }

        public void ExtractSyncData(string data)
        { //接收到从机的数据
            try
            {
                var syncSqlCommandModels = JsonConvert.DeserializeObject < List<SyncSQLCommandModel>>(data);

                if (syncSqlCommandModels == null)
                {
                    return;
                }

                foreach (var syncSqlCommandModel in syncSqlCommandModels)
                {
                    var databaseName = syncSqlCommandModel.DatabaseName;
                    var cmdText = syncSqlCommandModel.CommandText;
                    var masterExecuteIndex = syncSqlCommandModel.MasterExecuteIndex;

                    //if (masterExecuteIndex == _slaveExecuteIndex) return;

                    //执行数据库写入操作
                    Log.Info($"执行主站数据库操作：[{databaseName}], {cmdText}");

                    ExecuteNonQuery(databaseName, cmdText);

                    _slaveExecuteIndex = masterExecuteIndex;
                }
            }
            catch (IOException ex)
            {
                Log.Error($"从主站数据解析：{data}出错：{ex}");
            }
        }

        private static SyncSQLCommandModel DisassembleData(string strJsonData)
        {
            return JsonConvert.DeserializeObject<SyncSQLCommandModel>(strJsonData);
        }

        #endregion

        #region "Resource Service"
        
        private SQLCommandModel UnpackData(string strData)
        {
            var ser = new DataContractJsonSerializer(typeof(SQLCommandModel));

            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(strData)))
                {
                    var obj = ser.ReadObject(ms) as SQLCommandModel;
                    return obj;
                }
            }
            catch (IOException ex)
            {
                Log.Error($"反序列化SQLCommandModel：{strData}出错：{ex}");
                return null;
            }

        }

        public virtual string GetStatus()
        {
            return null;
        }

        #endregion

        #region 功能代码
        private DataBaseHelper _dbConn;

        private readonly List<SyncSQLCommandModel> _syncSqlCommandModels=new List<SyncSQLCommandModel>();
        /// <summary>
        /// 数据库执行sql语句
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        /// <exception cref="SqlException">
        /// sql exception 的错误代码详见 (sql server 2014)
        /// <see cref="https://docs.microsoft.com/en-us/sql/relational-databases/replication/mssql-eng002601?view=sql-server-2014"/>
        /// </exception>
        [ResourceService]
        public string ExecuteNonQuery(string databaseName, string cmdText)
        {
            try
            {
                if (_masterExecuteIndex < long.MaxValue)
                {
                    _masterExecuteIndex++;
                }
                else
                {
                    _masterExecuteIndex = 0;
                }

                _syncSqlCommand = new SyncSQLCommandModel()
                    {MasterExecuteIndex = _masterExecuteIndex, DatabaseName = databaseName, CommandText = cmdText};

                lock (_buildSyncDataLocker)
                {
                    _syncSqlCommandModels.Add(_syncSqlCommand);
                }

                return new DataBaseHelper(databaseName).ExecuteNonQuery(CommandType.Text, cmdText).ToString();
            }
            catch (SqlException sqlException)
            {
                Log.Error($"{sqlException}");
                //sql server 2014 插入重复主键的错误代码为 2627， unique字段插入重复值错误代码为2601. 
                //sunjian 2019-11-15
                return sqlException.Errors.Cast<SqlError>().Any(sqlExceptionError =>
                    sqlExceptionError.Number == 2627 || sqlExceptionError.Number == 2601)
                    ? "-2"/*重复插值错误代码返回-2*/
                    : "-1"/*非重复插值的sql语句执行错误返回代码-1*/;
            }
            catch (Exception ex)
            {
                Log.Error($"数据库操作：{databaseName} 动作：{cmdText}的数据库操作出错 {ex.Message}");
                return "-1";
            }
        }

        public DataSet GetDataSet(string databaseName, string cmdText)
        {
            try
            {
                DataSet ds = new DataBaseHelper(databaseName).GetDataSet( CommandType.Text, cmdText);
                return ds;
            }
            catch (Exception ex)
            {
                Log.Error($"数据库操作：{databaseName} 动作：{cmdText}的数据库操作出错 {ex.Message}");
                return null;
            }
        }

        [ResourceService]
        public string GetDataSetInJson(string databaseName, string cmdText)
        {
            try
            {
                return JsonConvert.SerializeObject(GetDataSet(databaseName,cmdText));
            }
            catch (Exception ex)
            {
                Log.Error($"数据库操作：{databaseName} 动作：{cmdText}的数据库操作出错 {ex.Message}");
                return null;
            }
        }

        [ResourceService]
        public string GetConnectionString(string databaseName)
        {
            try
            {
                return DataBaseHelper.GetNameConnStrList()[databaseName];
            }
            catch (Exception ex)
            {
                Log.Error($"获取{databaseName}连接字符串失败{ex.Message}.");
                return null;
            }
        }

        public string GetConnectionString()
        {
            try
            {
                return _dbConn.DbConnectionString;
            }
            catch (Exception ex)
            {
                Log.Error($"获取连接字符串失败{ex.Message}.");
                return null;
            }
        }
        #endregion

    }

    [DataContract]
    public class SQLCommandModel
    {
        [DataMember]
        public string DatabaseName;
        [DataMember]
        public string CommandText;
    }

    [DataContract]
    public class SyncSQLCommandModel
    {
        [DataMember]
        public long MasterExecuteIndex;
        [DataMember]
        public string DatabaseName;
        [DataMember]
        public string CommandText;
    }
}
