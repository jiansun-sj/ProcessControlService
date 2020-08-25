using ProcessControlService.ResourceLibrary.Machines.DataSources.Utils;
using System;
using System.Linq;
using System.Xml;
using YumpooDrive;
using YumpooDrive.ModBus;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{

    /// <summary>
    /// add by zsy
    /// 使用ModbusTcpNet替换原有ModbusTCPMasterDriver，配置文件参考backup/ModbusTcpTest.xml
    /// </summary>
    public class ModbusTCPDataSourceNew : DataSource
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ModbusTCPDataSourceNew));
        private ModbusTcpNet _modbusDevice = new ModbusTcpNet();
        private string _ip;
        private int _port;

        protected override bool Connected => (_modbusDevice != null && _modbusDevice.IsConnect);

        public ModbusTCPDataSourceNew(string Name, Machine machine)
            : base(Name, machine)
        {
        }

        public override bool LoadFromConfig(XmlNode ele)
        {
            XmlElement node = (XmlElement)ele;
            try
            {
                _ip = node.GetAttribute("IP");
                string Port = node.GetAttribute("Port");

                _port = Convert.ToInt32(Port);
            }
            catch (Exception ex)
            {
                LOG.Error($"Load ModbusTcpDataSource Config Failed {ex.Message}");
            }

            return base.LoadFromConfig(node);
        }

        public override void Disconnect()
        {
            _modbusDevice.ConnectClose();
        }

        protected override bool Connect()
        {
            try
            {
                _modbusDevice.IpAddress = _ip;
                _modbusDevice.Port = _port;
                var res = _modbusDevice.ConnectServer();

                if (res.IsSuccess)
                {
                    LOG.Info($"Connect to ModbusTcp device [{SourceName}] success.");
                    return true;
                }
                else
                {
                    LOG.Info($"Connect to ModbusTcp device [{SourceName}] failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LOG.Error($"Connect to ModbusTcp device [{SourceName}] error. Exception:{ex.Message}");
                return false;
            }
        }

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            lock (this)
            {
                OperateResult res = new OperateResult();

                try
                {
                    var address = tag.Address.ToLower();

                    if (address.Contains("i"))
                    {
                        throw new Exception("Error tag address.Can not write this tag");
                    }

                    string station = "";
                    if (address.StartsWith("s") && address.Contains(";"))
                    {
                        var ads = address.Split(';');
                        station = ads[0];
                        address = ads[1];
                    }


                    string reg = "";
                    if (address.StartsWith("di") || address.StartsWith("do") || address.StartsWith("ai") || address.StartsWith("ao"))
                    {
                        reg = address.Substring(0, 2);
                        address = address.Replace(reg, "");
                    }
                    else
                    {
                        throw new Exception("Error tag address.");
                    }


                    if (!string.IsNullOrEmpty(station))
                    {
                        address = station + ";" + address;
                    }

                    switch (tag.TagType)
                    {
                        case "bool":
                            if (reg == "do")
                            {
                                res = _modbusDevice.WriteCoil(address, (bool)value);
                            }
                            break;

                        case "string":
                            res = _modbusDevice.Write(address.Split('.')[0], ConvertUtils.GetBytes(tag, value));
                            break;
                        default:
                            res = _modbusDevice.Write(address, ConvertUtils.GetBytes(tag, value).Reverse().ToArray());
                            break;
                    }
                    LOG.Info($"Datasource[{SourceName}] Write tag. Tag[{tag.TagName}] Address[{tag.Address}] IsSuccess[{res.IsSuccess}]");

                }
                catch (Exception ex)
                {
                    LOG.Error($"Datasource[{SourceName}] write error. Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");

                }
                return res.IsSuccess;
            }
        }

        public override bool UpdateAllValue()
        {
            foreach (var tag in Tags.Values)
            {
                try
                {
                    Read(tag);
                }
                catch (Exception ex)
                {
                    LOG.Error($"Datasource[{SourceName}] read error. Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");
                }
            }
            return true;
        }
        private void Read(Tag tag)
        {
            var address = tag.Address.ToLower();
            string station = "";
            if (address.StartsWith("s") && address.Contains(";"))
            {
                var ads = address.Split(';');
                station = ads[0];
                address = ads[1];
            }

            string reg;
            if (address.StartsWith("di") || address.StartsWith("do") || address.StartsWith("ai") || address.StartsWith("ao"))
            {
                reg = address.Substring(0, 2);
                address = address.Replace(reg, "");
            }
            else
            {
                throw new Exception("Error tag address.");
            }

            if (reg == "ai")
            {
                address = "x=4;" + address;
            }

            if (!string.IsNullOrEmpty(station))
            {
                address = station + ";" + address;
            }

            switch (tag.TagType)
            {
                case "bool":

                    OperateResult<bool> resbool = new OperateResult<bool>();

                    if (reg == "do")
                    {
                        resbool = _modbusDevice.ReadCoil(address);
                    }
                    else if (reg == "di")
                    {
                        resbool = _modbusDevice.ReadDiscrete(address);
                    }
                    if (resbool.IsSuccess)
                    {
                        tag.TagValue = resbool.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "short":
                case "int16":
                    OperateResult<short> res = _modbusDevice.ReadInt16(address);
                    if (res.IsSuccess)
                    {
                        tag.TagValue = res.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "int":
                case "int32":
                    OperateResult<int> resint = _modbusDevice.ReadInt32(address);
                    if (resint.IsSuccess)
                    {
                        tag.TagValue = resint.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "int64":
                case "long":
                    OperateResult<long> reslong = _modbusDevice.ReadInt64(address);
                    if (reslong.IsSuccess)
                    {
                        tag.TagValue = reslong.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "uint":
                case "uint32":
                    OperateResult<uint> resuint = _modbusDevice.ReadUInt32(address);
                    if (resuint.IsSuccess)
                    {
                        tag.TagValue = resuint.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "uint64":
                case "ulong":
                    OperateResult<ulong> resulong = _modbusDevice.ReadUInt64(address);
                    if (resulong.IsSuccess)
                    {
                        tag.TagValue = resulong.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "single":
                case "float":
                    OperateResult<float> resfloat = _modbusDevice.ReadFloat(address);
                    if (resfloat.IsSuccess)
                    {
                        tag.TagValue = resfloat.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "double":
                    OperateResult<double> resdouble = _modbusDevice.ReadDouble(address);
                    if (resdouble.IsSuccess)
                    {
                        tag.TagValue = resdouble.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                case "string":
                    OperateResult<string> resStr = _modbusDevice.ReadString(address.Split('.')[0], ushort.TryParse(address.Split('.')[1], out ushort len) ? (ushort)0 : len);
                    if (resStr.IsSuccess)
                    {
                        tag.TagValue = resStr.Content;
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.TagValue = null;
                        tag.Quality = Quality.Bad;
                    }
                    break;
                default:
                    break;
            }
        }


    }
}



