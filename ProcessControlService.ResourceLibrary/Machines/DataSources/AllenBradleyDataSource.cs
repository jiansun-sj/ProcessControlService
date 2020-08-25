using log4net;
using ProcessControlService.ResourceLibrary.Machines.DataSources.Utils;
using System;
using System.Text;
using System.Xml;
using YumpooDrive;
using YumpooDrive.Profinet.AllenBradley;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class AllenBradleyDataSource : DataSource
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(AllenBradleyDataSource));
        private AllenBradleyNetNew PLC;
        public AllenBradleyDataSource(string Name, Machine machine)
            : base(Name, machine)
        {
        }

        public override void Disconnect()
        {
            PLC?.ConnectClose();
        }

        protected override bool Connect()
        {
            try
            {
                // PLC.ConnectTimeOut = 1000;
                OperateResult opres = PLC.ConnectServer();
                if (opres.IsSuccess)
                {
                    LOG.Info($"Connect to [{SourceName}] success.");
                }
                else
                {
                    LOG.Warn($"Connect to [{SourceName}] failed.");
                }
                return opres.IsSuccess;
            }
            catch (Exception ex)
            {
                LOG.Error($"Connect to [{SourceName}] failed. Message [{ex.Message}]");
                return false;
            }

        }

        protected override bool Connected => PLC != null && PLC.IsConnect;

        public override bool UpdateAllValue()
        {
            try
            {
                foreach (Tag tag in Tags.Values)
                {
                    ReadTag(tag);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override object ReadTag(Tag tag)
        {
            if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
            {
                try
                {
                    if (tag.TagType == "bool")
                    {
                        var res = PLC.ReadBool(tag.Address);
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
                    }
                    else if (tag.TagType == "string")
                    {
                        //if (tag.Address.Contains("#"))
                        //{
                        // string address = tag.Address.Split('#')[0];
                        // ushort len = Convert.ToUInt16(tag.Address.Split('#')[1]);
                        OperateResult<string> res = PLC.ReadString(tag.Address);
                        if (res.IsSuccess)
                        {
                            string strval = res.Content;//.Length >= len ? res.Content.Substring(0, len) : res.Content;
                            tag.TagValue = strval.Replace("\0", "");
                            tag.Quality = Quality.Good;
                        }
                        else
                        {
                            tag.TagValue = null;
                            tag.Quality = Quality.Bad;
                        }
                        //}
                        //else
                        //{
                        //    throw new Message("Invalid tag address");
                        //}
                    }
                    else
                    {
                        //  2019年11月11日 11:41:12  夏  读取int由传入长度改为常量1
                        // ushort len = utils.GetLength(tag);
                        OperateResult<byte[]> res = PLC.Read(tag.Address, 1);
                        ConvertUtils.DecodeTagValue(tag, res);
                    }
                    return tag.TagValue;
                }
                catch (Exception ex)
                {
                    LOG.Error($"Datasource[{SourceName}] read error. Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");
                    tag.TagValue = null;
                    tag.Quality = Quality.Bad;
                    return tag.TagValue;
                }
            }
            else
            {
                return null;
            }
        }


        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            lock (this)
            {
                try
                {
                    OperateResult opres = new OperateResult();
                    if (value is bool)
                    {
                        opres = PLC.Write(tag.Address, (bool)value);
                    }
                    else if (tag.TagType == "string")
                    {
                        if (tag.Address.Contains("#"))
                        {
                            string[] adds = tag.Address.Split('#');
                            string address = adds[0];
                            ushort len = Convert.ToUInt16(adds[1]);

                            string val = value.ToString();

                            if (val.Length > len)
                            {
                                val = val.Substring(0, len);
                            }

                            StringBuilder sb = new StringBuilder(val);
                            while (sb.Length < len)
                            {
                                sb.Append("\0");
                            }
                            opres = PLC.Write(address, sb.ToString());
                        }
                        else
                        {
                            opres = PLC.Write(tag.Address, value.ToString());
                        }
                    }
                    else if (tag.TagType == "int16")
                    {
                        opres = PLC.Write(tag.Address, new[] { Convert.ToInt16(value) });//sunjian 2019/12/19 若前面步骤传递值为int32，（short）int32强转会失败。全部使用Convert函数转换
                    }
                    else if (tag.TagType == "int32")
                    {
                        opres = PLC.Write(tag.Address, new[] { Convert.ToInt32(value) });
                    }
                    else if (tag.TagType == "int64")
                    {
                        opres = PLC.Write(tag.Address, new[] { Convert.ToInt64(value) });
                    }
                    else if (tag.TagType == "float")
                    {
                        opres = PLC.Write(tag.Address, new[] { Convert.ToSingle(value) });
                    }
                    else if (tag.TagType == "double")
                    {
                        opres = PLC.Write(tag.Address, new[] { Convert.ToDouble(value) });
                    }
                    LOG.Info($"DataSource[{SourceName}] write tag. Tag[{tag.TagName}] Address[{tag.Address}] Value[{value.ToString()}] IsSuccess[{opres.IsSuccess}]");
                    return true;

                }
                catch (Exception ex)
                {
                    LOG.Error($"DataSource[{SourceName}] write tag error. Tag[{tag.TagName}] Address[{tag.Address}] Value[{value.ToString()}] Message[{ex.Message}]");
                    return false;
                }
            }
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            var xmlElement = (XmlElement)node;
            string _IPAddress = xmlElement.GetAttribute("IPAddress");
            PLC = new AllenBradleyNetNew
            {
                IpAddress = _IPAddress,

            };

            if (xmlElement.HasAttribute("Slot"))
            {
                PLC.Slot = byte.TryParse(xmlElement.GetAttribute("Slot"), out byte slot) ? slot : (byte)0;
            }
            if (xmlElement.HasAttribute("Port"))
            {
                PLC.Port = int.TryParse(xmlElement.GetAttribute("Port"), out int port) ? port : 44818;
            }
            else
            {
                PLC.Port = 44818;
            }

            return base.LoadFromConfig(xmlElement);
        }

    }


}
