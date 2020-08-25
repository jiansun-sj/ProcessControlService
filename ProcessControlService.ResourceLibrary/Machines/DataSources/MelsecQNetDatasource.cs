using log4net;
using ProcessControlService.ResourceLibrary.Machines;
using ProcessControlService.ResourceLibrary.Machines.DataSources;
using ProcessControlService.ResourceLibrary.Machines.DataSources.Utils;
using System;
using System.Collections.Generic;
using System.Xml;
using YumpooDrive;
using YumpooDrive.Profinet.Melsec;

namespace CustomDatasource.DataSources
{
    /// <summary>
    /// Q 以太网
    /// </summary>
    public class MelsecQNetDatasource : DataSource
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MelsecQNetDatasource));
        private MelsecMcNet PLC;

        public MelsecQNetDatasource(string Name, Machine machine)
            : base(Name, machine)
        {
        }

        public override void Disconnect()
        {
            try
            {

                PLC.ConnectClose();
            }
            catch { }
        }

        protected override bool Connect()
        {
            try
            {
                var res = PLC.ConnectServer();
                if (res.IsSuccess)
                {
                    LOG.Info($"Connect to [{SourceName}] success.");
                }
                else
                {
                    LOG.Warn($"Connect to [{SourceName}] failed.");
                }
                return Connected;
            }
            catch (Exception ex)
            {
                LOG.Error($"Connect to [{SourceName}] failed. Message [{ex.Message}]");
                return false;
            }
        }

        protected override bool Connected
        {
            get
            {
                try
                {
                    return PLC.IsConnect;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

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
                            tag.Quality = Quality.Bad;
                        }
                    }
                    else if (tag.TagType == "string")
                    {
                        if (tag.Address.Contains("."))
                        {
                            try
                            {
                                string address = tag.Address.Split('.')[0];
                                ushort len = Convert.ToUInt16(tag.Address.Split('.')[1]);
                                var res = PLC.ReadString(tag.Address.Split('.')[0], len);
                                if (res.IsSuccess)
                                {
                                    tag.TagValue = res.Content.Replace("\0", "");
                                    tag.Quality = Quality.Good;
                                }
                                else
                                {
                                    tag.Quality = Quality.Bad;
                                }
                            }
                            catch (Exception)
                            {
                                LOG.Error($"Tag Address Error {tag.Address}");
                            }
                        }
                        else
                        {
                            LOG.Error($"Tag Address Error {tag.Address}");
                        }
                    }
                    else
                    {
                        ushort len = ConvertUtils.GetLength(tag);

                        var res = PLC.Read(tag.Address, len);
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
                OperateResult opres = new OperateResult();
                try
                {
                    if (value is bool)
                    {
                        opres = PLC.Write(tag.Address, (bool)value);
                    }
                    else
                    {
                        if (tag.TagType == "string")
                        {
                            if (tag.Address.Contains("."))
                            {
                                try
                                {
                                    string[] adds = tag.Address.Split('.');
                                    string address = adds[0];
                                    ushort len = Convert.ToUInt16(adds[1]);
                                    List<byte> values = new List<byte>();
                                    values.AddRange(ConvertUtils.GetBytes(tag, value));
                                    while (values.Count < len)
                                    {
                                        values.Add(0);
                                    }

                                    opres = PLC.Write(address, values.ToArray());
                                }
                                catch (Exception)
                                {
                                    LOG.Error($"Tag Address Error {tag.Address}");
                                }
                            }
                            else
                            {
                                LOG.Error($"Tag Address Error {tag.Address}");
                            }
                        }
                        else
                        {
                            opres = PLC.Write(tag.Address, ConvertUtils.GetBytes(tag, value));
                        }
                    }

                    LOG.Info($"数据源[{SourceName}]写入数据 Tag[{tag.TagName}] Address[{tag.Address}] Value[{value.ToString()}] IsSuccess[{opres.IsSuccess}]");

                }
                catch (Exception ex)
                {
                    LOG.Error($"数据源[{SourceName}]写入数据出错 Tag[{tag.TagName}] Address[{tag.Address}] Value[{value.ToString()}] Message[{ex.Message}]");
                }
                return opres.IsSuccess;
            }
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            XmlElement level1_item = (XmlElement)node;

            string IP = level1_item.GetAttribute("IP");
            string Port = level1_item.GetAttribute("Port");
            PLC = new MelsecMcNet();
            try
            {
                PLC.IpAddress = IP;
                PLC.Port = Convert.ToInt32(Port);
                PLC.ConnectTimeOut = 2000;
            }
            catch { }
            return base.LoadFromConfig(node);
        }
    }
}