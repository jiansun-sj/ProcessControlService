// ==================================================
// 文件名：Machine.cs
// 创建时间：2020/06/16 13:32
// ==================================================
// 最后修改于：2020/08/05 13:32
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using log4net;
using Newtonsoft.Json;
using ProcessControlService.Contracts;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.Attributes;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Event;
using ProcessControlService.ResourceLibrary.Machines.Actions;
using ProcessControlService.ResourceLibrary.Machines.DataSources;
using ProcessControlService.ResourceLibrary.ResourceTemplate;

namespace ProcessControlService.ResourceLibrary.Machines
{
    public sealed partial class Machine : Resource /*替换为继承自Resource抽象类*/, IActionContainer, IEventContainer,
        IRedundancy, IWork
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Machine));

        public Machine(string name) : base(name)
        {
            EventsManagement.AddEventContainer(this);
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            // level0 -- "NeedDataSync" --add by Dongmin 20191107
            var level0Item = (XmlElement)node;
            if (level0Item.HasAttribute("NeedDataSync"))
            {
                var strNeedDataSync = level0Item.GetAttribute("NeedDataSync");
                NeedDataSync = strNeedDataSync.ToLower() == "true";
            }

            foreach (XmlNode level1Node in node)
            {
                // level1 --  "DataSources", "Alarms","Actions"
                if (level1Node.NodeType == XmlNodeType.Comment)
                    continue;
                try
                {
                    var level1Item = (XmlElement)level1Node;

                    if (string.Equals(level1Item.Name, "DataSources", StringComparison.CurrentCultureIgnoreCase))
                        LoadDataSourceFromXml(level1Node);
                    else if (string.Equals(level1Item.Name, "alarms",
                        StringComparison.CurrentCultureIgnoreCase))
                        LoadAlarmsFromXml(level1Node);
                    else if (string.Equals(level1Item.Name, "actions", StringComparison.CurrentCultureIgnoreCase))
                        LoadActionsFromXml(level1Node);
                    else if (string.Equals(level1Item.Name, "events", StringComparison.CurrentCultureIgnoreCase))
                        LoadEventsFromXml(level1Node);
                }
                catch (Exception ex)
                {
                    Log.Error($"加载机器{ResourceName}出错：{ex.Message}");
                }
            }

            //加载Machine Template中的配置，模块化配置Machine的Actions。
            if (!level0Item.HasAttribute("ResourceTemplate")) return true;
            var resourceTemplateName = level0Item.GetAttribute("ResourceTemplate");

            var machineTemplate =
                (MachineResourceTemplate)ResourceManager.GetResourceTemplate(resourceTemplateName);

            if (!machineTemplate.HasActions) return true;
            LoadActionsFromXml(machineTemplate.ActionsNode);

            Log.Info($"成功从MachineResourceTemplate中装载Actions至Machine: [{ResourceName}]中");

            #region comment

            #endregion

            return true;
        }

        public RedundancyMode Mode { get; private set; } = RedundancyMode.Unknown;

        // add by David 20170917
        public override void FreeResource()
        {
            StopWork();
        }

        #region IWork

        public bool WorkStarted { get; private set; }

        public void StartWork()
        {
            if (WorkStarted)
                return;

            if (CurrentRedundancyMode == RedundancyMode.Slave)
                return;

            WorkStarted = true;

            Log.Info($"设备{ResourceName}监控启动.");

            // Connect to opc data source
            foreach (var machineData in _machineDataSources)
                //machine_data.Connect(); //David 20170709
                machineData.StartEquipmentCommunication();

            StartAutoReadAlarm();
        }

        public void StopWork()
        {
            if (!WorkStarted)
                return;

            WorkStarted = false;

            Log.Info($"设备{ResourceName}监控停止.");

            StopAutoReadAlarm(); //停止报警更新

            //释放 DataSource
            foreach (var ds in _machineDataSources)
                //ds.Dispose(); // change by dongmin 20191105
                ds.StopEquipmentCommunication(); // add by dongmin 20191105
        }

        #endregion

        #region Redanduncy

        public RedundancyMode CurrentRedundancyMode => Mode;

        public void RedundancyModeChange(RedundancyMode mode)
        {
            Mode = mode;

            if (Mode != RedundancyMode.Slave)
                StartWork();
            else
                StopWork();
        }

        public bool NeedDataSync { get; private set; }

        public string BuildSyncData()
        {
            return PackData();
        }

        public void ExtractSyncData(string data)
        {
            try
            {
                //LOG.Debug(string.Format("主站Machine:{0},数据：{1}", ResourceName, data));
                var machineModel = UnpackData(data);

                foreach (var dsModel in machineModel.DataSources)
                {
                    var ds = GetDataSource(dsModel.DataSourceName);

                    //LOG.Debug(string.Format("写入DataSource:{0}", dsModel.DataSourceName));
                    var count = 0;

                    foreach (var tagModel in dsModel.Tags)
                    {
                        count++;
                        var tagName = tagModel.TagName;

                        var tag = GetTag(tagName);
                        tag.WriteValueInString(tagModel.TagValue);
                        //   LOG.Debug($"写入Tag:{TagName},数据：{tagModel.TagValue},耗時{stopwatchTag.ElapsedMilliseconds}ms, number: {count}");
                    }
                }

                // LOG.Info($"Machine Name:{ResourceName},Extract Sync Data, total time spent:{stopwatch.ElapsedMilliseconds}ms");
            }
            catch (IOException ex)
            {
                Log.Error($"从主站数据解析：{data}出错：{ex}");
            }
        }

        #endregion

        // 数据采集

        #region "Machine Data"

        public Tag GetTag(string tagName)
        {
            foreach (var machineData in _machineDataSources)
                if (machineData.Tags.ContainsKey(tagName))
                    return machineData.Tags[tagName];
            return null;
        }

        [ResourceService]
        public string GetTags(string Tags)
        {
            try
            {
                var tags = JsonConvert.DeserializeObject<string[]>(Tags);
                if (tags != null)
                {
                    List<object> val = new List<object>();
                    foreach (var item in tags)
                    {
                        var tag = GetTag(item);
                        val.Add(new KeyValuePair<string, string>(item, tag.TagValue == null ? "" : tag.TagValue.ToString()));
                    }
                    return JsonConvert.SerializeObject(val);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        ///     获取一个Machine所需要的所有tag值。
        ///     sunjian, 2019/10/20
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string GetAllTags(string json)
        {
            var dicTags = JsonConvert.DeserializeObject<List<string>>(json);

            var machineTags = (from tagName in dicTags
                               select GetTag(tagName)
                into tag
                               where tag != null
                               select new MachineTagModel
                               {
                                   Name = tag.TagName,
                                   Address = tag.Address,
                                   Type = tag.TagType,
                                   Value = tag.TagValue?.ToString(),
                                   AccessType = tag.AccessType.ToString(),
                                   Quality = tag.Quality.ToString()
                               }).ToList();

            return JsonConvert.SerializeObject(machineTags);
        }

        [ResourceService]
        public List<Tag> GetAllTags()
        {
            var tags = new List<Tag>();
            
            foreach (var machineDataSource in _machineDataSources)
            {
                tags.AddRange(machineDataSource.Tags.Values);
            }

            return tags;
        }

        /// <summary>
        ///     获取Machine所有的标签名和字符串形式标签值的字典返回值
        /// </summary>
        /// <returns></returns>
        [ResourceService]
        public Dictionary<string, string> GetAllTagValues()
        {
            return _machineDataSources.SelectMany(machineData => machineData.Tags).ToDictionary(
                machineDataTag => machineDataTag.Key, machineDataTag => machineDataTag.Value.GetValueInString());
        }

        [ResourceService]
        public DataSource GetDataSource(string dataSourceName)
        {
            foreach (var ds in _machineDataSources)
                if (ds.SourceName == dataSourceName)
                    return ds;
            return null;
        }

        [ResourceService]
        public List<DataSource> ListDataSource()
        {
            return _machineDataSources;
        }

        /// <summary>
        ///     获取单个点的值
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public string GetTagValueString(string tagName)
        {
            foreach (var machineData in _machineDataSources)
                if (machineData.Tags.ContainsKey(tagName))
                    return machineData.Tags[tagName].TagValue?.ToString() ?? "";
            return "";
        }

        public List<string> ListDataSourceName()
        {
            return _machineDataSources.Select(machineData => machineData.SourceName).ToList();
        }

        /// <summary>
        ///     获取Machine所有的Tags
        ///     Created by David 20170529
        /// </summary>
        /// <returns></returns>
        public List<string> ListTagNames()
        {
            var tagNames = new List<string>();

            foreach (var ds in _machineDataSources)
                tagNames.AddRange(ds.Tags.Keys);

            return tagNames;
        }

        public bool WriteTag(Tag tag, object value)
        {
            //Stopwatch stopwatch=new Stopwatch();

            return GetDataSource(tag).WriteTag(tag, value);

            // LOG.Info($"Machine write tag spent time: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        ///     shuyu  2019年8月17日 20:35:21
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="value"></param>
        /// sunjian 2019-12-15 改为bool返回值
        public bool WriteTag(string tagName, object value)
        {
            var tag = GetTag(tagName);
            if (tag != null) return WriteTag(tag, value);

            Log.Error($"Write Tag Error, Can't Find Tag [{tagName}] in Machine [{ResourceName}]");
            return false;
        }

        /// <summary>
        ///     Writed by David Dong
        ///     Created at 20170320
        ///     写入机器的值
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        public bool WriteTagWithStrValue(Tag tag, string strValue)
        {
            GetDataSource(tag).WriteTag(tag, tag.TranslateValueFromString(strValue));
            return true;
        }

        #endregion

        // 动作

        #region "Actions definitions"

        public void AddAction(BaseAction action)
        {
            _actions.AddAction(action);
        }

        public BaseAction GetAction(string name)
        {
            return (MachineAction)_actions.GetAction(name);
        }

        public void ExecuteAction(string name)
        {
            var action = (MachineAction)GetAction(name);
            action.Execute();
        }

        public string[] ListActionNames()
        {
            return _actions.ListActionNames();
        }

        #endregion

        // 报警

        #region "Alarms"

        public readonly Dictionary<string, AlarmDefinition> AlarmDefinitions = new Dictionary<string, AlarmDefinition>();

        public readonly LiveAlarmCollections LiveAlarms = new LiveAlarmCollections();

        public void ConfirmAlarm(string alarmId)
        {
            LiveAlarms.GetAlarm(alarmId).Confirm();
        }

        public List<MachineAlarmModel> GetAlarms()
        {
            return LiveAlarms.GetAlarmModels();
        }

        //public bool ContinueReading = false;
        private readonly object _locker = new object();

        public int UpdateAlarmInterval
        {
            get => _readAlarmInterval;
            set => _readAlarmInterval = value;
        }

        public void StartAutoReadAlarm()
        {
            _alarmTimer.Elapsed += ReadMachineAlarmThread; //到达时间的时候执行事件；   
            _alarmTimer.AutoReset = true; //设置是执行一次（false）还是一直执行(true)；   
            _alarmTimer.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件；   
        }

        public void StopAutoReadAlarm()
        {
            _alarmTimer.Enabled = false;
        }

        /// <summary>
        ///     读取机器报警线程
        ///     Changed by Dongmin 2017/05/30
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void ReadMachineAlarmThread(object source, ElapsedEventArgs e)
        {
            foreach (var almDef in AlarmDefinitions.Values)
            {
                var status = almDef.UpdateStatus();

                lock (_locker)
                {
                    LiveAlarms.ChangeAlarmStatus(almDef, status);
                }
            }
        }

        #endregion

        // 状态

        #region "Status"

        public object GetStatus(string statusName)
        {
            return _statusList[statusName];
        }

        public string GetStatusString(string statusName)
        {
            return _statusList[statusName].GetValueString();
        }

        public string[] ListStatusName()
        {
            return _statusList.Keys.ToArray();
        }

        public void UpdateAllStatus()
        {
            foreach (var status in _statusList.Values) status.UpdateData();
        }

        #endregion

        // 事件

        #region "Event"

        public void AddEvent(BaseEvent @event)
        {
            _events.AddEvent(@event);
        }

        public BaseEvent GetEvent(string name)
        {
            return _events.GetEvent(name);
        }

        public string[] ListEventNames()
        {
            return _events.ListEventNames();
        }

        #endregion
    }
}