using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using log4net;
using ProcessControlService.ResourceLibrary.Machines.DataSources.Utils;
using YumpooDrive;
using YumpooDrive.Profinet.Melsec;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{

    /// <summary>
    /// FX串口
    /// </summary>
    public class MelsecFxSerialDatasource : DataSource
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MelsecFxSerialDatasource));
        private MelsecFxSerial PLC;

        public MelsecFxSerialDatasource(string Name, Machine machine)
            : base(Name, machine)
        {
        }

        public override void Disconnect()
        {
            try
            {
                PLC.Close();
            }
            catch { }
        }

        protected override bool Connect()
        {
            try
            {
                PLC.Open();
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
                    return PLC.IsOpen();
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

        private object BytesToObject(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                IFormatter formatter = new BinaryFormatter(); return formatter.Deserialize(ms);
            }
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            XmlElement level1_item = (XmlElement)node;

            string COM = level1_item.GetAttribute("COM");
            //string Ip = level1_item.GetAttribute("IP");
            //string port = level1_item.GetAttribute("Port");
            PLC = new MelsecFxSerial();
            try
            {
                PLC.SerialPortInni(sp =>
                {
                    sp.PortName = COM;
                    sp.StopBits = System.IO.Ports.StopBits.One;
                    sp.DataBits = 7;
                    sp.BaudRate = 9600;
                    sp.Parity = System.IO.Ports.Parity.Even;
                });
            }
            catch { }
            return base.LoadFromConfig(node);
        }
    }
}