// ==================================================
// 文件名：S7LinkedDataSource.cs
// 创建时间：2020/07/10 17:38
// ==================================================
// 最后修改于：2020/07/10 17:38
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class S7LinkedDataSource : DataSource
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(S7LinkedDataSource));

        private Func<bool> _connectedFunc;
        public string LinkedDataSourceName;
        public string LinkedMachineName;

        private Func<Tag, object> _readTagFunc;
        
        private Func<Tag, object, bool> _writeTagToRealDeviceFunc;

        public S7LinkedDataSource(string name, Machine machine) : base(name, machine)
        {
        }

        protected override bool Connected => _connectedFunc();

        public override void Disconnect()
        {
        }

        //Machine启动时，LinkedDataSource不需要去实际连接PLC
        protected override bool Connect()
        {
            return true;
        }

        public override bool UpdateAllValue()
        {
            try
            {
                //foreach (var tag in Tags.Values) ReadTag(tag);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override object ReadTag(Tag tag)
        {
            return _readTagFunc(tag);
        }

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            return _writeTagToRealDeviceFunc(tag, value);
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            var level1Item = (XmlElement) node;

            LinkedMachineName = level1Item.GetAttribute("LinkedMachineName");
            LinkedDataSourceName = level1Item.GetAttribute("LinkedDataSourceName");

            if (level1Item.HasAttribute("LinkedIpAddress"))
                LinkedIpAddress = level1Item.GetAttribute("LinkedIp");

            if (!base.LoadFromConfig(node))
            {
                return false;
            }
            
            try
            {
                var machine = (Machine) ResourceManager.GetResource(LinkedMachineName);

                var masterDataSource = machine.GetDataSource(LinkedDataSourceName);

                if (masterDataSource is S7DataSource s7DataSource)
                {
                    foreach (var keyValuePair in Tags)
                    {
                        keyValuePair.Value.TagName += "@"+Owner.ResourceName;
                        s7DataSource.AddItem(keyValuePair.Value);
                    }
                }

                _readTagFunc = masterDataSource.ReadTag;

                _writeTagToRealDeviceFunc = masterDataSource.WriteTagToRealDevice;

                _connectedFunc = masterDataSource.IsConnect;
            }
            catch (Exception e)
            {
                Log.Error($"链接DataSource:[{LinkedMachineName}].[{LinkedDataSourceName}]失败，异常为：{e.Message}.");
                return false;
            }

            return true;
        }

        public string LinkedIpAddress { get; set; }
    }
}