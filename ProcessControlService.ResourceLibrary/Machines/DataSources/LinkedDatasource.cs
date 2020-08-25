using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class LinkedDataSource : DataSource
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LinkedDataSource));

        private Func<bool> _connectedFunc;
        public string LinkedDataSourceName;
        public string LinkedMachineName;

        private Func<Tag, object> _readTagFunc;

        private Func<Tag, object, bool> _writeTagToRealDeviceFunc;

        public LinkedDataSource(string name, Machine machine) : base(name, machine)
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
                foreach (var tag in Tags.Values)
                {
                    if (_readTagFunc == null) continue;
                    tag.TagValue = _readTagFunc(tag);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            return _writeTagToRealDeviceFunc(tag, value);
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            var level1Item = (XmlElement)node;

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
                var machine = (Machine)ResourceManager.GetResource(LinkedMachineName);

                var masterDataSource = machine.GetDataSource(LinkedDataSourceName);

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