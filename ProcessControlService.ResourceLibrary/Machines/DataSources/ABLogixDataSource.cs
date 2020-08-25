using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Machines.Drivers;
using System;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class ABLogixDataSource : DataSource
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ABLogixDataSource));
        ABLogixDriver PLC;
        public ABLogixDataSource(string Name, Machine machine)
            : base(Name, machine)
        {
        }
        public override bool LoadFromConfig(XmlNode node)
        {

            var level1Item = (XmlElement)node;

            // get ip address
            string IP = level1Item.GetAttribute("IPAddress");
            string Port = level1Item.GetAttribute("Port");
            string Slot = level1Item.GetAttribute("Slot");
            PLC = new ABLogixDriver(IP, Port, Slot);
            return base.LoadFromConfig(node);
        }
        protected override void Disconnect()
        {
            PLC.Disconnect();
        }

        protected override bool Connect()
        {

            try
            {
                bool _connOK = PLC.Connect();
                //Thread.Sleep(200);
                if (_connOK)
                {
                    if (CheckConnected())
                    {
                        LOG.Info(string.Format("连接到AB设备{0}成功.", SourceName));
                        return true;
                    }

                }
                LOG.Info(string.Format("连接到AB设备{0}失败.", SourceName));
                return false;

            }
            catch (Exception ex)
            {
                //LOG.Info(string.Format("连接到设备{0}失败.", SourceName));
                LOG.Error(string.Format("AB_PLC连接失败：{0}", ex.Message));
                return false;
            }

        }
        protected override bool Connected
        {
            get { return ((PLC != null) && PLC.IsConnected()); }
        }

        // 检查通信连接 - David 20170709
        protected override bool CheckConnected()
        {
            Connected = ((PLC != null) && PLC.IsConnected());
            return Connected;
        }


        public override bool UpdateAllValue()
        {
            lock (this) // david 20170617
            {
                try
                {
                    // read from device
                    if (PLC.UpdateBlock())
                    {// update from buffer to Tags
                        foreach (Tag tag in Tags.Values)
                        {
                            if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                            {
                                try
                                {
                                    tag.TagValue = PLC.ReadItem(tag.Address);
                                    //if (tag.TagType == "string" && tag.TagValue.ToString().Contains("\0"))
                                    //{
                                    //    tag.TagValue = tag.TagValue.ToString().Replace("\0","");
                                    //}
                                    
                                    tag.Quality = Quality.Good;
                                   

                                }
                                catch (Exception ex)
                                {
                                    LOG.Error($"数据源[{SourceName}]读取数据出错 Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");
                                    tag.Quality = Quality.Bad;
                                }
                            }
                        }
                        return true;
                    }
                    else
                    {
                        LOG.Error("AB_PLC.UpdateBlock出错!");
                        Disconnect();
                        return false;
                    }
                }
                catch (Exception)
                {
                    Disconnect();
                    return false;
                }
            }
        }

        protected override bool WriteTagToRealDevice(Tag tag, object value)
        {
            //if (Owner.CurrentRedundancyMode != RedundancyMode.Master)
            //{
            //    return;
            //}

            if (Owner.Mode != RedundancyMode.Master)
            {
                return true;
            }

            lock (this) // david 20170617
            {
                try
                {
                    //PLC.WriteItems(tag, value);
                    LOG.Debug(string.Format("Tag {0}值{1}.", tag.TagName, value));
                    LOG.Error("暂时不支持写入操作!");
                    return true;
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("S7写入PLC出错：{0}", ex));
                    return false;
                }

            }
        }

        public override object ReadTag(Tag tag)
        {
            return PLC.ReadItem(tag.Address);
        }

        public override void AddItem(Tag tag)
        {
            PLC.AddAddress(tag);

            base.AddItem(tag);
        }

    }


}
