using log4net;
using ProcessControlService.ResourceLibrary.Machines.DataSources.Utils;
using System;
using System.Text;
using System.Xml;
using YumpooDrive;
using YumpooDrive.Profinet.Toyopuc;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class ToyopucDataSource : DataSource
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ToyopucDataSource));
        private ToyopucNet PLC;
        public ToyopucDataSource(string Name, Machine machine)
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
                    LOG.Info($"Connect to ToyopucDataSource [{SourceName}] success.");
                }
                else
                {
                    LOG.Warn($"Connect to ToyopucDataSource [{SourceName}] failed.");
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
                    else if (tag.TagType == "string" || tag.Address.Contains("#"))
                    {

                        string address = tag.Address.Split('#')[0];
                        ushort len = Convert.ToUInt16(tag.Address.Split('#')[1]);
                        OperateResult<string> res = PLC.ReadString(tag.Address, len, Encoding.ASCII);
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

                    }
                    else
                    {

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
                    else
                    {
                        opres = PLC.Write(tag.Address, ConvertUtils.GetBytes(tag, value));
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
            PLC = new ToyopucNet
            {
                IpAddress = _IPAddress,

            };
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
