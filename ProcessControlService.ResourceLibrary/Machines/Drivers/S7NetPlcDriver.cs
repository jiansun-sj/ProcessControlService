// ==================================================
// 文件名：S7NetPlcDriver.cs
// 创建时间：2020/06/11 21:05
// ==================================================
// 最后修改于：2020/06/11 21:05
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using log4net;
using S7.Net;
using S7.Net.Types;
using Double = S7.Net.Types.Double;

namespace ProcessControlService.ResourceLibrary.Machines.Drivers
{
    internal class S7NetPlcDriver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(S7NetPlcDriver));

        #region Private fields

        //public List<Tag> itemlist = new List<Tag>();

        private readonly Plc _plc;

        #endregion

        #region Constructor

        /// <summary>
        ///     初始化CpuType.S71200,"192.168.11.11",0,0
        /// </summary>
        /// <param name="cpu"></param>
        /// <param name="ip"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        public S7NetPlcDriver(CpuType cpu, string ip, short rack, short slot)
        {
            _plc = new Plc(cpu, ip,102, rack, slot);
        }

        #endregion

        //private ConnectionStates _connectionState;
        //public ConnectionStates ConnectionState
        //{
        //    get { return _connectionState; }
        //    private set { _connectionState = value; }
        //}

        public bool IsConnected()
        {
            return _plc != null && /*_plc.IsAvailable &&*/ _plc.IsConnected;
        }

        public bool Connect()
        {
            //ConnectionState = ConnectionStates.Connecting;
            _plc.Open();
            //var error = _plc.Open();
            if (_plc.IsConnected) return true;
            //ConnectionState = ConnectionStates.Offline;

            Log.Error($"PLC:[{_plc.IP}连接失败]");
            return false;

            //ConnectionState = ConnectionStates.Online;
        }

        public void Disconnect()
        {
            //ConnectionState = ConnectionStates.Offline;
            _plc.Close();
        }

        public object ReadItem(string address)
        {
            lock (this)
            {
                try
                {
                    if (DbAddress.ContainsKey(address))
                    {
                        var addr = DbAddress[address];
                        var db_id = addr.DBBlockID;

                        var offset = (short) (addr.Offset - _dbStartAddress[db_id]);

                        if (addr.Type == "bool")
                        {
                            var _buf = _dbAreaBuf[db_id];
                            var _result_byte = _buf[offset];

                            var result = (_result_byte & (1 << addr.BitOffset)) != 0;
                            return result;
                        }

                        if (addr.Type == "int16")
                        {
                            var _buf = _dbAreaBuf[db_id];
                            var _low_byte = _buf[offset];
                            var _high_byte = _buf[offset + 1];

                            //Int16 result = (Int16)(_high_byte << 8 + _low_byte);
                            var result = (short) ((_low_byte << 8) + _high_byte);
                            return result;
                        }

                        if (addr.Type == "int32")
                        {
                            var _buf = _dbAreaBuf[db_id];
                            var _byte1 = _buf[offset];
                            var _byte2 = _buf[offset + 1];
                            var _byte3 = _buf[offset + 2];
                            var _byte4 = _buf[offset + 3];

                            //Int32 result = (Int32)(_byte4 << 24 + _byte3 << 16 + _byte2 << 8 + _byte1);
                            var result = (_byte1 << 24) + (_byte2 << 16) + (_byte3 << 8) + _byte4;
                            return result;
                        }

                        if (addr.Type == "string")
                        {
                            var _buf = _dbAreaBuf[db_id];
                            var result =
                                Encoding.ASCII.GetString(_buf, offset + 2,
                                    _buf[offset + 1]); //西门子字符串前两位要空出来,读取长度根据第2个字节
                            return result;
                        }

                        if (addr.Type == "ByteArray") //2020/6/11 ByteArray
                        {
                            var _buf = _dbAreaBuf[db_id];
                            var result = Encoding.ASCII.GetString(_buf, offset, addr.BitOffset);
                            return result;
                        }

                        throw new Exception("Tag类型不支持");
                    }

                    if (_mAddress.ContainsKey(address))
                    {
                        var addr = _mAddress[address];
                        var offset = (short) (addr.Offset - _mAreaMinAddress);
                        var _buf = _mAreaBuf;

                        if (addr.Type == "bool")
                        {
                            var _result_byte = _buf[offset];

                            var result = (_result_byte & (1 << addr.BitOffset)) != 0;
                            return result;
                        }

                        if (addr.Type == "int16")
                        {
                            var _low_byte = _buf[offset];
                            var _high_byte = _buf[offset + 1];

                            var result = (short) ((_low_byte << 8) + _high_byte);
                            return result;
                        }

                        if (addr.Type == "int32")
                        {
                            var _byte1 = _buf[offset];
                            var _byte2 = _buf[offset + 1];
                            var _byte3 = _buf[offset + 2];
                            var _byte4 = _buf[offset + 3];

                            //Int32 result = (Int32)(_byte4 << 24 + _byte3 << 16 + _byte2 << 8 + _byte1);
                            var result = (_byte1 << 24) + (_byte2 << 16) + (_byte3 << 8) + _byte4;
                            return result;
                        }
                    }
                    else
                    {
                        throw new Exception("_dbAddress或_MAddress不包含该地址!");
                    }
                }
                catch (Exception ex)
                {
                    //LOG.Error(string.Format("地址{0}读取错:{1}", address, ex));
                    Log.Error($"Address:{address} readed error,Message:{ex},IP地址时{_plc.IP}");
                }

                return null;
            }
        }

        private bool ReConnect()
        {
            //int RetryTime = 0;
            while (true)
            {
                Disconnect(); //先断开连接
                if (Connect())
                    //连接成功
                    return true;
                //RetryTime++;
                Thread.Sleep(100);
            }

            return false;
        }


        public bool WriteItems(Tag tag, object value)
        {
            lock (this)
            {
                try
                {
                    if (!IsConnected()) return false;

                    if (tag.TagType == "string") //在创建TagType时已经转为小写
                    {
                        var pLcAddress = DbAddress[tag.Address];
                        var strValue = value.ToString();
                        var strlen = (byte) strValue.Length;
                        var valueByte = new byte[strlen + 1];
                        var strbyte = Encoding.ASCII.GetBytes(strValue);

                        if (strbyte.Length != strlen)
                            throw new InvalidOperationException("写入" + tag.Address + "出错,解析的字符串长度与GetBytes数组长度不一致!");

                        if (strlen > pLcAddress.BitOffset)
                            throw new InvalidOperationException("写入" + tag.Address + "出错,输入的字符串的长度过长!");

                        valueByte[0] = strlen;
                        strbyte.CopyTo(valueByte, 1);

                         _plc.Write(DataType.DataBlock, pLcAddress.DBBlockID, pLcAddress.Offset + 1,
                            valueByte);

                        /*if (result != ErrorCode.NoError)
                            throw new InvalidOperationException(result + "\n" + "Tag: " + tag.Address);*/

                        return true;
                    }
                    //2020/6/11 ByteArray

                    if (tag.TagType == "bytearray") //在创建TagType时已经转为小写
                    {
                        var pLcAddress = DbAddress[tag.Address];
                        var strValue = value.ToString();
                        var strlen = strValue.Length;
                        var valueByte = new byte[strlen];
                        var strbyte = Encoding.ASCII.GetBytes(strValue);

                        if (strbyte.Length != strlen)
                            throw new InvalidOperationException("写入" + tag.Address + "出错,解析的字符串长度与GetBytes数组长度不一致!");

                        if (strlen != pLcAddress.BitOffset)
                            throw new InvalidOperationException("写入" + tag.Address + "出错,输入的字符串长度与定义长度不匹配!");

                        strbyte.CopyTo(valueByte, 0);

                        _plc.WriteBytes(DataType.DataBlock, pLcAddress.DBBlockID, pLcAddress.Offset,
                            valueByte);

                        /*if (result != ErrorCode.NoError)
                            throw new InvalidOperationException(result + "\n" + "Tag: " + tag.Address);*/

                        return true;
                    }
                    else
                    {
                        switch (value)
                        {
                            case double d:
                            {
                                var bytes = Double.ToByteArray(d);
                                value = DWord.FromByteArray(bytes);
                                break;
                            }
                            case bool b:
                                value = b ? 1 : 0;
                                break;
                        }

                        _plc.Write(tag.Address, value);
                        /*if (result != ErrorCode.NoError)
                            throw new InvalidOperationException(result + "\n" + "Tag: " + tag.Address);*/

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    //LOG.Error(string.Format("S7 Driver写入地址{0},值{1},出错{2}", tag.Address, value,ex));
                    Log.Error($"S7 Driver writes address:{tag.Address}, value:{value}, Message:{ex}");
                    return false;
                }
            }
        }

        public void ReadClass(object sourceClass, int db)
        {
            _plc.ReadClass(sourceClass, db);
        }

        public void WriteClass(object sourceClass, int db)
        {
            _plc.WriteClass(sourceClass, db);
        }

        #region DB AREA

        private readonly Dictionary<short, short>
            _dbAreaLength = new Dictionary<short, short>(); //前一个int DBBlock,后一个 int读取长度

        private readonly Dictionary<short, short>
            _dbStartAddress = new Dictionary<short, short>(); //前一个int DBBlock,后一个 出现最小地址

        private readonly Dictionary<short, short>
            _dbAreaMaxAddress = new Dictionary<short, short>(); //前一个int DBBlock,后一个 出现最大地址

        private readonly Dictionary<short, byte[]> _dbAreaBuf = new Dictionary<short, byte[]>(); //前一个int DBBlock,后一个缓存区

        private short _mAreaLength;
        private short _mAreaMinAddress;
        private short _mAreaMaxAddress;
        private byte[] _mAreaBuf;

        public Dictionary<string, SiemensPLCAddress> DbAddress = new Dictionary<string, SiemensPLCAddress>();
        private readonly Dictionary<string, SiemensPLCAddress> _mAddress = new Dictionary<string, SiemensPLCAddress>();

        private short GetMaxAddress(SiemensPLCAddress addr)
        {
            short result = 0;
            if (addr != null)
            {
                if (addr.Type == "int32")
                    result = (short) (addr.Offset + 3);
                else if (addr.Type == "int16")
                    result = (short) (addr.Offset + 1);
                else if (addr.Type == "bool")
                    result = addr.Offset;
                else if (addr.Type == "string")
                    result = (short) (addr.Offset + 2 + addr.BitOffset - 1);
                else if (addr.Type == "ByteArray") //2020/6/11 ByteArray
                    result = (short) (addr.Offset + addr.BitOffset - 1);
                else
                    throw new Exception("Tag类型不支持");
                return result;
            }

            return -1;
        }

        // 根据地址增加需要读取的DB块
        public void AddAddress(Tag tag)
        {
            try
            {
                if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
                {
                    var address = tag.Address.ToUpper();
                    if (address.Substring(0, 2) == "DB")
                    {
                        var addr = SiemensPLCAddress.ConvertAddressAsDB(address);
                        //检查定义的Tag类型与地址自动解析的类型是否一致
                        if (addr.Type.ToLower() != tag.TagType.ToLower()) throw new Exception("Tag类型与地址自动解析的类型不一致");
                        DbAddress.Add(address, addr);

                        if (addr != null)
                        {
                            var dbId = addr.DBBlockID;
                            var dbOffset = addr.Offset;

                            if (_dbStartAddress.ContainsKey(dbId))
                            {
                                //如果存在该DB ID
                                if (_dbStartAddress[dbId] > dbOffset)
                                    //如果是最小地址
                                    _dbStartAddress[dbId] = dbOffset;
                            }
                            else
                            {
                                _dbStartAddress.Add(dbId, dbOffset);
                            }

                            var maxAddr = GetMaxAddress(addr);
                            if (_dbAreaMaxAddress.ContainsKey(dbId))
                            {
                                //如果存在该DB ID
                                if (_dbAreaMaxAddress[dbId] < maxAddr)
                                    //如果是最大地址
                                    _dbAreaMaxAddress[dbId] = maxAddr;
                            }
                            else
                            {
                                _dbAreaMaxAddress.Add(dbId, maxAddr);
                            }

                            if (_dbAreaLength.ContainsKey(dbId))
                                //如果存在该DB ID
                                _dbAreaLength[dbId] = (short) (_dbAreaMaxAddress[dbId] - _dbStartAddress[dbId] + 1);
                            else
                                _dbAreaLength.Add(dbId,
                                    (short) (_dbAreaMaxAddress[dbId] - _dbStartAddress[dbId] + 1));
                        }
                    }
                    else if (address.Substring(0, 1) == "M")
                    {
                        var addr = SiemensPLCAddress.ConvertAddressAsM(address);
                        //检查定义的Tag类型与地址自动解析的类型是否一致
                        if (addr.Type.ToLower() != tag.TagType.ToLower()) throw new Exception("Tag类型与地址自动解析的类型不一致");
                        _mAddress.Add(address, addr);
                        if (addr != null)
                        {
                            var mOffset = addr.Offset;

                            if (_mAreaMinAddress > mOffset)
                                //如果是最小地址
                                _mAreaMinAddress = mOffset;

                            var maxAddr = GetMaxAddress(addr);

                            if (_mAreaMaxAddress < maxAddr)
                                //如果是最大地址
                                _mAreaMaxAddress = maxAddr;

                            _mAreaLength = (short) (_mAreaMaxAddress - _mAreaMinAddress + 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //LOG.Error(string.Format("S7驱动添加地址{0}出错{1}", tag.Address, ex));
                Log.Error($"S7 driver added address:{tag.Address} error,Message: {ex}");
            }
        }

        public void UpdateDbBlock()
        {
            lock (this)
            {
                //Log.Info("S7Tag开始读取" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
                if (DbAddress.Count > 0)
                    foreach (var data in _dbAreaLength) // data:前一个int DBBlock,后一个 int读取长度
                    {
                        var dbId = data.Key;
                        var length = data.Value;
                        
                       // var dwords = _plc.Read(DataType.DataBlock, 600, 0, VarType.DWord, 4);
                        //var receive = _plc.ReadBytes(DataType.DataBlock, dbId, _dbStartAddress[dbId], 2);

                        var receive = _plc.ReadBytes(DataType.DataBlock, dbId, _dbStartAddress[dbId], length);
                        
                        //Log.Info($"接收数据为：{receive.Aggregate("",((s, b) => s+=b+","))}");
                        
                        if (receive.Length == 0)
                            Log.Error("DB" + dbId + "接收数据为空");
                        else if (receive.Length == length)
                        {
                            _dbAreaBuf[dbId] = receive;

                            //var result = BitConverter.ToInt16(new byte[]{_dbAreaBuf[6000][0],_dbAreaBuf[6000][0]},0);

                            /*if (dbId!=6000)
                                continue;
                            
                            var result = (short) ((_dbAreaBuf[6000][0] << 8) + _dbAreaBuf[6000][1]);
                            Log.Info($"触发信号：{result}");*/
                        }
                        else
                            Log.Error("DB" + dbId + "实际接收数据长度为" + receive.Length + "与理论长度" + length + "不符");
                    }

                if (_mAddress.Count > 0)
                {
                    var mreceive = _plc.ReadBytes(DataType.Memory, 0, _mAreaMinAddress, _mAreaLength);
                    if (mreceive.Length == 0)
                        Log.Error("M块接收数据为空");
                    else if (mreceive.Length == _mAreaLength)
                        _mAreaBuf = mreceive;
                    else
                        Log.Error("M块实际接收数据长度为" + mreceive.Length + "与理论长度" + _mAreaLength + "不符");
                }
            }
        }

        #endregion

        //public enum ConnectionStates
        //{
        //    Offline,
        //    Connecting,
        //    Online
        //}
    }

    public class SiemensPLCAddress
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SiemensPLCAddress));
        public short BitOffset; // 1

        public string CompleteAddress; //DB200.DBX1.1
        public short DBBlockID; // 200
        public short Offset; // 1
        public string Type; // bool

        //Byte数组的地址格式DB6002.DBB11.13
        public static SiemensPLCAddress ConvertAddressAsDB(string Address)
        {
            try
            {
                var AddressSection = Address.Split('.');
                var _Type = "";
                short _Offset = 0;
                short _BitOffset = 0;
                short _DBBlockID = 0;

                if (AddressSection.Length >= 2)
                {
                    var strDBBlock = AddressSection[0]; // DB200
                    var strOffset = AddressSection[1]; // DBX0;DBW0;DBD0;DBB0

                    var strDBBlockID = strDBBlock.Substring(2, strDBBlock.Length - 2);
                    _DBBlockID = Convert.ToInt16(strDBBlockID); //200

                    var strType = strOffset.Substring(0, 3);

                    if (strType.ToUpper() == "DBX")
                    {
                        _Type = "bool";
                        var strOffsetID = strOffset.Substring(3, strOffset.Length - 3);
                        _Offset = Convert.ToInt16(strOffsetID);
                        if (AddressSection.Length == 3)
                            _BitOffset = Convert.ToInt16(AddressSection[2]);
                        else
                            throw new Exception("Tag地址bool不支持");
                    }
                    else if (strType.ToUpper() == "DBW")
                    {
                        _Type = "int16";
                        var strOffsetID = strOffset.Substring(3, strOffset.Length - 3);
                        _Offset = Convert.ToInt16(strOffsetID);
                    }
                    else if (strType.ToUpper() == "DBD")
                    {
                        _Type = "int32";
                        var strOffsetID = strOffset.Substring(3, strOffset.Length - 3);
                        _Offset = Convert.ToInt16(strOffsetID);
                    }
                    else if (strType.ToUpper() == "STR" && strOffset.Contains("STRING"))
                    {
                        _Type = "string";
                        var strOffsetID = strOffset.Substring(6, strOffset.Length - 6);
                        _Offset = Convert.ToInt16(strOffsetID);
                        if (AddressSection.Length == 3)
                        {
                            _BitOffset = Convert.ToInt16(AddressSection[2]); //长度
                            if (_BitOffset > 0 && _BitOffset < 256)
                            {
                            }
                            else
                            {
                                throw new Exception("Tag地址string长度应在1~255之间");
                            }
                        }
                        else
                        {
                            throw new Exception("Tag地址string不支持");
                        }
                    }
                    else if (strType.ToUpper() == "DBB") //2020/6/11 ByteArray
                    {
                        //DBB4.128才算ByteArray;DBB4是一个byte类型的数字

                        _Type = "ByteArray";
                        var strOffsetID = strOffset.Substring(3, strOffset.Length - 3);
                        _Offset = Convert.ToInt16(strOffsetID);
                        if (AddressSection.Length == 3)
                        {
                            _BitOffset = Convert.ToInt16(AddressSection[2]); //长度
                            if (_BitOffset > 0 && _BitOffset < 256)
                            {
                            }
                            else
                            {
                                throw new Exception("Tag地址ByteArray长度应在1~255之间");
                            }
                        }
                        else
                        {
                            //DBB4是一个byte类型的数字
                            throw new Exception("Tag地址ByteArray不支持");
                        }
                    }
                    else
                    {
                        throw new Exception("Tag地址不支持");
                    }
                }
                else
                {
                    throw new Exception("Tag地址不支持");
                }


                return new SiemensPLCAddress
                {
                    CompleteAddress = Address,
                    Type = _Type,
                    DBBlockID = _DBBlockID,
                    Offset = _Offset,
                    BitOffset = _BitOffset
                };
            }
            catch (Exception ex)
            {
                //LOG.Error(string.Format("Tag {0}地址解析出错:{1}", Address, ex));
                LOG.Error(string.Format("Tag: {0} Address resolution error,Message:{1}", Address, ex));
                return null;
            }
        }

        public static SiemensPLCAddress ConvertAddressAsM(string Address)
        {
            try
            {
                var AddressSection = Address.Split('.');
                var _Type = "";
                short _Offset = 0;
                short _BitOffset = 0;
                short _DBBlockID = 0;

                if (AddressSection.Length == 2) //M0.0
                {
                    var strDBBlock = AddressSection[0]; // M0

                    var strOffset = strDBBlock.Substring(1, strDBBlock.Length - 1);

                    _Type = "bool";
                    _Offset = Convert.ToInt16(strOffset);
                    _BitOffset = Convert.ToInt16(AddressSection[1]);
                }
                else if (AddressSection.Length == 1) // MB,MW,MD ...
                {
                    var strDBBlock = AddressSection[0]; // MB0

                    var strOffset = strDBBlock.Substring(2, strDBBlock.Length - 2);

                    //_Type = "bool";
                    _Offset = Convert.ToInt16(strOffset);
                    _BitOffset = 0;

                    var strType = strDBBlock.Substring(0, 2);

                    if (strType.ToUpper() == "MB")
                        _Type = "byte"; //2020/6/11 原来是short,应该是有误的,改为byte
                    else if (strType.ToUpper() == "MW")
                        _Type = "int16";
                    else if (strType.ToUpper() == "MD")
                        _Type = "int32";
                    else
                        throw new Exception("Tag地址不支持");
                }
                else
                {
                    throw new Exception("Tag地址不支持");
                }

                return new SiemensPLCAddress
                {
                    CompleteAddress = Address,
                    Type = _Type,
                    DBBlockID = _DBBlockID,
                    Offset = _Offset,
                    BitOffset = _BitOffset
                };
            }
            catch (Exception ex)
            {
                //LOG.Error(string.Format("Tag {0}地址解析出错:{1}", Address, ex));
                LOG.Error($"Tag: {Address} address resolution error,Message:{ex}");
                return null;
            }
        }
    }
}