using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using YumpooDrive;
using YumpooDrive.Profinet.AllenBradley;

namespace ProcessControlService.ResourceLibrary.Machines.Drivers
{
    //在PLC中TEST3是INT32的数组，TEST.TPS是INT16的数组，TEST4是STRING14的数组
    //<Tag Name="Test" Address="TEST3[0]" TagType="int32" AccessType="Read"/>
    //<Tag Name="Test" Address="TEST3[2].0" TagType="bool" AccessType="Read"/>
    //<Tag Name="Test" Address="TEST.TPS[0]" TagType="int16" AccessType="Read"/>
    //<Tag Name="Test" Address="TEST.TPS[5]" TagType="int16" AccessType="Read"/>
    //<Tag Name="Test" Address="TEST4[2],L14" TagType="string" AccessType="Read"/>

    public class ABLogixDriver
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ABLogixDriver));
        private AllenBradleyNetNew allenBradleyNet = null;
        private bool Connected = false;
        string IP = "192.168.11.244";
        string Port = "44818";
        string Slot = "0";
        public ABLogixDriver(string IP, string Port, string Slot)
        {
            this.IP = IP;
            this.Port = Port;
            this.Slot = Slot;
        }
        public bool IsConnected()
        {
            return (null != allenBradleyNet && Connected);
        }
        public bool Connect()
        {
            Connected = false;
            try
            {
                allenBradleyNet = new AllenBradleyNetNew(IP);
                // 连接
                if (!System.Net.IPAddress.TryParse(IP, out System.Net.IPAddress address))
                {
                    LOG.Error("Ip地址输入不正确！");
                }
                if (!int.TryParse(Port, out int port))
                {
                    LOG.Error("端口号输入不正确！");
                }
                if (!byte.TryParse(Slot, out byte slot))
                {
                    LOG.Error("slot输入不正确！");
                }
                allenBradleyNet.IpAddress = IP;
                allenBradleyNet.Port = port;
                allenBradleyNet.Slot = slot;


                OperateResult connect = allenBradleyNet.ConnectServer();
                if (connect.IsSuccess)
                {
                    LOG.Info("连接成功！");
                    Connected = true;
                }
                else
                {
                    LOG.Error("连接失败！" + connect.ToMessageShowString());
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message);
            }
            return Connected;
        }
        private bool ReConnect()
        {
            //int RetryTime = 0;
            while (true)
            {
                Disconnect();//先断开连接
                if (Connect())
                {//连接成功
                    return true;
                }
                //RetryTime++;
                Thread.Sleep(100);
            }
            return false;
        }
        public void Disconnect()
        {
            Connected = false;
            allenBradleyNet.ConnectClose();
        }


        private Dictionary<string, Int16> ReadAreaMaxLen = new Dictionary<string, Int16>();//key是数组名，value是读取长度,如果索引是10，那么最大长度是11
        private Dictionary<string, Int32> ReceiveByteMaxLen = new Dictionary<string, Int32>();//理应收到的报文的最大长度
        private Dictionary<string, ABLogixPLCAddress> _dbAddress = new Dictionary<string, ABLogixPLCAddress>();
        private Dictionary<string, byte[]> _dbAreaBuf = new Dictionary<string, byte[]>(); //前一个数组名,后一个缓存区
        public void AddAddress(Tag tag)
        {
            try
            {
                if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                {
                    string address = tag.Address;
                    ABLogixPLCAddress addr = ABLogixPLCAddress.ConvertAddress(address, tag.TagType);
                    _dbAddress.Add(address, addr);

                    if (addr != null)
                    {
                        Int16 len = (Int16)(addr.index + 1);
                        if (ReadAreaMaxLen.ContainsKey(addr.ArrayName))
                        { //如果存在该DB ID
                            if (ReadAreaMaxLen[addr.ArrayName] < len)
                            {//如果是最大地址
                                ReadAreaMaxLen[addr.ArrayName] = len;
                                if (addr.Type == "int32" )
                                {
                                    ReceiveByteMaxLen[addr.ArrayName]=(len * 4);
                                }
                                else if (addr.Type == "byte")
                                {
                                    ReceiveByteMaxLen[addr.ArrayName] = (len);
                                }
                                else if (addr.Type == "int16" || addr.Type == "bool")
                                {
                                    ReceiveByteMaxLen[addr.ArrayName] = (len*2);
                                }
                                else if (addr.Type == "string")
                                {
                                    int tempstrlen = addr.StrLen + 4;
                                    if (tempstrlen % 4 == 0)
                                    {
                                    }
                                    else
                                    {
                                        tempstrlen = ((tempstrlen + 4) / 4) * 4;
                                    }
                                    ReceiveByteMaxLen[addr.ArrayName]= (len * tempstrlen + 2);
                                }
                                else
                                {
                                    throw new Exception("Tag类型不识别.");
                                }
                            }
                        }
                        else
                        {
                            ReadAreaMaxLen.Add(addr.ArrayName, len);
                            if (addr.Type == "int32" )
                            {
                                ReceiveByteMaxLen.Add(addr.ArrayName, (len * 4));
                            }
                            else if (addr.Type == "byte")
                            {
                                ReceiveByteMaxLen.Add(addr.ArrayName, (len));
                            }
                            else if (addr.Type == "int16" || addr.Type == "bool")
                            {
                                ReceiveByteMaxLen.Add(addr.ArrayName, (len * 2));
                            }
                            else if (addr.Type == "string")
                            {
                                int tempstrlen = addr.StrLen + 4;
                                if (tempstrlen % 4 == 0)
                                {
                                }
                                else
                                {
                                    tempstrlen = ((tempstrlen + 4) / 4) * 4;
                                }
                                ReceiveByteMaxLen.Add(addr.ArrayName, (len * tempstrlen + 2));
                            }
                            else
                            {
                                throw new Exception("Tag类型不识别.");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("ABLogixPLCAddress.ConvertAddress后的addr为null");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("AB驱动添加地址{0}出错{1}", tag.Address, ex));
            }
        }
        public object ReadItem(string address)
        {
            //lock (this)
            //{
                if (_dbAddress.ContainsKey(address))
                {
                    ABLogixPLCAddress addr = _dbAddress[address];

                    if (addr.Type == "bool")
                    {
                        byte[] _buf = _dbAreaBuf[addr.ArrayName];
                        if (0<= addr.BoolOffset&& addr.BoolOffset < 8)
                        {
                            bool result = ((_buf[addr.index * 2] & (1 << addr.BoolOffset)) != 0);
                            return result;
                        }
                        else if (8 <= addr.BoolOffset && addr.BoolOffset < 16)
                        {
                            bool result = ((_buf[addr.index * 2+1] & (1 << (addr.BoolOffset-8))) != 0);
                            return result;
                        }
                        //else if (16 <= addr.BoolOffset && addr.BoolOffset < 24)
                        //{
                        //    bool result = ((_buf[addr.index * 4+2] & (1 << (addr.BoolOffset-16))) != 0);
                        //    return result;
                        //}
                        //else if (24 <= addr.BoolOffset && addr.BoolOffset < 32)
                        //{
                        //    bool result = ((_buf[addr.index * 4+3] & (1 << (addr.BoolOffset-24))) != 0);
                        //    return result;
                        //}
                        else
                        {
                            throw new Exception("addr.BoolOffset大于15");
                        }
                    }
                    else if (addr.Type == "byte")
                    {
                        byte[] _buf = _dbAreaBuf[addr.ArrayName];
                        byte result = _buf[addr.index];
                        return result;
                    }
                    else if (addr.Type == "int16")
                    {
                        byte[] _buf = _dbAreaBuf[addr.ArrayName];
                        byte _low_byte = _buf[addr.index*2];
                        byte _high_byte = _buf[addr.index*2 + 1];

                        Int16 result = (Int16)((_high_byte << 8) + _low_byte);
                        return result;
                    }
                    else if (addr.Type == "int32")
                    {
                        int begin = addr.index * 4;
                        byte[] _buf = _dbAreaBuf[addr.ArrayName];

                        Int32 result = (Int32)((_buf[begin + 3] << 24) + (_buf[begin + 2] << 16) + (_buf[begin + 1] << 8) + _buf[begin]);
                        return result;
                    }
                    else if (addr.Type == "string")
                    {
                        int tempstrlen = addr.StrLen + 4;
                        if (tempstrlen % 4 == 0)
                        {
                        }
                        else
                        {
                            tempstrlen = ((tempstrlen + 4) / 4) * 4;
                        }
                        int begin = addr.index * tempstrlen + 2;//String的字节数组,从索引2开始
                        byte[] _buf = _dbAreaBuf[addr.ArrayName];
                        //int strlen= (Int32)((_buf[begin] << 24) + (_buf[begin+1] << 16) + (_buf[begin+2] << 8) + _buf[begin+3]);
                        int strlen = (_buf[begin + 3] << 24) + (_buf[begin + 2] << 16) + (_buf[begin + 1] << 8) + _buf[begin];
                        string result = System.Text.Encoding.ASCII.GetString(_buf, begin+4, strlen).Replace("\0","");  //AB字符串前四个字节是实际长度
                        return result;//= System.Text.RegularExpressions.Regex.Replace(result, @"[\u0001-\u001F]", ""); //过滤不可显示字符
                    }
                    else
                    {
                        throw new Exception("Tag类型不支持");
                    }
                }
                else
                {
                    throw new Exception("读取Tag集合中未找到该地址");
                }
                return null;
            //}
        }
        public bool UpdateBlock()
        {
            //lock (this)
            //{
            OperateResult<byte[]> read = null;
            try
            {
                if (null != ReadAreaMaxLen && ReadAreaMaxLen.Count > 0)
                {
                        
                    foreach (var item in ReadAreaMaxLen)
                    {
                        //Stopwatch sw = new Stopwatch();

                        //sw.Start();
                           
                        read = allenBradleyNet.Read(item.Key, (ushort)item.Value);
                        if (null==read)
                        {
                            LOG.Error("读取失败:返回的报文体为空!");
                            return false;
                        }

                        //sunjian   2019/11/14   防止xml配置有误，read.Content为空，导致UpdateBlock返回值为false，造成ABLogixDataSource UpdateAllValue更新所有数据失败，machine所有其他值都拿不到。
                        if (read.Content is null)
                        {
                            LOG.Error($"{allenBradleyNet} {item.Key} 数据读取失败，返回报文体内容为空！");
                            continue;
                        }

                        if (ReceiveByteMaxLen[item.Key] != read.Content.Length)
                        {
                            LOG.Error("读取失败:返回的报文体byte数组长度异常!");
                            return false;
                        }

                        if (read.IsSuccess)
                        {
                            _dbAreaBuf[item.Key] = read.Content;
                        }
                        else
                        {
                            LOG.Error("读取失败：" + read.ToMessageShowString());
                            return false;
                        }

                        //sw.Break();
                        //LOG.Info("读取Tag点时间是："+sw.ElapsedMilliseconds);
                    }
                    return true;
                }

                LOG.Error("读取失败:读取集合为空!");
                return false;
            }
            catch (Exception ex)
            {
                LOG.Error("读取失败：" + ex.Message + ex.StackTrace);
                return false;
            }
            // }
        }
    }

    public class ABLogixPLCAddress
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ABLogixPLCAddress));

        public string CompleteAddress;
        public string Type;
        public string ArrayName;
        public Int16 index;
        public Int16 BoolOffset;
        public Int16 StrLen;

        public static ABLogixPLCAddress ConvertAddress(string CompleteAddress, string Type)
        {
            try
            {
                string[] AddressSection = CompleteAddress.Trim().Split('[',']');

                string ArrayName = "";
                Int16 index = 0;
                Int16 BoolOffset = 0;//只有bool会用到
                Int16 StrLen = 0;//只有String会用到

                if (AddressSection.Length == 2)
                {
                    ArrayName = AddressSection[0];
                    index = Convert.ToInt16(AddressSection[1]);
                    if (Type.ToLower() == "bool")
                    {
                        throw new Exception("不支持bool数组");
                    }
                    else if(Type.ToLower() == "int16")
                    {
                        int maxindex = 480 / 2;//480是报文最大长度
                        if (index > (maxindex - 1))
                        {
                            throw new Exception("INT16最多只能读" + maxindex + "个长度");
                        }
                    }
                    else if (Type.ToLower() == "int32")
                    {
                        int maxindex = 480 / 4;
                        if (index > (maxindex - 1))
                        {
                            throw new Exception("INT32最多只能读" + maxindex + "个长度");
                        }
                    }
                    else if (Type.ToLower() == "byte")
                    {
                        int maxindex = 480 / 1;
                        if (index > (maxindex - 1))
                        {
                            throw new Exception("Byte最多只能读" + maxindex + "个长度");
                        }
                    }
                    else
                    {
                        throw new Exception("Tag地址与类型不匹配");
                    }
                }
                else if (AddressSection.Length == 3)
                {
                    if (AddressSection[2]=="")
                    {
                        ArrayName = AddressSection[0];
                        index = Convert.ToInt16(AddressSection[1]);
                        if (Type.ToLower() == "bool")
                        {
                            throw new Exception("不支持bool数组");
                        }
                        else if(Type.ToLower() == "int16")
                        {
                            int maxindex = 480 / 2;//480是报文最大长度
                            if (index > (maxindex - 1))
                            {
                                throw new Exception("INT16最多只能读" + maxindex + "个长度");
                            }
                        }
                        else if (Type.ToLower() == "int32")
                        {
                            int maxindex = 480 / 4;
                            if (index > (maxindex - 1))
                            {
                                throw new Exception("INT32最多只能读" + maxindex + "个长度");
                            }
                        }
                        else if(Type.ToLower() == "byte" )
                        {
                            int maxindex = 480 / 1;
                            if (index > (maxindex - 1))
                            {
                                throw new Exception("Byte最多只能读" + maxindex + "个长度");
                            }
                        }
                        else
                        {
                            throw new Exception("Tag地址与类型不匹配");
                        }
                    }
                    else
                    {
                        ArrayName = AddressSection[0];
                        index = Convert.ToInt16(AddressSection[1]);
                        if (AddressSection[2].Contains("."))
                        {
                            BoolOffset = Convert.ToInt16(AddressSection[2].Substring(1));
                            if (Type.ToLower() == "bool")
                            {
                                int maxindex = 480 / 2;
                                if (index > (maxindex - 1))
                                {
                                    throw new Exception("INT16最多只能读" + maxindex + "个长度");
                                }
                                if (BoolOffset>15)
                                {
                                    throw new Exception("Bool偏移量最大为15");
                                }
                            }
                            else
                            {
                                throw new Exception("Tag地址与类型不匹配");
                            }
                        }
                        else if (AddressSection[2].Contains(",L"))
                        {
                            StrLen = Convert.ToInt16(AddressSection[2].Substring(2));
                            if (Type.ToLower() == "string")
                            {
                                if (StrLen == 0)
                                {
                                    throw new Exception("定义的String类型长度不能为0");
                                }
                                int tempstrlen = StrLen + 4;
                                if (tempstrlen % 4 == 0)
                                {
                                }
                                else
                                {
                                    tempstrlen = ((tempstrlen + 4) / 4) * 4;
                                }
                                int maxindex = 480 / tempstrlen;

                                if (index > (maxindex-1))
                                {
                                    throw new Exception("String"+ StrLen + "最多只能读"+ maxindex + "个长度");
                                }
                            }
                            else
                            {
                                throw new Exception("Tag地址与类型不匹配");
                            }
                        }
                        else
                        {
                            throw new Exception("Tag地址分割元素数大于2但是不包含'.'或',L'");
                        }
                    }
                }
                else
                {
                    throw new Exception("Tag地址分割元素数既不为2也不为3");
                }
                return new ABLogixPLCAddress()
                {
                    CompleteAddress = CompleteAddress,
                    Type = Type.ToLower(),
                    ArrayName = ArrayName,
                    index = index,
                    BoolOffset = BoolOffset,
                    StrLen = StrLen
                };
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("Tag {0}地址解析出错:{1}", CompleteAddress, ex));
                return null;
            }
        }
    }
}


