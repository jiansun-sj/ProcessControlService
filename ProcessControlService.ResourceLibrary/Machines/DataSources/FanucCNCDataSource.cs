using System;
using ProcessControlService.ResourceLibrary.Machines.Drivers;
using ProcessControlService.ResourceFactory;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class FanucCNCDataSource : DataSource
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(FanucCNCDataSource));

        FanucCNCDriver cncdevice;
        //private string _IPAddress;

        public FanucCNCDataSource(string Name, Machine machine)
            : base(Name, machine)
        {
            //cncdevice = new FanucCNCDriver(ip);
        }

        public override void Disconnect()
        {

            //Connected = false; // comment by David 20170709

            cncdevice.Disconnect();
        }

        protected override bool Connect()
        {

            try
            {
                bool _connOK = cncdevice.Connect();
                //Thread.Sleep(200);
                if (_connOK)
                {
                    if (CheckConnected())
                    {
                        LOG.Info(string.Format("连接到CNC设备{0}成功.", SourceName));
                        return true;
                    }

                }
                LOG.Info(string.Format("连接到CNC设备{0}失败.", SourceName));
                return false;

            }
            catch (Exception ex)
            {
                LOG.Error($"CNC连接失败：{ex.Message}");
                return false;
            }

        }

        // 检查通信连接 - David 20170709
        [Obsolete]
        protected override bool CheckConnected()
        {
            Connected = (cncdevice != null && cncdevice.conn);
            return Connected;
        }


        public override bool UpdateAllValue()
        {
            try
            {
                // update from buffer to MonitorTags
                foreach (Tag tag in Tags.Values)
                {
                    if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                    {
                        tag.TagValue = cncdevice.ReadItem(tag.Address);
                        //LOG.Info(string.Format("读取CNC地址{0}值{1}", tag.Address, tag.TagValue));
                        if (null == tag.TagValue)
                        {
                            throw new Exception("读取失败！返回为null");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("读取CNC数据出错：{0}", ex.Message));
                Disconnect();
                return false;
            }
        }

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            if (Owner.Mode != RedundancyMode.Master)
            {
                return true;
            }

            try
            {
                //tag.TagValue = value;
                cncdevice.WriteItems(tag, value);
                LOG.Debug(string.Format("Tag{0}值{1}.", tag.TagName, value.ToString()));
                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("data source写入CNC出错：{0}", ex));
                return false;
            }
            ////立即写入
            //if (MonitorTags.ContainsKey(name))
            //{
            //    foreach (Tag tag in MonitorTags.Values)
            //    {
            //        if (tag.AccessType == TagAccessType.Read)
            //        {
            //            continue;
            //        }

            //        if (tag.TagName == name)
            //        {
            //            try
            //            {
            //                //tag.TagValue = value;
            //                cncdevice.WriteItems(tag,value);
            //                LOG.Debug(string.Format("Tag{0}值{1}.", tag.TagName,value.ToString()));
            //            }
            //            catch (Message ex)
            //            {
            //                LOG.Error(string.Format("data source写入CNC出错：{0}", ex));
            //            }
            //        }
            //    }

            //}
        }

        public override void AddItem(Tag tag)
        {
            base.AddItem(tag);
        }
        //public override void WriteTagToDevice(Tag tag)
        //{
        //    throw new Message("S7DataSource不支持WriteTagToDevice");

        //}

        public override bool LoadFromConfig(XmlNode node)
        {

            XmlElement level1_item = (XmlElement)node;

            // get ip address
            string _IPAddress = level1_item.GetAttribute("IPAddress");
            cncdevice = new FanucCNCDriver(_IPAddress);

            return base.LoadFromConfig(node);
        }
    }


}
