// ==================================================
// 文件名：DataBinding.cs
// 创建时间：2020/06/16 15:38
// ==================================================
// 最后修改于：2020/08/05 15:38
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.Attributes;
using ProcessControlService.ResourceFactory.DBUtil;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.DataBinding
{
    /// <summary>
    ///     Created by DongMin 20180129
    /// </summary>
    public class DataBinding : Resource, IActionContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DataBinding));
        private BindingDataSet _dest;

        private BindingType _linkType;

        //private string _dbConnection;
        //private string _dbTableName;

        // Fields in the table
        //private List<BindingItem> _tagBindings = new List<BindingItem>();

        //private List<string> _dbFieldNames = null;

        //private List<DBField> _fields = new List<DBField>();
        //private Dictionary<string, DBField> _fields = new Dictionary<string, DBField>();

        private BindingDataSet _source;

        public string DbBindingName;

        public DataBinding(string name) : base(name)
        {
            DbBindingName = name;

            // create actions
            var action1 = new BindingAction("BindingAction", this);
            _actions.AddAction(action1);
        }

        #region "Resource Service"

        [ResourceService]
        public ExecuteCounts GetExecuteCounts()
        {
            var countes =
                new ExecuteCounts
                {
                    GetValueSuccessCounts = _source.GetValueSuccessCounts,
                    GetValueFailedCounts = _source.GetValueFailedCounts,
                    SetValueSuccessCounts = _dest.SetValueFailedCounts,
                    SetValueFailedCounts = _source.SetValueFailedCounts,
                };
            return countes;
        }

        //public virtual string GetStatus()
        //{
        //    return null;
        //}

        #endregion

        public void ExecuteBinding()
        {
            var transfer = _source.GetValues();

            _dest.SetValues(transfer);
        }

        #region "Resource definition"

        //public string ClassName
        //{
        //    get { return "DBBinding"; }
        //}


        /// <summary>
        ///     Created by Dongmin at 20180129
        //     <Resource Type = "DataBinding" Name="DBLink1" LinkType="DBToTag" SourceTable="testtable1" Enable="false" >
        //	<BindingItems>
        //		<BindingItem Machine = "BTSTester" Tag="D1" DBField="column_1"/>
        //		<BindingItem Machine = "BTSTester" Tag="D2" DBField="column_2"/>
        //	</BindingItems>
        //</Resource>
        //<Resource Type = "DataBinding" Name="DBLink2" LinkType="TagToDB" DestTable="testtable1" Enable="false" >
        //	<BindingItems>
        //		<BindingItem Machine = "BTSTester" Tag="D1" DBField="column_1"/>
        //		<BindingItem Machine = "BTSTester" Tag="D2" DBField="column_2"/>
        //	</BindingItems>
        //</Resource>
        //<Resource Type = "DataBinding" Name="DBLink3" LinkType="DBToDB" SourceTable="testtable1" DestTable="testtable2" Enable="true" >
        //	<BindingItems>
        //		<BindingItem SourceDBField = "column_1" DestDBField="column_1"/>
        //		<BindingItem SourceDBField = "column_2" DestDBField="column_2"/>
        //	</BindingItems>
        //</Resource>
        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level0Item = (XmlElement) node;

                var strType = level0Item.GetAttribute("LinkType");

                //string strDestTable = string.Empty;
                //string strSourceTable = string.Empty;

                //if (level0_item.HasAttribute("DestTable"))
                //{
                //    strDestTable = level0_item.GetAttribute("DestTable");
                //}
                //if (level0_item.HasAttribute("SourceTable"))
                //{
                //    strSourceTable = level0_item.GetAttribute("SourceTable");
                //}

                // create data set
                switch (strType.ToLower())
                {
                    case "tagtodb":
                        _source = new BindingTags();
                        _dest = new BindingDbFields();
                        _linkType = BindingType.TagToDB;
                        //((BindingDBFields)_dest).TableName = strDestTable;
                        break;

                    case "dbtotag":
                        _source = new BindingDbFields();
                        _dest = new BindingTags();
                        _linkType = BindingType.DBToTag;
                        //((BindingDBFields)_source).TableName = strSourceTable;
                        break;

                    case "dbtodb":
                        _source = new BindingDbFields();
                        _dest = new BindingDbFields();
                        _linkType = BindingType.DBToDB;
                        //((BindingDBFields)_source).TableName = strSourceTable;
                        //((BindingDBFields)_dest).TableName = strDestTable;
                        break;

                    case "tagtotag":
                        _source = new BindingTags();
                        _dest = new BindingTags();
                        _linkType = BindingType.TagToTag;
                        break;

                    case "jsontexttodb":
                        _source = new BindingJsonFields();
                        _dest = new BindingDbFields();
                        _linkType = BindingType.JsonTextToDb;
                        break;
                }

                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "Fields", "Actions"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    if (level1Item.Name.ToLower() == "dbsourceselect")
                    {
                        //< DBSourceSelect DBField = "Compare" ConditionType = "Min" />

                        // DB connection load -- David 20191112
                        var dbconnNode =
                            level1Node.SelectSingleNode("DBConnection"); /*sunjian 2019-11-16 多写了两个\\，搜索结果是父对象下的node*/
                        if (dbconnNode != null)
                        {
                            var dbconnItem = (XmlElement) dbconnNode;
                            ((BindingDbFields) _source).DbType =
                                DataBaseHelper.ConvertDatabaseType(dbconnItem.GetAttribute("Type"));
                            ((BindingDbFields) _source).ConnectionString = dbconnItem.GetAttribute("ConnectionString");
                        }

                        // Data table load
                        if (level1Item.HasAttribute("DBTable"))
                            ((BindingDbFields) _source).TableName = level1Item.GetAttribute("DBTable");

                        var strDbField = level1Item.GetAttribute("DBField");
                        var strConditionType = level1Item.GetAttribute("ConditionType");

                        var type = SourceConditionType.Max;
                        switch (strConditionType.ToLower())
                        {
                            case "equal":
                                type = SourceConditionType.Equal;
                                break;
                            case "max":
                                type = SourceConditionType.Max;
                                break;
                            case "min":
                                type = SourceConditionType.Min;
                                break;
                        }

                        var strConditionValue = string.Empty;
                        var strConditionValueType = string.Empty;
                        if (level1Item.HasAttribute("ConditionValue"))
                        {
                            strConditionValue = level1Item.GetAttribute("ConditionValue");
                            strConditionValueType = level1Item.GetAttribute("ConditionValueType");
                        }

                        if (type == SourceConditionType.Max || type == SourceConditionType.Min)
                        {
                            if (!level1Item.HasAttribute("ConditionValueType"))
                                throw new Exception(
                                    $"配置文件中数据库数据绑定{DbBindingName}限定条件为Max或Min，必须给定字段类型SourceDbFieldType和DestDbFieldType");
                            if (level1Item.GetAttribute("ConditionValueType") == "string")
                                throw new Exception("元数据DBField是字符串类型，执行Max操作将导致排序不准确，请选择数字/日期/bool等类型字段进行Max操作。");
                            /*执行最大最小操作时需要保证字段为数字格式，否则字符串格式的数字排序最大值一定是9开头的数字。 jiansun 2019-11-18*/
                        }

                        switch (_linkType)
                        {
                            case BindingType.DBToTag:
                            case BindingType.DBToDB:
                            {
                                ((BindingDbFields) _source).SourceConditionDbField = strDbField;
                                ((BindingDbFields) _source).SourceConditionType = type;
                                if (type == SourceConditionType.Equal)
                                    ((BindingDbFields) _source).SourceConditionValue =
                                        strConditionValueType.ToLower() == "string"
                                            ? $"'{Parameter.CreateValueFromString(strConditionValueType, strConditionValue)}'"
                                            : Parameter.CreateValueFromString(strConditionValueType, strConditionValue);

                                break;
                            }
                        }
                    }

                    else if (level1Item.Name.ToLower() == "jsonsourceselect")
                    {
                        //< DBSourceSelect DBField = "Compare" ConditionType = "Min" />

                        // DB connection load -- David 20191112
                        var jsonFilePathNode = level1Node.SelectSingleNode("JsonFilePath");
                        if (jsonFilePathNode != null)
                        {
                            var filePathNode = (XmlElement) jsonFilePathNode;
                            ((BindingJsonFields) _source).FilePath = filePathNode.GetAttribute("FilePath");
                        }

                        // Data table load
                        if (level1Item.HasAttribute("ArrayName"))
                            ((BindingJsonFields) _source).ArrayName = level1Item.GetAttribute("ArrayName");

                        var strDBField = level1Item.GetAttribute("JsonField");
                        var strConditionType = level1Item.GetAttribute("ConditionType");

                        var _type = SourceConditionType.Max;
                        switch (strConditionType.ToLower())
                        {
                            case "equal":
                                _type = SourceConditionType.Equal;
                                break;
                            case "max":
                                _type = SourceConditionType.Max;
                                break;
                            case "min":
                                _type = SourceConditionType.Min;
                                break;
                        }

                        var strConditionValue = string.Empty;
                        if (level1Item.HasAttribute("ConditionValue"))
                            strConditionValue = level1Item.GetAttribute("ConditionValue");

                        if (_linkType != BindingType.JsonTextToDb) continue;
                        ((BindingJsonFields) _source).SourceConditionJsonField = strDBField;
                        ((BindingJsonFields) _source).SourceConditionType = _type;
                        if (_type == SourceConditionType.Equal)
                            ((BindingJsonFields) _source).SourceConditionValue = strConditionValue;
                    }

                    else if (level1Item.Name.ToLower() == "dbdestselect")
                    {
                        //< DBDestSelect DBField = "Compare2" ConditionValueType = "int16" ConditionValue = "65" />

                        // DB connection load -- David 20191112
                        var dbconnNode =
                            level1Node.SelectSingleNode("DBConnection"); /*sunjian 2019-11-16 多写了两个\\，搜索结果是父对象下的node*/
                        if (dbconnNode != null)
                        {
                            var dbconnItem = (XmlElement) dbconnNode;
                            ((BindingDbFields) _dest).DbType =
                                DataBaseHelper.ConvertDatabaseType(dbconnItem.GetAttribute("Type"));
                            ((BindingDbFields) _dest).ConnectionString = dbconnItem.GetAttribute("ConnectionString");
                        }

                        // Data table load
                        if (level1Item.HasAttribute("DBTable"))
                            ((BindingDbFields) _dest).TableName = level1Item.GetAttribute("DBTable");


                        ((BindingDbFields) _dest).DestConditionDBField = level1Item.GetAttribute("DBField");
                        ((BindingDbFields) _dest).DestConditionValueType =
                            level1Item.GetAttribute("ConditionValueType");
                        ((BindingDbFields) _dest).DestConditionValue = level1Item.GetAttribute("ConditionValue");

                        var strOperateType = level1Item.GetAttribute("OperateType");
                        switch (strOperateType.ToLower())
                        {
                            case "update":
                                ((BindingDbFields) _dest).DestOperateType = DestOperateType.Update;
                                break;
                            case "insert":
                                ((BindingDbFields) _dest).DestOperateType = DestOperateType.Insert;
                                break;
                        }
                    }


                    else if (level1Item.Name.ToLower() == "bindingitems")
                    {
                        foreach (XmlNode level2Node in level1Node)
                        {
                            // datasource
                            if (level2Node.NodeType == XmlNodeType.Comment)
                                continue;

                            var level2Item = (XmlElement) level2Node;
                            if (level2Item.Name.ToLower() == "bindingitem")
                            {
                                string strMachine;
                                string strTag;
                                string strDBField;
                                string strDBField2;
                                string strDBFieldType;
                                string strJsonField;

                                Machine machine = null;

                                switch (_linkType)
                                {
                                    case BindingType.DBToDB:

                                        strDBField = level2Item.GetAttribute("SourceDBField");
                                        strDBField2 = level2Item.GetAttribute("DestDBField");
                                        strDBFieldType = level2Item.GetAttribute("DBFieldType");

                                        _source.AddItem(new DbFieldBindingItem(strDBField, strDBFieldType));
                                        _dest.AddItem(new DbFieldBindingItem(strDBField2, strDBFieldType));

                                        break;
                                    case BindingType.DBToTag:
                                        strMachine = level2Item.GetAttribute("Machine");
                                        strTag = level2Item.GetAttribute("Tag");
                                        strDBField = level2Item.GetAttribute("DBField");
                                        strDBFieldType = level2Item.GetAttribute("DBFieldType");

                                        _source.AddItem(new DbFieldBindingItem(strDBField, strDBFieldType));
                                        _dest.AddItem(new MachineBindingItem(strMachine, strTag));

                                        break;
                                    case BindingType.TagToDB:
                                        strMachine = level2Item.GetAttribute("Machine");
                                        strTag = level2Item.GetAttribute("Tag");
                                        strDBField = level2Item.GetAttribute("DBField");
                                        strDBFieldType = level2Item.GetAttribute("DBFieldType");

                                        _source.AddItem(new MachineBindingItem(strMachine, strTag));
                                        _dest.AddItem(new DbFieldBindingItem(strDBField, strDBFieldType));

                                        break;

                                    case BindingType.JsonTextToDb:
                                        strJsonField = level2Item.GetAttribute("SourceJsonField");
                                        strDBField2 = level2Item.GetAttribute("DestDBField");
                                        strDBFieldType = level2Item.GetAttribute("DBFieldType");

                                        _source.AddItem(new JsonBindingItem(strJsonField));
                                        _dest.AddItem(new DbFieldBindingItem(strDBField2, strDBFieldType));
                                        break;

                                    case BindingType.TagToTag:
                                        var strSourceMachine = level2Item.GetAttribute("SourceMachine");
                                        var strDestMachine = level2Item.GetAttribute("DestMachine");
                                        var strSourceTag = level2Item.GetAttribute("SourceTag");
                                        var strDestTag = level2Item.GetAttribute("DestTag");

                                        _source.AddItem(new MachineBindingItem(strSourceMachine, strSourceTag));
                                        _dest.AddItem(new MachineBindingItem(strDestMachine, strDestTag));

                                        break;
                                }
                            }
                        }
                    }
                }

                Log.Info("装载DBBindding配置成功");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("装载DBBindding配置出错：" + ex);
                return false;
            }
        }

        public override void FreeResource()
        {
            // nothing to do now
        }

        #endregion

        // 动作

        #region "Actions definitions"

        private readonly ActionCollection _actions = new ActionCollection();

        public void AddAction(BaseAction action)
        {
            _actions.AddAction(action);
        }

        public BaseAction GetAction(string name)
        {
            return _actions.GetAction(name);
        }


        public virtual void ExecuteAction(string name)
        {
            var _action = GetAction(name);
            _action.Execute();
        }

        public string[] ListActionNames()
        {
            return _actions.ListActionNames();
        }

        #endregion
    }

    public enum BindingType
    {
        TagToDB = 0,
        DBToTag = 1,
        DBToDB = 2,
        JsonTextToDb = 3,
        TagToTag = 4
    }

    public class ExecuteCounts
    {
        public int GetValueFailedCounts;
        public int GetValueSuccessCounts;
        public int SetValueFailedCounts;
        public int SetValueSuccessCounts;
    }
}