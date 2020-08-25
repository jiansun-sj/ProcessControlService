// ==================================================
// 文件名：MachinePartialClass.cs
// 创建时间：2020/06/16 18:00
// ==================================================
// 最后修改于：2020/07/10 18:00
// 修改人：jians
// ==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Timers;
using System.Xml;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Event;
using ProcessControlService.ResourceLibrary.Machines.Actions;
using ProcessControlService.ResourceLibrary.Machines.DataSources;

namespace ProcessControlService.ResourceLibrary.Machines
{
    public sealed partial class Machine
    {
        #region "Actions definitions"

        private readonly ActionCollection _actions = new ActionCollection();

        #endregion

        #region "Event"

        private readonly EventCollection _events = new EventCollection();

        #endregion

        #region "Status"

        private readonly Dictionary<string, MachineStatus> _statusList = new Dictionary<string, MachineStatus>();

        #endregion

        #region "Resource definition"

        private void LoadEventsFromXml(IEnumerable level1Node)
        {
            #region events

            foreach (XmlNode level2Node in level1Node)
            {
                // level2 --  "events"

                //add
                if (level2Node.NodeType == XmlNodeType.Comment)
                    continue;

                var level2Item = (XmlElement)level2Node;

                // 动态创建Event
                var Name = level2Item.GetAttribute("Name");
                var EventType = level2Item.GetAttribute("Type");

                var Event = EventsManagement.CreateEvent(EventType, Name);
                Event.OwnerResource = this;

                if (Event == null)
                    throw new Exception($"创建Event 名称{Name},类型{EventType}失败");
                if (Event.LoadFromConfig(level2Item))
                    // 加入到Machine的Event集合
                    try
                    {
                        AddEvent(Event);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"加载机器{ResourceName} 的Event:{Name}出错:{ex}");
                    }
                else
                    Log.Error($"加载机器{ResourceName} 的Event:{Name}出错");
            }

            #endregion
        }

        private void LoadAlarmsFromXml(IEnumerable level1Node)
        {
            #region alarms

            foreach (XmlNode level2Node in level1Node)
            {
                // level2 --  "alarm"
                if (level2Node.NodeType == XmlNodeType.Comment)
                    continue;

                var level2Item = (XmlElement)level2Node;

                if (!string.Equals(level2Item.Name.ToLower(), "alarm",
                    StringComparison.CurrentCultureIgnoreCase)) continue;
                var almDef = AlarmDefinition.CreateFromConfig(level2Node, this);

                almDef.LoadFromConfig(level2Node);

                if (!AlarmDefinitions.ContainsKey(almDef.AlarmID)) //add by David 20170621,检测是否有重复报警
                    AlarmDefinitions.Add(almDef.AlarmID, almDef); //增加到数据源集合
                else
                    throw new Exception($"添加重复的报警项:{almDef.AlarmID}");
            }

            #endregion
        }

        private void LoadDataSourceFromXml(IEnumerable level1Node)
        {
            #region datasources

            foreach (XmlNode level2Node in level1Node)
            {
                // DataSource
                if (level2Node.NodeType == XmlNodeType.Comment)
                    continue;

                var level2Item = (XmlElement)level2Node;
                if (!string.Equals(level2Item.Name.ToLower(), "DataSource",
                    StringComparison.CurrentCultureIgnoreCase)) continue;
                //DataSource source = DataSource.CreateFromConfig(level2_node, this);

                var type = level2Item.GetAttribute("Type");
                var name = level2Item.GetAttribute("Name");
                var source = DataSourceManagement.CreateDataSource(type, name, this);

                foreach (var dataSource in _machineDataSources)
                    if (dataSource.SourceName == source.SourceName)
                        throw new Exception("DataSource 加载出错:名字重复");

                if (source.LoadFromConfig(level2Node))
                    _machineDataSources.Add(source); //增加到数据源集合
                else
                    throw new Exception("DataSource 加载出错");
            }

            #endregion
        }

        private void LoadActionsFromXml(IEnumerable level1Node)
        {
            #region actions

            foreach (XmlNode level2Node in level1Node)
            {
                // level2 --  "actions"

                //add
                if (level2Node.NodeType == XmlNodeType.Comment)
                    continue;
                //add:gu 20170223

                var level2Item = (XmlElement)level2Node;

                // 动态创建Action
                var name = level2Item.GetAttribute("Name");
                var actionType = level2Item.GetAttribute("Type");

                var action = (MachineAction)ActionsManagement.CreateAction(actionType, name);
                action.OwnerMachine = this;

                if (action.LoadFromConfig(level2Item))
                    // 加入到Machine的Action集合
                    try
                    {
                        AddAction(action);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"加载机器{ResourceName} 的Action:{name}出错:{ex}");
                    }
                else
                    Log.Error($"加载机器{ResourceName} 的Action:{name}出错");
            }

            #endregion
        }

        #endregion

        #region "Machine Data"

        private readonly List<DataSource> _machineDataSources = new List<DataSource>();

        private DataSource GetDataSource(Tag tag)
        {
            foreach (var machineData in _machineDataSources)
            {
                var strings = tag.TagName.Split('@');

                if (machineData.Tags.ContainsKey(strings[0]) /*||
                    machineData.Tags.ContainsKey(tag.TagName.Substring(0, ) ) *//*LinkedDataSource进行为Tag型关联时Tag名带有资源名后缀 sunjian 2020/7/10 长春*/
                )
                    return machineData;
            }
            return null;
        }

        #endregion

        #region "Alarms"

        private static int _readAlarmInterval = 1000; // 默认1s读一次数据
        private readonly Timer _alarmTimer = new Timer(_readAlarmInterval); //实例化Timer类，设置间隔时间为10000毫秒；   

        #endregion

        #region 数据打包

        private string PackData()
        {
            var machineModel = new MachineModel(this);

            var json = new DataContractJsonSerializer(machineModel.GetType());
            var szJson = "";
            using (var stream = new MemoryStream())
            {
                json.WriteObject(stream, machineModel);
                szJson = Encoding.UTF8.GetString(stream.ToArray());
            }

            return szJson;
        }

        private MachineModel UnpackData(string strData)
        {
            var ser = new DataContractJsonSerializer(typeof(MachineModel));

            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(strData)))
                {
                    var obj = ser.ReadObject(ms) as MachineModel;

                    //LOG.Debug($"Machine Name:{ResourceName}, Unpack Data, time spent:{stopwatch.ElapsedMilliseconds}ms");
                    return obj;
                }
            }
            catch (IOException ex)
            {
                Log.Error($"反序列化MachineModel：{strData}出错：{ex}");
                return null;
            }
        }

        #endregion
    }
}