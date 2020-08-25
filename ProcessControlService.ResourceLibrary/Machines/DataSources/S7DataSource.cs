

using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Machines.Drivers;
using S7.Net;
using System;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class S7DataSource : DataSource
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(S7DataSource));

        private S7NetPlcDriver _plc;
        public string Ip;

        public S7DataSource(string name, Machine machine)
            : base(name, machine)
        {
            //PLC = new S7NetPlcDriver(cputype, ip, rack, slot);
        }

        protected override bool Connected => _plc != null && _plc.IsConnected();

        public override bool LoadFromConfig(XmlNode node)
        {
            var level1Item = (XmlElement)node;

            // get ip address
            var cputype = (CpuType)Enum.Parse(typeof(CpuType), level1Item.GetAttribute("CpuType"));
            Ip = level1Item.GetAttribute("IPAddress");
            var rack = short.Parse(level1Item.GetAttribute("Rack"));
            var slot = short.Parse(level1Item.GetAttribute("Slot"));
            _plc = new S7NetPlcDriver(cputype, Ip, rack, slot);

            return base.LoadFromConfig(node);
        }

        public override void Disconnect()
        {
            //Connected = false; // comment by David 20170709

            _plc.Disconnect();
        }

        protected override bool Connect()
        {
            try
            {
                var connOk = _plc.Connect();
                //Thread.Sleep(200);
                if (connOk)
                    if (CheckConnected())
                    {
                        Log.Info($"连接到西门子设备{SourceName}成功.");
                        return true;
                    }
                Log.Info($"连接到西门子设备{SourceName}失败.");
                return false;
            }
            catch (Exception ex)
            {
                //LOG.Info(string.Format("连接到设备{0}失败.", SourceName));
                Log.Error($"西门子PLC连接失败：{ex.Message}");
                return false;
            }
        }

        // 检查通信连接 - David 20170709
        protected override bool CheckConnected()
        {
            Connected = _plc != null && _plc.IsConnected();
            return Connected;
        }

        public override bool UpdateAllValue()
        {
            lock (this) // david 20170617
            {
                try
                {
                    //Log.Info("S7DataSource开始读取" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
                    // read from device
                    _plc.UpdateDbBlock();

                    // update from buffer to Tags
                    foreach (var tag in Tags.Values)
                        if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                            try
                            {
                                tag.TagValue = _plc.ReadItem(tag.Address);
                                tag.Quality = Quality.Good;
                                // LOG.Info("Tag名:" + tag.TagName + "值:" + tag.TagValue);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(
                                    $"数据源[{SourceName}]读取数据出错 Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");
                                tag.Quality = Quality.Bad;
                            }
                    //Log.Info("S7DataSource读取结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
                    return true;
                }
                catch (PlcException plcException)
                {
                    var plcExceptionErrorCode = plcException.ErrorCode;
                    Log.Error($"出现PLC异常，DataSource：[{SourceName}]，ErrorCode:[{plcException.ErrorCode}]");
                    Disconnect();
                    return false;
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                    Disconnect();
                    return false;
                }
            }
        }

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            if (Owner.Mode != RedundancyMode.Master)
            {
                Log.Error($"当前冗余模式不是主机，无法向PLC中写入数值");
                return false;//sunjian 2019-12-16没有向plc中写入数值。
            }

            lock (this) // david 20170617
            {
                /*try
                {*/

                if (_plc.WriteItems(tag, value))
                {
                    Log.Debug($"*********往Machine: [{SourceName}]的Tag [{tag.TagName}] 写入值：[{value}]*********");
                    return true;
                }

                Log.Error($"*********往Machine: [{SourceName}]的Tag: [{tag.TagName}] 写入值：[{value}] 出错*********");
                return false;

                /*}
                catch (Message ex)
                {
                    LOG.Error(string.Format("S7写入PLC出错：{0}", ex));
                }*/
            }
        }

        public override object ReadTag(Tag tag)
        {
            return _plc.ReadItem(tag.Address);
        }

        public override void AddItem(Tag tag)
        {
            _plc.AddAddress(tag);

            base.AddItem(tag);
        }

        public void PlcAddAddress(Tag tag)
        {
            _plc.AddAddress(tag);
        }
    }
}