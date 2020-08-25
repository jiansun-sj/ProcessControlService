// ==================================================
// 文件名：DataSource.cs
// 创建时间：2020/06/16 11:52
// ==================================================
// 最后修改于：2020/08/17 11:52
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using log4net;
using S7.Net;

// 代码修改记录
// 20170523 David dong
//
namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public abstract class DataSource : IDisposable
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DataSource));

        public readonly Machine Owner;

        public readonly string SourceName;

        public bool ReadDataOpen = true; //20180113   robin


        public Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

        protected DataSource(string name, Machine machine)
        {
            SourceName = name;
            Owner = machine;
        }

        protected virtual bool Connected { get; set; } = false;

        // 装载数据源 Dongmin 20171230
        //<DataSource Type="Internal">
        //    <Tag Type="Tag" Name="Signal1" TagType="bool" DefaultValue="false" />
        //    <Tag Type="Tag" Name="Level1" TagType="float" DefaultValue="0.0" />
        //</DataSource>
        public static DataSource CreateFromConfig(XmlNode node, Machine machine)
        {
            // level --  "DataSource"
            if (node.NodeType == XmlNodeType.Comment)
                return null;

            try
            {
                var level1Item = (XmlElement) node;

                var strDsType = level1Item.GetAttribute("Type");
                var strDsName = level1Item.GetAttribute("Name");

                ///////////////////////////////////////
                // 使用反射创建数据源

                var appPath = Assembly.GetExecutingAssembly().GetName().Name;
                var fullDsType = appPath + ".Machines.DataSources." + strDsType; // 目前datasource只能在基础包里
                var objType = Type.GetType(fullDsType, true);
                var obj = Activator.CreateInstance(objType, strDsName, machine);
                ////////////////////////////////////////

                return (DataSource) obj;
            }
            catch (Exception ex)
            {
                LOG.Error($"加载数据源{node.Name}出错：{ex.Message}");
                return null;
            }
        }

        #region "dispose resource"

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     非密封类修饰用protected virtual
        ///     密封类修饰用private
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            LOG.Debug("DataSource被释放");
            if (_disposed) return;
            if (disposing)
                // 清理托管资源
                StopEquipmentCommunication(); // add by Dongminm 20191105
            // 清理非托管资源

            //让类型知道自己已经被释放
            _disposed = true;
        }

        #endregion

        #region "Polling Timer"

        // 时间片控制 -- Add by David 20170614
        public short ReadTimeGroup = -1; //-1 == no time group 

        public int UpdateInterval { get; set; } = 1000;


        private Thread _readDataThread;

        public void StartEquipmentCommunication()
        {
            if (ReadTimeGroup < 1 || ReadTimeGroup > 10) //如果不是时间组调用
            {
                //Connect(); //连接数据源
                _readDataThread = new Thread(NewAccessMachineDataThread);
                _readDataThread.Start();
                //
                //开一个单独的写入线程
                WriteTagThread();

                EnableWriteToDevice = true;
            }
        }

        // 停止设备通信
        // Add by dongmin 20191105
        public void StopEquipmentCommunication()
        {
            _readDataThread?.Abort();

            EnableWriteToDevice = false;

            Disconnect();
        }

        public virtual void WriteTagThread()
        {
            //父类啥也不干，主要实现在子类
        }

        //robin新增加  20180113
        private void NewAccessMachineDataThread()
        {
            Connect();
            
            while (true)
            {
                ReadDataSource();
                Thread.Sleep(UpdateInterval);

                //  LOG.Info($"Update Interval: {_readInterval}");
            }
        }

        public const short ReconnectTimes = 5;
        private short _currentReconnectTimes;

        /// <summary>
        ///     实际更新时间
        /// </summary>
        public long ActuallyInterval = 0;

        private void ReadDataSource()
        {
            var isConnected= Connected;
            
            //LOG.Info($"DataSource:[{SourceName}], 连接状态：[{isConnected}]");
            
            
            if (isConnected)
            {
                if (!UpdateAllValue()) LOG.Error($"设备数据源[{Owner.ResourceName}.{SourceName}],类{GetType()} 读取出错");
            }
            else
            {
                SetAllTagQualityBad();

                LOG.Error($"设备数据源[{Owner.ResourceName}.{SourceName}],类{GetType()} 尝试重连{_currentReconnectTimes}");
               
                if (_currentReconnectTimes >= ReconnectTimes)
                {
                    //重连
                    _currentReconnectTimes = 0;
                    SetAllTagQualityBad();

                    Disconnect();
                    
                    Connect();
                    
                    Thread.Sleep(100);
                }

                _currentReconnectTimes++;
            }
        }

        private void SetAllTagQualityBad()
        {
            foreach (var tag in Tags.Values) tag.Quality = Quality.Bad;
        }

        #endregion

        #region "overides"

        public virtual void AddItem(Tag tag)
        {
            try
            {
                Tags.Add(tag.TagName, tag);
            }
            catch (Exception ex)
            {
                LOG.Error($"添加Tag：{tag.TagName}出错：{ex.Message}");
            }
        }

        public virtual void AddItem(Machine owner, string name, string type, string address)
        {
            var newTag = new Tag(name, owner)
            {
                //new_tag.TagName = Name;
                TagType = type,
                Address = address
            };

            Tags.Add(name, newTag);
        }


        /// <summary>
        ///     连接
        /// </summary>
        /// <returns></returns>
        protected abstract bool Connect();

        /// <summary>
        ///     断开连接
        /// </summary>
        public abstract void Disconnect();

        public bool IsConnect()
        {
            return Connected;
        }

        /// <summary>
        ///     推荐使用Connected字段
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        protected virtual bool CheckConnected()
        {
            return false;
        }


        public bool EnableWriteToDevice { get; private set; } = true;

        public bool WriteTag(Tag tag, object value)
        {
            if (tag.AccessType == TagAccessType.ReadWrite || tag.AccessType == TagAccessType.Write)
            {
                var strings = tag.TagName.Split('@');

                if (Tags.ContainsKey(strings[0]))
                {
                    if (EnableWriteToDevice) // change by Dongmin 20191105
                    {
                        var writeTagSuccessful = WriteTagToRealDevice(tag, value); //Robin  20180227

                        return writeTagSuccessful;
                    }

                    //仅写入值，不写设备
                    tag.TagValue = value; // add by dongmin 20191105
                    return true;
                }

                LOG.Debug($"写tag值出错，{tag.TagName}字典中不包含该tag");
                return false;
            }

            LOG.Debug($"写tag值出错，{tag.TagName}不允许写入");
            return false;
        }

        /// <summary>
        ///     单点读取
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual object ReadTag(Tag tag)
        {
            return null;
        }

        public abstract bool WriteTagToRealDevice(Tag tag, object value);

        public abstract bool UpdateAllValue();

        public virtual bool LoadFromConfig(XmlNode node)
        {
            var level0Item = (XmlElement) node;
            // get update interval
            if (level0Item.HasAttribute("UpdateInterval"))
                UpdateInterval = Convert.ToInt16(level0Item.GetAttribute("UpdateInterval"));

            foreach (XmlNode level1Node in node)
            {
                //Tags
                if (level1Node.NodeType == XmlNodeType.Comment)
                    continue;

                var level1Item = (XmlElement) level1Node;
                if (level1Item.Name.ToLower() != "tag") continue; // load Tags

                var newTag = new Tag(Owner);
                newTag.LoadFromConfig(level1Node);

                AddItem(newTag);
            }

            return true;
        }

        #endregion
    }

    [DataContract]
    internal class DataSourceModel
    {
        [DataMember] public string DataSourceName;

        [DataMember] public List<TagModel> Tags;

        public DataSourceModel(DataSource dataSource)
        {
            DataSourceName = dataSource.SourceName;

            Tags = new List<TagModel>();

            foreach (var tag in dataSource.Tags.Values) Tags.Add(new TagModel(tag));
        }
    }
}