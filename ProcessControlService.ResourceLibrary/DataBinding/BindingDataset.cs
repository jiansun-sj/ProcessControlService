using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;
using System.IO;
using ProcessControlService.ResourceLibrary.Machines;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.DBUtil;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.DataBinding
{

    public abstract class BindingDataSet
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(BindingDataSet));

        //protected ParameterManager _dataset;
        protected List<BindingDataItem> DataItems = new List<BindingDataItem>();

        public void AddItem(BindingDataItem item)
        {
            DataItems.Add(item);
        }

        public abstract List<IBasicValue> GetValues();

        public abstract void SetValues(List<IBasicValue> values);

        public int GetValueSuccessCounts;
        public int GetValueFailedCounts;
        public int SetValueSuccessCounts;
        public int SetValueFailedCounts;

        //public abstract void SetValue();
    }


    /// <summary>
    /// sunjian added 2019-11-16
    /// </summary>
    public class BindingJsonFields : BindingDataSet
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(BindingJsonFields));

        public string ArrayName { get; set; }


        #region source conditions

        public string SourceConditionJsonField { get; set; }

        public SourceConditionType SourceConditionType { get; set; }

        public string SourceConditionValue { get; set; }

        #endregion

        #region dest conditions

        public DestOperateType DestOperateType { get; set; } = DestOperateType.Insert;

        public string DestConditionJsonField { get; set; }

        public string DestConditionValue { get; set; }

        public string FilePath { get; set; }

        #endregion

        public override List<IBasicValue> GetValues()
        {
            try
            {
                var fieldNames = (from JsonBindingItem item in DataItems select item.JsonFieldName).ToList();

                var streamReader = new StreamReader(FilePath, Encoding.Default);
                string jsonContents = null;
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    jsonContents += line;
                }


                var jsonDataSet = JsonUtil.JsonToDataTable(jsonContents, ArrayName).AsEnumerable();

                /*foreach (var jsDataRow in jsonDataSet.AsEnumerable())
                {
                    var a = jsDataRow[SourceConditionJsonField] /*== SourceConditionValue#1#;
                }*/

                DataRow dataRow;

                EnumerableRowCollection<double> doubleColumn;
                switch (SourceConditionType)
                {
                    case SourceConditionType.Equal:
                       dataRow =
                            jsonDataSet.FirstOrDefault(a => a[SourceConditionJsonField].ToString() == SourceConditionValue);
                        break;
                    case SourceConditionType.Max:
                        doubleColumn = from row in jsonDataSet select Convert.ToDouble(row[SourceConditionJsonField]);
                        dataRow = jsonDataSet.FirstOrDefault(a =>
                            a[SourceConditionJsonField].ToString() ==
                            doubleColumn.Max().ToString(CultureInfo.InvariantCulture));

                        break;
                    case SourceConditionType.Min:
                        doubleColumn = from row in jsonDataSet select Convert.ToDouble(row[SourceConditionJsonField]);
                        dataRow = jsonDataSet.FirstOrDefault(a =>
                            a[SourceConditionJsonField].ToString() ==
                            doubleColumn.Min().ToString(CultureInfo.InvariantCulture));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(dataRow));
                }

              

                var values = (from fieldName in fieldNames
                    select dataRow?[$"{fieldName}"].ToString()
                    into value
                    where value != null
                    select Parameter.CreateBasicValue(value.GetType().ToString(), value)).ToList();


                GetValueSuccessCounts++;
                return values;
            }
            catch (Exception e)
            {
                Log.Error($"获得Json源数据出错",e);
                return new List<IBasicValue>();
            }
           
        }

        public override void SetValues(List<IBasicValue> values)
        {
            throw new NotImplementedException();
        }
    }


    public class BindingDbFields : BindingDataSet
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(BindingDbFields));

        //private string _connetionString;
        public string ConnectionString
        {
            get;
            set;
        }

        public DatabaseType DbType
        {
            get;set;
        }

        

        //private List<string> _fieldNameList = new List<string>();

        public string TableName { get; set; }


        #region source conditions

        public string SourceConditionDbField { get; set; }

        public SourceConditionType SourceConditionType { get; set; }

        public object SourceConditionValue { get; set; }

        public string SourceDbFieldType;
        #endregion

        #region dest conditions
        DestOperateType _destOperateType= DestOperateType.Insert;
        public DestOperateType DestOperateType
        {
            get { return _destOperateType; }
            set { _destOperateType = value; }
        }

        private string _destConditionDBField;
        public string DestConditionDBField
        {
            get { return _destConditionDBField; }
            set { _destConditionDBField = value; }
        }

        private object _destConditionValue;
        public object DestConditionValue
        {
            get { return _destConditionValue; }
            set { _destConditionValue = value; }
        }

        private string _destConditionValueType;
        public string DestConditionValueType
        {
            get { return _destConditionValueType; }
            set { _destConditionValueType = value; }
        }

        public string DbFieldType;

        #endregion



        //public override void AddItem(BindingDataItem Item)
        //{
        //    //_fieldNameList.Add(Item.Name);

        //    _dataItems.Add(Item);
        //}

        public override List<IBasicValue> GetValues()
        {
            try
            {
                var fieldNames = string.Empty;
                // make "field1,field2,..."
                var i = 0;
                foreach (var item in DataItems)
                {
                    fieldNames += ((DbFieldBindingItem)item).DbFieldName;
                    if (i != (DataItems.Count() - 1))
                    {
                        fieldNames += ",";
                    }

                    i++;
                }

                // condition SQL
                string sqlWhere;
                switch (SourceConditionType)
                {
                    case SourceConditionType.Equal:
                        sqlWhere = $"where {SourceConditionDbField} = {SourceConditionValue}";
                        break;

                    case SourceConditionType.Max:
                        if ((SourceConditionValue as IBasicValue)?.SimpleType=="string")
                            throw new Exception("元数据DBField是字符串类型，执行Max操作将导致排序不准确，请选择数字/日期/bool等类型字段进行Max操作。");
                        /*执行最大最小操作时需要保证字段为数字格式，否则字符串格式的数字排序最大值一定是9开头的数字。 jiansun 2019-11-18*/
                        sqlWhere = $"order by {SourceConditionDbField} desc"/*sunjian 2019-11-16 ，最大最小条件写反了*/;
                        break;

                    case SourceConditionType.Min:
                        if ((SourceConditionValue as IBasicValue)?.SimpleType == "string")
                            throw new Exception("元数据DBField是字符串类型，执行Max操作将导致排序不正确，请选择数字/日期/bool类型字段进行Max操作。");
                        /*执行最大最小操作时需要保证字段为数字格式，否则字符串格式的数字排序最大值一定是9开头的数字。 jiansun 2019-11-18*/
                        sqlWhere = $"order by {SourceConditionDbField} asc"/*sunjian 2019-11-16, 最大最小条件写反了*/;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var values = new List<IBasicValue>();

                string strSql=null;
                switch (DbType)
                {
                    case DatabaseType.Oracle:
                        strSql = $"select * from (select {fieldNames} from {TableName} {sqlWhere}) where rownum = 1";
                        break;
                    case DatabaseType.Sqlserver:
                        strSql = $"select top 1 {fieldNames} from {TableName} {sqlWhere}"; //sunjian 2019-12-15 修复sql语句bug
                        break;
                    case DatabaseType.Mysql:
                        strSql = $"select {fieldNames} from {TableName} {sqlWhere} limit 1";
                        break;
                    case DatabaseType.Access:
                        strSql= $"select top 1 {fieldNames} from {TableName} {sqlWhere}";
                        break;
                    case DatabaseType.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                DataBaseHelper db = new DataBaseHelper(DbType, ConnectionString);
                DataSet set = db.GetDataSet(CommandType.Text, strSql);
                if (set.Tables[0].Rows.Count != 0)
                {
                    foreach (object data in set.Tables[0].Rows[0].ItemArray)
                    {
                        var theValue = Parameter.CreateBasicValue(data.GetType().ToString(), data.ToString());
                        values.Add(theValue);
                    }
                    
                }

                GetValueSuccessCounts++;
                return values;
            }
            catch (Exception ex)
            {
                Log.Error("BindingDBFields获取数值失败:{0}", ex);
                GetValueFailedCounts++;
                return null;
            }

        }

        public override void SetValues(List<IBasicValue> values)
        {
            try
            {
                switch (_destOperateType)
                {
                    case DestOperateType.Insert:
                        InsertToDb(values);
                        break;
                    case DestOperateType.Update:
                        DeleteFromDb();
                        InsertToDb(values);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SetValueSuccessCounts++;
            }
            catch (Exception ex)
            {
                Log.Error("BindingDBFields写入失败:{0}", ex);
                SetValueFailedCounts++;
            }
        }

        private void InsertToDb(IReadOnlyList<IBasicValue> values)
        {
            try
            {
                var fieldNames = string.Empty;
                var fieldValues = string.Empty;

                // make "field1,field2,..."
                var i = 0;
                foreach (BindingDataItem item in DataItems)
                {
                    fieldNames += ((DbFieldBindingItem)item).DbFieldName;
                    if (i != (DataItems.Count() - 1))
                    {
                        fieldNames += ",";
                    }

                    var strTagValue = values[i].ToString();

                    switch (((DbFieldBindingItem)item).DbFieldType.ToLower())
                    {
                        
                        case "string":
                            fieldValues += $"'{strTagValue}'";
                            break;
                        case "bool":
                            //sunjian 2019-12-20 bool值在数据库中应为0,1, PLC读取出来的true，false需要转换为0,1再写入数据库。
                            if (int.TryParse(strTagValue, out _))
                                fieldValues += $"{strTagValue}";
                            else
                                fieldValues += $"{(strTagValue.ToLower() == "true" ? 1 : 0)}";

                            break;
                        default:
                            fieldValues += strTagValue;
                            break;
                    }

                    if (i != (DataItems.Count() - 1))
                    {
                        fieldValues += ",";
                    }

                    i++;
                }

                // if update, need add compare field
                if(_destOperateType== DestOperateType.Update)
                {
                    fieldNames += ($",{_destConditionDBField}");
   
                    fieldValues += ($",{_destConditionValue}");
                }
               

                var strSql = $"insert into {TableName}({fieldNames}) values({fieldValues})";

                var db = new DataBaseHelper(DbType, ConnectionString);

                db.ExecuteNonQuery(CommandType.Text, strSql);
            }
            catch (Exception ex)
            {
                Log.Error("BindingDBFields插入记录失败:{0}", ex);
                throw new Exception("BindingDBFields写入失败");
            }
        }

        private void DeleteFromDb()
        {
            try
            {
                // condition SQL
                var sqlWhere = _destConditionValueType.ToLower() == "string"
                    ? $"where {_destConditionDBField} = '{_destConditionValue}'"
                    : $"where {_destConditionDBField} = {_destConditionValue}";

                var strSql = $"delete from {TableName} {sqlWhere}";

                var mysql = new DataBaseHelper(DbType, ConnectionString);

                mysql.ExecuteNonQuery(CommandType.Text, strSql);
            }
            catch (Exception ex)
            {
                Log.Error("BindingDBFields删除数值失败:{0}", ex);
                throw new Exception("BindingDBFields写入失败");
            }
        }

   
    }

    public class BindingTags : BindingDataSet
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(BindingTags));

        //private List<Tag> _tagList = new List<Tag>();

        
        //public override void AddItem(BindingDataItem Item)
        //{
        //    //_tagList.Add((Tag)Item);
        //}

        public override List<IBasicValue> GetValues()
        {
            try
            {
                var result = new List<IBasicValue>();

                foreach (var item in DataItems)
                {

                    result.Add(((MachineBindingItem)item).Value);
                }
                GetValueSuccessCounts++;
                return result;
            }
            catch (Exception ex)
            {
                Log.Error("BindingTags 获取值出错:{0}",ex);
                GetValueFailedCounts++;
                return null;
            }
        }

        public override void SetValues(List<IBasicValue> values)
        {
            try
            {
                var i = 0;
                foreach (var item in DataItems)
                {
                    ((MachineBindingItem)item).WriteTag(values[i++]);
                    //_tagList[i].TagValue = value;
                }
                SetValueSuccessCounts++;
            }
            catch (Exception ex)
            {
                SetValueFailedCounts++;
                Log.Error("BindingDataset写值出错:{0}",ex);
            }
           
        }
    }

    public abstract class BindingDataItem
    {
        public string Name { get; set; }
        public abstract IBasicValue Value { get;}

        //public enum SourceType
        //{
        //    FromMachine,
        //    FromDatabase
        //}
        //public SourceType Source;
    }

    public class DbFieldBindingItem : BindingDataItem
    {
        public string DbFieldName { get;  }
        public string DbFieldType { get;  }

        public DbFieldBindingItem(string fieldName,string fieldType)
        {
            Name = fieldName;
            DbFieldName = fieldName;
            DbFieldType = fieldType;
        }

        private IBasicValue _value;
        public override IBasicValue Value => _value;
    }


    public class MachineBindingItem : BindingDataItem
    {
        public string MachineName { get; set; }
        public string TagName { get; set; }
        private readonly Tag _tag;

        public MachineBindingItem(string machineName, string tagName)
        {
            Name = tagName;
            MachineName = machineName;
            TagName = tagName;

            var machine = (Machine)ResourceManager.GetResource(machineName);
            _tag = machine.GetTag(tagName);
        }

        public override IBasicValue Value => Parameter.CreateBasicValue(_tag.TagType,_tag.TagValue.ToString());

        public void WriteTag(IBasicValue tagValue)
        {
            _tag.Write(tagValue.GetValue());
        }
    }

    /// <summary>
    /// sunjian added 2019-11-16
    /// </summary>
    public class JsonBindingItem : BindingDataItem
    {
        public string JsonFieldName { get; set; }
       

        public JsonBindingItem(string fieldName)
        {
            Name = fieldName;
            JsonFieldName = fieldName;
        }

        private IBasicValue _value;
        public override IBasicValue Value => _value;
    }

    public enum SourceConditionType
    {
        //Nothing = 0,
        Equal = 0,
        Max = 1,
        Min = 2
    }

    public enum DestOperateType
    {
        Insert = 0,
        Update = 1
    }
   

}
