using System;
using System.Collections.Generic;
using System.Threading;
using ProcessControlService.ResourceLibrary.Machines.Drivers;
using ProcessControlService.ResourceFactory;
using System.Xml;


namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{

    /// <summary>
    /// 
    /// </summary>
    public class ModbusTCPDataSource : DataSource
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ModbusTCPDataSource));

        // Modbus 
        private ModbusTCPMasterDriver _modbusDevice = new ModbusTCPMasterDriver();

        public ModbusTCPDataSource(string Name, Machine machine)
            : base(Name, machine)
        {
        }

        public string IPAddress;

        public short PortNo;

        public ushort InputStatusStartAddress;
        public ushort InputStatusSize;

        //private byte slaveid;

        //public byte SlaveID
        //{
        //    get { return slaveid; }
        //    set { slaveid = value; }
        //}

        public override void AddItem(Tag tag)
        {
            base.AddItem(tag);
        }

        public override bool LoadFromConfig(XmlNode node)
        {

            XmlElement level1_item = (XmlElement)node;
            string strIPAddress = level1_item.GetAttribute("IPAddress");
            string strPort = level1_item.GetAttribute("Port");
            string strInputStatusStartAddress = level1_item.GetAttribute("InputStatusStartAddress");
            string strInputStatusSize = level1_item.GetAttribute("InputStatusSize");
            IPAddress = strIPAddress;
            PortNo = Convert.ToInt16(strPort);
            InputStatusStartAddress = Convert.ToUInt16(strInputStatusStartAddress);
            InputStatusSize = Convert.ToUInt16(strInputStatusSize);

            return base.LoadFromConfig(node);
        }

        private List<ModbusAddressType> _addressList;
        private void GenAddressTypeList()
        {
            _addressList = new List<ModbusAddressType>();

            foreach (Tag tag in Tags.Values)
            {
                if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                {
                    ModbusAddressType _address = ModbusTCPMasterDriver.GetAddressType(tag.Address);

                    //if (_address!=ModbusAddressType.WC&&_address!=ModbusAddressType.WR&&!_addressList.Contains(_address))
                    if (_address == ModbusAddressType.IC)
                    {
                        if (!_addressList.Contains(ModbusAddressType.IC))
                        {
                            _addressList.Add(_address);
                        }
                    }
                    else
                    {
                        LOG.Error(string.Format("ModbusTCPDataSource-GenAddressTypeList出错：不支持{0}类型", tag.Address));
                    }
                }
            }
        }

        public override void Disconnect()
        {
            //ContinueReading = false;

            //Connected = false;

            //Thread.Sleep(1000); // Waiting for the auto read OPC thread stop

            _modbusDevice.DisconnectFromClient();
        }

        protected override bool Connect()
        {

            try
            {
                bool _connOK = _modbusDevice.ConnectToClient(IPAddress, PortNo, InputStatusStartAddress, InputStatusSize);

                if (_connOK)
                {
                    if (CheckConnected())
                    {
                        LOG.Info($"连接到ModbusTCP设备{SourceName}成功.");

                        _modbusDevice.DataArrival += UpdateAllDataFinished;

                        GenAddressTypeList(); // 记住地址列表

                        return true;
                    }

                }

                LOG.Info($"连接到ModbusTCP设备{SourceName}失败.");
                return false;
            }
            catch (Exception ex)
            {
                LOG.Error($"连接到ModbusTCP设备{SourceName}失败：{ex.Message}");
                return false;
            }

        }

        // Changed by David 20170529
        public override bool UpdateAllValue()
        {
            lock (this)
            {
                try
                {

                    //LOG.Info("ModbusTCP_UpdateAllValue");
                    foreach (ModbusAddressType addressType in _addressList)
                    {

                        //LOG.Debug(string.Format("开始读取{0}号从站{1}数据,类型是：{2},GroupID:{3}", slaveid, SourceName, address, ReadTimeGroup));

                        //_modbusDevice.UpdateAddressArea(address); // 更新通信缓存
                        //_modbusDevice.AsyncReadAllData();
                        _modbusDevice.SyncReadAllData(addressType); //同步读取
                        //为了编译通过

                        //LOG.Debug(string.Format("读取{0}号从站{1}数据完毕,类型是：{2},GroupID:{3}", slaveid, SourceName, address, ReadTimeGroup));
                        Thread.Sleep(100);
                    }

                    return true;
                }
                catch (Exception ex)
                {

                    LOG.Error($"读取ModbusTCP设备数据出错：{ex.Message}");
                    Disconnect();
                    return false;
                }
            }
        }

        // 检查通信连接 - David 20170709
        [Obsolete]
        protected override bool CheckConnected()
        {
            Connected = (_modbusDevice != null && _modbusDevice.Connected);
            return Connected;
        }


        //public override void WriteTagToDevice(Tag tag)
        //{
        //    throw new Message("不支持WriteTagToDevice");


        //}

        private void UpdateAllDataFinished(object sender, ModbusTCPDataUpdatedEventArgs args)
        {
            //LOG.Info("ModbusTCP_UpdateAllDataFinished");
            // Update MonitorTags Value
            //foreach (Tag tag in MonitorTags.Values)
            //{
            //    tag.TagValue = ReadTagFromBuffer(tag.Address);
            //}

            foreach (Tag tag in Tags.Values)
            {
                if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                {
                    //if (MonitorTags[tag].Address.Substring(0, 2) == "IC")
                    //{
                    tag.TagValue = ReadTagFromBuffer(tag);
                    //LOG.Info(string.Format("读取Modbus地址{0}值{1}", MonitorTags[tag].Address, MonitorTags[tag].TagValue));
                    //}
                }
            }
        }

        //private void UpdateAllDataFinished(object sender, ModbusRTUDataUpdatedEventArgs args)
        //{
        //    // Update MonitorTags Value
        //    //foreach (Tag tag in MonitorTags.Values)
        //    //{
        //    //     tag.TagValue = ReadTagFromBuffer(tag.Address);
        //    //}
        //    foreach (string tag in MonitorTags.Keys)
        //    {
        //        if (tag.Contains("WI"))
        //            continue;
        //        MonitorTags[tag].TagValue = ReadTagFromBuffer(tag,MonitorTags[tag]);
        //    }

        //}

        private object ReadTagFromBuffer(Tag tag)
        {
            try
            {
                //if (ItemID.Contains("WI"))
                //    return null;
                ////Tag _tag = MonitorTags[ItemID]; 注释Robin 20180507
                //if (_tag.TagType == "float" && value.Address.Contains("IR"))
                //{
                //    int a = 0;
                //}
                if (tag.TagType == "bool")
                {
                    bool Value1 = false;
                    //_modbusDevice.GetValue(ItemID, ref Value);
                    _modbusDevice.GetValue(tag.Address, ref Value1);
                    //LOG.Info(string.Format("读取Modbus地址{0}值{1}", value.Address, Value1));
                    return Value1;
                }
                //else if(_tag.TagType.Equals("float"))
                //{
                //    float Value2=0.0F;
                //    //_modbusDevice.GetValue(ItemID, ref Value);
                //    _modbusDevice.GetValueFloat(value.Address, ref Value2);
                //    return Value2;
                //}
                //else if (_tag.TagType == "uint16")
                //{
                //    ushort Value3 = 0;
                //    //_modbusDevice.GetValue(ItemID, ref Value);
                //    _modbusDevice.GetValue(value.Address, ref Value3);
                //    return Value3;
                //}
                LOG.Error("ModBusTCP--ReadTagFromBuffer出错");
                return null;
            }
            catch (Exception ex)
            {
                LOG.Error("ModBusTCP--ReadTagFromBuffer出错" + ex.Message);
                return null;
            }
        }


        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            if (Owner.Mode != RedundancyMode.Master)
            {
                return true;
            }

            lock (this)
            {
                try
                {
                    ushort TagAddress = Convert.ToUInt16(tag.Address.Substring(2));

                    if (tag.Address.Substring(0, 2) == "WC")
                    {
                        _modbusDevice.WriteItem(TagAddress, Convert.ToBoolean(value));
                        return true;
                    }
                    //else if (MonitorTags[ItemID].Address.Substring(0, 2) == "WR")
                    //{
                    //    ushort WrValue = Convert.ToUInt16(value);
                    //    _modbusDevice.WriteItem(TagAddress, WrValue);
                    //}
                    //}
                    else
                    {
                        LOG.Error(string.Format("写入modus出错:写入地址错误！"));
                        return false;
                    }

                    ////立即写入
                    //if (MonitorTags.ContainsKey(ItemID))
                    //{
                    //    ushort TagAddress = Convert.ToUInt16(MonitorTags[ItemID].Address.Substring(2));

                    //    if (MonitorTags[ItemID].Address.Substring(0, 2) == "WC")
                    //    {
                    //        _modbusDevice.WriteItem(TagAddress, Convert.ToBoolean(value));
                    //    }
                    //    //else if (MonitorTags[ItemID].Address.Substring(0, 2) == "WR")
                    //    //{
                    //    //    ushort WrValue = Convert.ToUInt16(value);
                    //    //    _modbusDevice.WriteItem(TagAddress, WrValue);
                    //    //}
                    //    //}
                    //    else
                    //    {
                    //         LOG.Error(string.Format("写入modus出错:写入地址错误！"));
                    //    }

                    //}
                    ////}
                    ////}
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("写入函数出错：{0}", ex.Message));
                    return false;
                }
            }
        }
    }


}
