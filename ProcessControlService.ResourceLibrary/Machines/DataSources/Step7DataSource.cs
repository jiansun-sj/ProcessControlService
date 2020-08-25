using ProcessControlService.ResourceLibrary.Machines.DataSources.Utils;
using System;
using System.Globalization;
using System.Text;
using System.Xml;
using YumpooDrive;
using YumpooDrive.Profinet.Siemens;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    /// <summary>
    /// add by zsy
    /// 新的西门子plc驱动，不使用S7.NET
    /// 配置参考backup/Step7Test.xml
    /// </summary>
    public class Step7DataSource : DataSource
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(Step7DataSource));
        private SiemensS7Net PLC;

        public Step7DataSource(string Name, Machine machine)
            : base(Name, machine)
        {
        }
        public override bool LoadFromConfig(XmlNode node)
        {
            var level1_item = (XmlElement)node;

            var cputype = (SiemensPLCS)Enum.Parse(typeof(SiemensPLCS), level1_item.GetAttribute("CpuType"));
            var ip = level1_item.GetAttribute("IPAddress");
            //short rack = short.Parse(level1_item.GetAttribute("Rack"));
            //  short slot = short.Parse(level1_item.GetAttribute("Slot"));
            PLC = new SiemensS7Net(cputype, ip)
            {
                ConnectTimeOut = 1000
            };
            // PLC.Rack = Convert.ToByte(rack);
            // PLC.Slot = Convert.ToByte(slot);

            return base.LoadFromConfig(node);
        }
        public override void Disconnect()
        {
            PLC?.ConnectClose();
        }

        protected override bool Connect()
        {
            try
            {
                var opres = PLC.ConnectServer();
                if (opres.IsSuccess)
                {
                    LOG.Info($"Connect to Step7DataSource [{SourceName}] success.");
                }
                else
                {
                    LOG.Warn($"Connect to Step7DataSource [{SourceName}] failed.");
                }
                return opres.IsSuccess;
            }
            catch (Exception ex)
            {
                LOG.Error($"Connect to Step7DataSource [{SourceName}] failed. Message [{ex.Message}]");
                return false;
            }

        }

        protected override bool Connected => (PLC != null && PLC.IsConnect);

        public override bool UpdateAllValue()
        {
            lock (this)
            {
                try
                {
                    //LOG.Info("HSLDataSource开始读取"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
                    foreach (Tag tag in Tags.Values)
                    {
                        if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                        {
                            try
                            {
                                ReadTag(tag);
                            }
                            catch (Exception ex)
                            {
                                LOG.Error($"Datasource[{SourceName}] read error. Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");
                                tag.Quality = Quality.Bad;
                            }
                        }
                    }
                    //LOG.Info("HSLDataSource读取结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
                    return true;
                }
                catch (Exception)
                {
                    Disconnect();
                    return false;
                }

            }
        }

        public override object ReadTag(Tag tag)
        {
            //LOG.Info("HSLTag开始读取" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
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
                        OperateResult<string> res = new OperateResult<string>();
                        if (tag.Address.Contains("#"))
                        {
                            string[] adds = tag.Address.Split('#');
                            string address = adds[0];
                            ushort len = Convert.ToUInt16(adds[1]);
                            res = PLC.ReadString(adds[0], len);
                        }
                        else
                        {
                            res = PLC.ReadString(tag.Address, 1);
                        }
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
                    else if (tag.TagType == "datetime") //西门子Date_And_Time
                    {
                        OperateResult<byte[]> res = PLC.Read(tag.Address, 8);
                        if (res.IsSuccess && res.Content.Length == 8)
                        {
                            var time = GetDateTime(res.Content, out bool isSuccess);
                            if (isSuccess)
                            {
                                tag.TagValue = time;
                                tag.Quality = Quality.Good;
                            }
                        }
                        else
                        {
                            tag.Quality = Quality.Bad;
                        }
                    }
                    else
                    {
                        var len = ConvertUtils.GetLength(tag);
                        OperateResult<byte[]> res = PLC.Read(tag.Address, len);
                        ConvertUtils.DecodeTagValue(tag, res, true);
                    }
                    return tag.TagValue;
                }
                catch (Exception ex)
                {
                    LOG.Error($"Datasource[{SourceName}] read error. Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");
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
                    if (value is bool boolean)
                    {
                        opres = PLC.Write(tag.Address, boolean);
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
                        PLC.Write(tag.Address, ConvertUtils.GetBytes(tag, value));
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

        private DateTime GetDateTime(byte[] datas, out bool IsSuccess)
        {
            var dyear = ConvertBCDToInt(datas[0]);
            var year = dyear >= 89 ? $"19{dyear:D2}" : $"20{dyear:D2}";
            var month = ConvertBCDToInt(datas[1]);
            var day = ConvertBCDToInt(datas[2]);
            var hour = ConvertBCDToInt(datas[3]);
            var min = ConvertBCDToInt(datas[4]);
            var sec = ConvertBCDToInt(datas[5]);
            var lmsec = ConvertBCDToInt(datas[6]);
            var fmsec = ConvertBCDToInt(datas[7]).ToString("D2")[0];
            var msec = $"{fmsec}{lmsec:D2}";
            var strTime = $"{year}-{month:D2}-{day:D2} {hour:D2}:{min:D2}:{sec:D2} {msec}";
            try
            {
                var time = DateTime.ParseExact(strTime, "yyyy-MM-dd HH:mm:ss fff", CultureInfo.CurrentCulture);
                IsSuccess = true;
                return time;
            }
            catch (Exception)
            {
                // LOG.Error($"时间转换失败：{strTime}");
                IsSuccess = false;
                return new DateTime();
            }
        }

        private byte ConvertBCDToInt(byte b)
        {
            //高四位  
            byte b1 = (byte)((b >> 4) & 0xF);
            //低四位  
            byte b2 = (byte)(b & 0xF);

            return (byte)(b1 * 10 + b2);
        }
    }


}
