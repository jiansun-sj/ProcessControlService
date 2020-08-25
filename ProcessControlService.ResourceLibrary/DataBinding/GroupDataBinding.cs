// ==================================================
// 文件名：GroupDataBinding.cs
// 创建时间：2019/11/18 20:07
// ==================================================
// 最后修改于：2019/11/18 20:07
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Xml;
using log4net;
using ProcessControlService.Contracts;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.DataBinding;

namespace ProcessControlService.CustomActions1.DataBinding
{
    /// <summary>
    /// sunjian 2019-11-19
    /// 批量数据捆绑同步。
    /// </summary>
    public class GroupDataBinding : IResource, IActionContainer, IResourceExportService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupDataBinding));

        private readonly ActionCollection _actions = new ActionCollection();
        private GroupBindingDataSet _dest;

        private BindingType _linkType;
        private GroupBindingDataSet _source;

        public GroupDataBinding(string groupDataBindingName)
        {
            ResourceName = groupDataBindingName;
            var groupDataBindingAction = new GroupBindingAction("GroupBindingAction", this);
            _actions.AddAction(groupDataBindingAction);
        }


        public void AddAction(BaseAction action)
        {
            _actions.AddAction(action);
        }

        public BaseAction GetAction(string actionName)
        {
            return _actions.GetAction(actionName);
        }

        public void ExecuteAction(string name)
        {
            var action = GetAction(name);
            action.Execute();
        }

        public string[] ListActionNames()
        {
            return _actions.ListActionNames();
        }


        public string ResourceName { get; }
        public string ResourceType => "GroupDataBinding";

        private string _bindingFieldType="";

        public IResource GetResourceObject()
        {
            return this;
        }

        public IResourceExportService GetExportService()
        {
            throw new NotImplementedException();
        }

        public bool LoadFromConfig(XmlNode node)
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
                        /*_source = new BindingTags();
                        _dest = new BindingDBFields();*/ //todo:group tag
                        _linkType = BindingType.TagToDB;
                        //((BindingDBFields)_dest).TableName = strDestTable;
                        break;

                    case "dbtotag":
                        /*_source = new BindingDBFields();
                        _dest = new BindingTags();*/
                        _linkType = BindingType.DBToTag;
                        //((BindingDBFields)_source).TableName = strSourceTable;
                        break;

                    case "dbtodb":
                        _source = new GroupBindingDbFields();
                        _dest = new GroupBindingDbFields();
                        _linkType = BindingType.DBToDB;
                        //((BindingDBFields)_source).TableName = strSourceTable;
                        //((BindingDBFields)_dest).TableName = strDestTable;
                        break;

                    case "tagtotag":
                        /*_source = new BindingTags();
                        _dest = new BindingTags();*/
                        _linkType = BindingType.TagToTag;
                        break;

                    case "jsontexttodb":
                        /*_source = new BindingJsonFields();
                        _dest = new BindingDBFields();*/ //todo: group json text
                        _linkType = BindingType.JsonTextToDb;
                        break;
                }

                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "Fields", "Actions"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    switch (level1Item.Name.ToLower())
                    {
                        case "dbsourceselect":
                        {
                            var dbConnNode = level1Node.SelectSingleNode("DBConnection");
                            if (dbConnNode != null)
                            {
                                var dbConnItem = (XmlElement) dbConnNode;
                                ((GroupBindingDbFields) _source).DbType =
                                    DataBaseHelper.ConvertDatabaseType(dbConnItem.GetAttribute("Type"));
                                ((GroupBindingDbFields) _source).ConnectionString =
                                    dbConnItem.GetAttribute("ConnectionString");
                            }

                            // Data table load
                            if (level1Item.HasAttribute("DBTable"))
                                ((GroupBindingDbFields) _source).TableName = level1Item.GetAttribute("DBTable");

                            var strDbField = level1Item.GetAttribute("DBField");
                            ((GroupBindingDbFields) _source).SourceDbField = strDbField;


                            if (level1Item.HasAttribute("FieldType"))
                            {
                                ((GroupBindingDbFields) _source).SourceBindingFieldType =level1Item.GetAttribute("FieldType").ToLower();
                                if (level1Item.GetAttribute("FieldType").ToLower() == "int")
                                {
                                    int.TryParse(level1Item.GetAttribute("FromNumber"), out var fromNumber);
                                    int.TryParse(level1Item.GetAttribute("ToNumber"), out var toNumber);
                                    ((GroupBindingDbFields) _source).SourceStartNumber = fromNumber;
                                    ((GroupBindingDbFields) _source).SourceEndNumber = toNumber;
                                }
                                else if (level1Item.GetAttribute("FieldType").ToLower() == "datetime")
                                {
                                    DateTime.TryParse(level1Item.GetAttribute("FromDate"), out var fromDateTime);
                                    DateTime.TryParse(level1Item.GetAttribute("ToDate"), out var toDateTime);
                                    ((GroupBindingDbFields) _source).SourceStartDateTime = fromDateTime;
                                    ((GroupBindingDbFields) _source).SourceEndDateTime = toDateTime;
                                }
                            }
                           

                           

                            break;
                        }


/*
                        case "jsonsourceselect":
                        {
                            //< DBSourceSelect DBField = "Compare" ConditionType = "Min" />

                            // DB connection load -- David 20191112
                            XmlNode jsonFilePathNode = level1Node.SelectSingleNode("JsonFilePath");
                            if (jsonFilePathNode != null)
                            {
                                XmlElement filePathNode = (XmlElement)jsonFilePathNode;
                                ((BindingJsonFields)_source).FilePath = filePathNode.GetAttribute("FilePath");
                            }

                            // Data table load
                            if (level1Item.HasAttribute("ArrayName"))
                            {
                                ((BindingJsonFields)_source).ArrayName = level1Item.GetAttribute("ArrayName");
                            }

                            string strDBField = level1Item.GetAttribute("JsonField");
                            string strConditionType = level1Item.GetAttribute("ConditionType");

                            SourceConditionType _type = SourceConditionType.Max;
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
                            {
                                strConditionValue = level1Item.GetAttribute("ConditionValue");
                            }

                            if (_linkType != BindingType.JsonTextToDb) continue;
                            ((BindingJsonFields)_source).SourceConditionJsonField = strDBField;
                            ((BindingJsonFields)_source).SourceConditionType = _type;
                            if (_type == SourceConditionType.Equal)
                                ((BindingJsonFields)_source).SourceConditionValue = strConditionValue;
                            break;
                        }
*/


                        case "dbdestselect":
                        {

                            //todo: 还有一个OperateType属性，用于后期拓展其他功能。
                            //< DBDestSelect DBField = "Compare2" ConditionValueType = "int16" ConditionValue = "65" />

                            // DB connection load -- David 20191112
                            var dbConnNode = level1Node.SelectSingleNode("DBConnection");
                            if (dbConnNode != null)
                            {
                                var dbConnItem = (XmlElement) dbConnNode;
                                ((GroupBindingDbFields) _dest).DbType =
                                    DataBaseHelper.ConvertDatabaseType(dbConnItem.GetAttribute("Type"));
                                ((GroupBindingDbFields) _dest).ConnectionString =
                                    dbConnItem.GetAttribute("ConnectionString");
                            }

                            // Data table load
                            if (level1Item.HasAttribute("DBTable"))
                                ((GroupBindingDbFields) _dest).TableName = level1Item.GetAttribute("DBTable");

                            ((GroupBindingDbFields)_dest).DestinationDbField = level1Item.GetAttribute("DBField");
                            if (level1Item.HasAttribute("FieldType"))
                            {
                                ((GroupBindingDbFields)_dest).DestinationBindingFieldType = level1Item.GetAttribute("FieldType").ToLower();
                                if (level1Item.GetAttribute("FieldType").ToLower() == "int")
                                {
                                    int.TryParse(level1Item.GetAttribute("FromNumber"), out var fromNumber);
                                    int.TryParse(level1Item.GetAttribute("ToNumber"), out var toNumber);

                                    ((GroupBindingDbFields)_dest).DestinationStartNumber = fromNumber;
                                    ((GroupBindingDbFields)_dest).DestinationEndNumber = toNumber;
                                }
                                else if (level1Item.GetAttribute("FieldType").ToLower() == "datetime")
                                {
                                    
                                }
                            }

                            break;
                        }
                        case "bindingitems":
                        {
                            foreach (XmlNode level2Node in level1Node)
                            {
                                // datasource
                                if (level2Node.NodeType == XmlNodeType.Comment)
                                    continue;

                                var level2Item = (XmlElement) level2Node;
                                if (level2Item.Name.ToLower() != "bindingitem") continue;
                                string strMachine;
                                string strTag;
                                string strDBField;
                                string strDBField2;
                                string strDBFieldType;
                                string strJsonField;

                                switch (_linkType)
                                {
                                    case BindingType.DBToDB:
                                        //strMachine = level2_item.GetAttribute("Machine");
                                        //strTag = level2_item.GetAttribute("Tag");
                                        strDBField = level2Item.GetAttribute("SourceDBField");
                                        strDBField2 = level2Item.GetAttribute("DestDBField");
                                        strDBFieldType = level2Item.GetAttribute("DBFieldType");

                                        _source.AddItem(new DBFieldBindingItem(strDBField, strDBFieldType));
                                        _dest.AddItem(new DBFieldBindingItem(strDBField2, strDBFieldType));

                                        break;
                                    case BindingType.DBToTag:
                                        /*
                                            strMachine = level2_item.GetAttribute("Machine");
                                            strTag = level2_item.GetAttribute("Tag");
                                            strDBField = level2_item.GetAttribute("DBField"); ;
                                            strDBFieldType = level2_item.GetAttribute("DBFieldType");

                                            //machine = (Machine)ResourceManager.GetResource(strMachine);
                                            //tag = machine.GetTag(strTag);

                                            _source.AddItem(new DBFieldBindingItem(strDBField, strDBFieldType));
                                            _dest.AddItem(new MachineBindingItem(strMachine, strTag));
*/

                                        break;
                                    case BindingType.TagToDB:
                                        /*strMachine = level2_item.GetAttribute("Machine");
                                            strTag = level2_item.GetAttribute("Tag");
                                            strDBField = level2_item.GetAttribute("DBField");
                                            strDBFieldType = level2_item.GetAttribute("DBFieldType");

                                            //machine = (Machine)ResourceManager.GetResource(strMachine);
                                            //tag = machine.GetTag(strTag);

                                            _source.AddItem(new MachineBindingItem(strMachine, strTag));
                                            _dest.AddItem(new DBFieldBindingItem(strDBField, strDBFieldType));*/

                                        break;

                                    case BindingType.JsonTextToDb:
                                        /*strJsonField = level2_item.GetAttribute("SourceJsonField");
                                            strDBField2 = level2_item.GetAttribute("DestDBField");
                                            strDBFieldType = level2_item.GetAttribute("DBFieldType");

                                            _source.AddItem(new JsonBindingItem(strJsonField));
                                            _dest.AddItem(new DBFieldBindingItem(strDBField2, strDBFieldType));*/
                                        break;


                                    case BindingType.TagToTag:
                                        /*string strSourceMachine = level2_item.GetAttribute("SourceMachine");
                                            string strDestMachine = level2_item.GetAttribute("DestMachine");
                                            string strSourceTag = level2_item.GetAttribute("SourceTag");
                                            string strDestTag = level2_item.GetAttribute("DestTag");

                                            _source.AddItem(new MachineBindingItem(strSourceMachine, strSourceTag));
                                            _dest.AddItem(new MachineBindingItem(strDestMachine, strDestTag));*/

                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            break;
                        }
                    }
                }

                Log.Info("装载GroupDbBinding配置成功");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("装载GroupDbBinding配置出错：" + ex);
                return false;
            }
        }

        public void FreeResource()
        {
            throw new NotImplementedException();
        }

        public List<ResourceServiceModel> GetExportServices()
        {
            throw new NotImplementedException();
        }

        public string CallExportService(string ServiceName, string strParameter = null)
        {
            throw new NotImplementedException();
        }

        public void ExecuteBinding()
        {
            var transfer = _source.GetDataTable();

            _dest.SetValues(transfer);
        }
    }
}