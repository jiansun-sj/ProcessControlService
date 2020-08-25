// ==================================================
// 文件名：GroupBindingDataSet.cs
// 创建时间：2019/11/19 10:08
// ==================================================
// 最后修改于：2019/11/19 10:08
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ResourceLibrary.DataBinding
{
    /// <summary>
    ///     sunjian added 2019-11-18
    /// </summary>
    public abstract class GroupBindingDataSet : BindingDataset
    {
        public string DestinationBindingFieldType;
        public int DestinationEndNumber;


        public int DestinationStartNumber;

        public string SourceBindingFieldType;
        public DateTime SourceEndDateTime;
        public int SourceEndNumber;

        public DateTime SourceStartDateTime;
        public int SourceStartNumber;


        public abstract DataTable GetDataTable();

        public abstract void SetValues(DataTable dataTable);
    }

    /// <summary>
    /// sunjian added 2019-11-18
    /// </summary>
    public class GroupBindingDbFields : GroupBindingDataSet
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupBindingDbFields));

        public string ConnectionString { get; set; }

        public DatabaseType DbType { get; set; }

        public string TableName { get; set; }

        public string SourceDbField { get; set; }
        public string DestinationDbField { get; set; }

        public override List<BasicValue> GetValues()
        {
            throw new NotImplementedException();
        }

        public override void SetValues(List<BasicValue> values)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetDataTable()
        {
            try
            {
                var fieldNames = $"{SourceDbField},";
                // make "field1,field2,..."
                var i = 0;
                foreach (var item in DataItems)
                {
                    fieldNames += ((DBFieldBindingItem) item).DBFieldName;
                    if (i != DataItems.Count() - 1) fieldNames += ",";
                    i++;
                }

                var strSql = string.Empty;
                switch (DbType)
                {
                    case DatabaseType.ORACLE:
                        /*                        strSQL = $"select * from (select {fieldNames} from {TableName} where rownum = 1";
                                                break;*/
                        throw new ArgumentOutOfRangeException();
                    case DatabaseType.ACCESS:
                        strSql = SourceBindingFieldType == "int"
                            ? $"select {fieldNames} from {TableName} where {SourceDbField}>={SourceStartNumber} and {SourceDbField}<={SourceEndNumber} order by {SourceDbField}"
                            : $"select {fieldNames} from {TableName} where {SourceDbField}>=#{SourceStartDateTime:d}# and {SourceDbField}<=#{SourceEndDateTime:d}# order by {SourceDbField}";
                        break;
                    case DatabaseType.SQLSERVER:
                        strSql = SourceBindingFieldType == "int"
                            ? $"select {fieldNames} from {TableName} where {SourceDbField}>={SourceStartNumber} and {SourceDbField}<={SourceEndNumber} order by {SourceDbField}"
                            : $"select {fieldNames} from {TableName} where {SourceDbField}>='{SourceStartDateTime:d}' and {SourceDbField}<='{SourceEndDateTime:d}' order by {SourceDbField}";
                        break;
                    case DatabaseType.MYSQL:
                        throw new ArgumentOutOfRangeException();
                    /*strSQL = $"select {fieldNames} from {_tableName} {sqlWhere} limit 1";
                        break;*/


                    case DatabaseType.UNKNOWN:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var db = new DataBaseHelper(DbType, ConnectionString);
                var set = db.GetDataSet(CommandType.Text, strSql);

                return set.Tables[0];
            }
            catch (Exception e)
            {
                Log.Error("GroupBinding获取捆绑单位失败", e);
                return null;
            }
        }

        public override void SetValues(DataTable dataTable)
        {
            try
            {
                DeleteGroups();

                InsertGroups(dataTable);
            }
            catch (Exception e)
            {
                Log.Error("GroupBinding SetValues失败", e);
            }
        }

        private void InsertGroups(DataTable dataTable)
        {
            try
            {
                var fieldNames = $"{DestinationDbField},";

                var columnDictionary = new Dictionary<string, string>();
                // make "field1,field2,..."
                var i = 0;
                foreach (var item in DataItems)
                {
                    columnDictionary.Add(((DBFieldBindingItem) item).DBFieldName,
                        ((DBFieldBindingItem) item).DBFieldType.ToLower());

                    fieldNames += ((DBFieldBindingItem) item).DBFieldName;
                    if (i == DataItems.Count() - 1) continue;
                    fieldNames += ",";
                    i++;
                }


                var strSql = string.Empty;
                foreach (DataRow dataTableRow in dataTable.Rows)
                {
                    var fieldValues = DestinationBindingFieldType == "int"
                        ? $"{dataTableRow[DestinationDbField]},"
                        : $"'{dataTableRow[DestinationDbField]}',";
                    i = 0;
                    foreach (var columnInfo in columnDictionary)
                    {
                        if (columnInfo.Value == "string" || columnInfo.Value == "datetime")
                            fieldValues += $"'{dataTableRow[columnInfo.Key]}'";
                        else
                            fieldValues += $"{dataTableRow[columnInfo.Key]}";
                        if (i != DataItems.Count() - 1) fieldValues += ",";
                        i++;
                    }

                    strSql += $"insert into {TableName}({fieldNames}) values({fieldValues});\n";
                }

                var db = new DataBaseHelper(DbType, ConnectionString);
                db.ExecuteNonQuery(CommandType.Text, strSql);
            }
            catch (Exception ex)
            {
                Log.Error("GroupBindingDBFields插入记录失败", ex);
                throw new Exception("BindingDBFields写入失败");
            }
        }

        private void DeleteGroups()
        {
            var deleteStrSql = string.Empty;

            switch (DbType)
            {
                case DatabaseType.ORACLE:
                    /*                        strSQL = $"select * from (select {fieldNames} from {TableName} where rownum = 1";
                                                break;*/
                    throw new ArgumentOutOfRangeException();
                case DatabaseType.ACCESS:
                    deleteStrSql = DestinationBindingFieldType == "int"
                        ? $"delete from {TableName} where {DestinationDbField}>={DestinationStartNumber} and {DestinationDbField}<={DestinationEndNumber}"
                        : $"delete * from {TableName}";
                    break;
                case DatabaseType.SQLSERVER:
                    deleteStrSql = DestinationBindingFieldType == "int"
                        ? $"delete from {TableName} where {DestinationDbField}>={DestinationStartNumber} and {DestinationDbField}<={DestinationEndNumber}"
                        : $"delete {TableName}";
                    break;
                case DatabaseType.MYSQL:
                    throw new ArgumentOutOfRangeException();
                /*strSQL = $"select {fieldNames} from {_tableName} {sqlWhere} limit 1";
                    break;*/


                case DatabaseType.UNKNOWN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            new DataBaseHelper(DbType, ConnectionString).ExecuteNonQuery(CommandType.Text, deleteStrSql);
        }
    }












    /// <summary>
    /// sunjian added 2019-11-19
    /// </summary>
    public class GroupBindingJsonFields : GroupBindingDataSet
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupBindingJsonFields));

        public string ConnectionString { get; set; }

        public DatabaseType DbType { get; set; }

        public string TableName { get; set; }

        public string SourceDbField { get; set; }
        public string DestinationDbField { get; set; }

        public override List<BasicValue> GetValues()
        {
            throw new NotImplementedException();
        }

        public override void SetValues(List<BasicValue> values)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetDataTable()
        {
            try
            {
                var fieldNames = $"{SourceDbField},";
                // make "field1,field2,..."
                var i = 0;
                foreach (var item in DataItems)
                {
                    fieldNames += ((DBFieldBindingItem) item).DBFieldName;
                    if (i != DataItems.Count() - 1) fieldNames += ",";
                    i++;
                }

                var strSql = string.Empty;
                switch (DbType)
                {
                    case DatabaseType.ORACLE:
                        /*                        strSQL = $"select * from (select {fieldNames} from {TableName} where rownum = 1";
                                                break;*/
                        throw new ArgumentOutOfRangeException();
                    case DatabaseType.ACCESS:
                        strSql = SourceBindingFieldType == "int"
                            ? $"select {fieldNames} from {TableName} where {SourceDbField}>={SourceStartNumber} and {SourceDbField}<={SourceEndNumber} order by {SourceDbField}"
                            : $"select {fieldNames} from {TableName} where {SourceDbField}>=#{SourceStartDateTime:d}# and {SourceDbField}<=#{SourceEndDateTime:d}# order by {SourceDbField}";
                        break;
                    case DatabaseType.SQLSERVER:
                        strSql = SourceBindingFieldType == "int"
                            ? $"select {fieldNames} from {TableName} where {SourceDbField}>={SourceStartNumber} and {SourceDbField}<={SourceEndNumber} order by {SourceDbField}"
                            : $"select {fieldNames} from {TableName} where {SourceDbField}>='{SourceStartDateTime:d}' and {SourceDbField}<='{SourceEndDateTime:d}' order by {SourceDbField}";
                        break;
                    case DatabaseType.MYSQL:
                        throw new ArgumentOutOfRangeException();
                    /*strSQL = $"select {fieldNames} from {_tableName} {sqlWhere} limit 1";
                        break;*/


                    case DatabaseType.UNKNOWN:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var db = new DataBaseHelper(DbType, ConnectionString);
                var set = db.GetDataSet(CommandType.Text, strSql);

                return set.Tables[0];
            }
            catch (Exception e)
            {
                Log.Error("GroupBinding获取捆绑单位失败", e);
                return null;
            }
        }

        public override void SetValues(DataTable dataTable)
        {
            try
            {
                DeleteGroups();

                InsertGroups(dataTable);
            }
            catch (Exception e)
            {
                Log.Error("GroupBinding SetValues失败", e);
            }
        }

        private void InsertGroups(DataTable dataTable)
        {
            try
            {
                var fieldNames = $"{DestinationDbField},";

                var columnDictionary = new Dictionary<string, string>();
                // make "field1,field2,..."
                var i = 0;
                foreach (var item in DataItems)
                {
                    columnDictionary.Add(((DBFieldBindingItem) item).DBFieldName,
                        ((DBFieldBindingItem) item).DBFieldType.ToLower());

                    fieldNames += ((DBFieldBindingItem) item).DBFieldName;
                    if (i == DataItems.Count() - 1) continue;
                    fieldNames += ",";
                    i++;
                }


                var strSql = string.Empty;
                foreach (DataRow dataTableRow in dataTable.Rows)
                {
                    var fieldValues = DestinationBindingFieldType == "int"
                        ? $"{dataTableRow[DestinationDbField]},"
                        : $"'{dataTableRow[DestinationDbField]}',";
                    i = 0;
                    foreach (var columnInfo in columnDictionary)
                    {
                        if (columnInfo.Value == "string" || columnInfo.Value == "datetime")
                            fieldValues += $"'{dataTableRow[columnInfo.Key]}'";
                        else
                            fieldValues += $"{dataTableRow[columnInfo.Key]}";
                        if (i != DataItems.Count() - 1) fieldValues += ",";
                        i++;
                    }

                    strSql += $"insert into {TableName}({fieldNames}) values({fieldValues});\n";
                }

                var db = new DataBaseHelper(DbType, ConnectionString);
                db.ExecuteNonQuery(CommandType.Text, strSql);
            }
            catch (Exception ex)
            {
                Log.Error("GroupBindingDBFields插入记录失败", ex);
                throw new Exception("BindingDBFields写入失败");
            }
        }

        private void DeleteGroups()
        {
            var deleteStrSql = string.Empty;

            switch (DbType)
            {
                case DatabaseType.ORACLE:
                    /*                        strSQL = $"select * from (select {fieldNames} from {TableName} where rownum = 1";
                                                break;*/
                    throw new ArgumentOutOfRangeException();
                case DatabaseType.ACCESS:
                    deleteStrSql = DestinationBindingFieldType == "int"
                        ? $"delete from {TableName} where {DestinationDbField}>={DestinationStartNumber} and {DestinationDbField}<={DestinationEndNumber}"
                        : $"delete * from {TableName}";
                    break;
                case DatabaseType.SQLSERVER:
                    deleteStrSql = DestinationBindingFieldType == "int"
                        ? $"delete from {TableName} where {DestinationDbField}>={DestinationStartNumber} and {DestinationDbField}<={DestinationEndNumber}"
                        : $"delete {TableName}";
                    break;
                case DatabaseType.MYSQL:
                    throw new ArgumentOutOfRangeException();
                /*strSQL = $"select {fieldNames} from {_tableName} {sqlWhere} limit 1";
                    break;*/


                case DatabaseType.UNKNOWN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            new DataBaseHelper(DbType, ConnectionString).ExecuteNonQuery(CommandType.Text, deleteStrSql);
        }
    }
}