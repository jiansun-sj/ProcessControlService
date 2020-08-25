//using FtdAdapter;
using Modbus.Device;
using System;
using System.Net.Sockets;

namespace ProcessControlService.ResourceLibrary.Machines.Drivers
{
    public class ModbusTCPMasterDriver
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ModbusTCPMasterDriver));

        // Modbus TCP parameters
        //private string _clientIP = "10.174.87.18";
        //private short _clientPort = 502;

        private TcpClient _TcpClient;
        private ModbusIpMaster _master;

        // Buffers
        //private const ushort InputRegisterStartAddress = 0;
        //private const ushort InputRegisterSize = 0;
        //private ushort[] _inputRegister = new ushort[InputRegisterSize];

        private ushort InputStatusStartAddress = 0;
        private ushort InputStatusSize = 32;
        //private bool[] _inputStatus = new bool[InputStatusSize];
        private bool[] _inputStatus;

        // properties
        public bool Connected => _connected;

        public bool _connected;
        //protected Dictionary<string, Int16> ModbusIndex = new Dictionary<string, Int16>();

        //alter by guyang 临时 为了编译通过
        public bool ConnectToClient(string _clientIP, short _clientPort, ushort _InputStatusStartAddress, ushort _InputStatusSize)
        {
            try
            {
                InputStatusStartAddress = _InputStatusStartAddress;
                InputStatusSize = _InputStatusSize;
                _inputStatus = new bool[InputStatusSize];
                _TcpClient = new TcpClient(_clientIP, _clientPort);

                _master = ModbusIpMaster.CreateIp(_TcpClient);

                _connected = true;
                return true;
            }
            catch (Exception ex)
            {
                _connected = false;
                LOG.Error(string.Format("ModbusTCPDriver连接失败！{0}", ex.Message));
                return false;
            }
        }

        public void DisconnectFromClient()
        {
            _connected = false;
            _TcpClient = null;
            _master = null;
        }



        /// <summary>
        /// 根据地址字符串获得地址类型
        /// Created by David 20170529
        /// </summary>
        /// <param name="Address">地址字符串</param>
        /// <returns>地址类型</returns>
        public static ModbusAddressType GetAddressType(string Address)
        {
            string addressArea = Address.Substring(0, 2);
            switch (addressArea.ToUpper())
            {
                case "IC":
                    return ModbusAddressType.IC;
                //case "IS":
                //    return ModbusAddressType.IS;
                case "IR":
                    return ModbusAddressType.IR;
                case "HR":
                    return ModbusAddressType.HR;
                case "WC":
                    //return ModbusAddressType.WC;   20180504 Robin注释，博世项目中使用的WC可能不是真正的WC
                    return ModbusAddressType.IC;
                case "WR":
                    return ModbusAddressType.WR;
                default:
                    throw new Exception("不支持的地址类型");
            }
        }


        //private void AddModbusItem(string Name, Int16 Address)
        //{
        //}

        //public void AsyncReadAllData()
        //{
        //    Thread checkThread = new Thread(SyncReadAllData);
        //    checkThread.Start();
        //}

        public void SyncReadAllData(ModbusAddressType address)
        {
            try
            {

                //LOG.Info("ModbusTCP_Driver");
                if (!_connected)
                    return;

                // Read from device to Buffer
                //_inputStatus = _master.ReadInputs(InputStatusStartAddress, InputStatusSize);

                //if (InputRegisterSize > 0)
                //{
                //    _inputRegister = _master.ReadInputRegisters(InputRegisterStartAddress, InputRegisterSize);
                //}
                if (address == ModbusAddressType.IC)
                {
                    if (InputStatusSize > 0)
                    {
                        _inputStatus = _master.ReadCoils(InputStatusStartAddress, InputStatusSize);
                    }
                }
                else
                {
                    LOG.Error(string.Format("读取ModbusTCPDriver数据出错：不支持{0}类型", address.ToString()));
                    return;
                }

                // Notification
                ModbusTCPDataUpdatedEventArgs Evargs = new ModbusTCPDataUpdatedEventArgs();
                DataArrivalEvent(this, Evargs);

            }
            catch (Exception ex)
            {
                _connected = false;
                LOG.Error(string.Format("读取ModbusTCPDriver数据出错：{0}", ex.Message));
            }
        }

        #region "获取返回值"

        /// <summary>
        /// Get value from Buffer
        /// Address format: IR0~IR192 (Input register)or IS0~IS60(Input status)
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        //public bool GetValue(string Address, ref ushort Value)
        //{
        //    string addressArea, addresOffset;
        //    addressArea = Address.Substring(0, 2);
        //    addresOffset = Address.Substring(2);

        //    try
        //    {

        //        Int16 iOffset = Convert.ToInt16(addresOffset);

        //        if (!(addressArea.ToLower() == "IR".ToLower()))
        //        {
        //            throw new Message("Address area not support");
        //            return false;
        //        }

        //        if (addressArea.ToLower() == "IR".ToLower())
        //        {
        //            if (iOffset >= InputRegisterStartAddress && iOffset < InputRegisterSize)
        //            {
        //                Value = _inputRegister[iOffset];
        //                return true;
        //            }
        //            else
        //            {
        //                throw new Message("Address overflow");
        //                return false;
        //            }
        //        }
        //        return false;

        //    }
        //    catch (Message ex)
        //    {
        //        LOG.Error(string.Format("读取变量{0}出错.错误代码：{1}", Address, ex));
        //        return false;
        //    }


        //}

        /// <summary>
        /// Get value from Buffer
        /// Address format: IR0~IR192 (Input register)or IS0~IS60(Input status)
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        //public bool GetValue(string Address, ref Single Value)
        //{
        //    string addressArea, addresOffset;
        //    addressArea = Address.Substring(0, 2);
        //    addresOffset = Address.Substring(2);

        //    try
        //    {

        //        Int16 iOffset = Convert.ToInt16(addresOffset);

        //        if (!(addressArea.ToLower() == "IR".ToLower()))
        //        {
        //            throw new Message("Address area not support");
        //            return false;
        //        }

        //        if (addressArea.ToLower() == "IR".ToLower())
        //        {
        //            if (iOffset >= InputRegisterStartAddress && iOffset < InputRegisterSize)
        //            {
        //                ushort low, high;
        //                low = _inputRegister[iOffset];
        //                high = _inputRegister[iOffset + 1];

        //                Value = Modbus.Utility.ModbusUtility.GetSingle(high, low);
        //                return true;
        //            }
        //            else
        //            {
        //                throw new Message("Address overflow");
        //                return false;
        //            }
        //        }
        //        return false;

        //    }
        //    catch (Message ex)
        //    {
        //        LOG.Error(string.Format("读取变量{0}出错.错误代码：{1}", Address, ex));
        //        return false;
        //    }


        //}

        /// <summary>
        /// Get value from Buffer
        /// Address format: IR0~IR192 (Input register)or IS0~IS60(Input status)
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public bool GetValue(string Address, ref bool Value)
        {
            string addressArea, addresOffset;
            addressArea = Address.Substring(0, 2);
            addresOffset = Address.Substring(2);

            try
            {

                int iOffset = Convert.ToInt32(addresOffset);

                //if (!(addressArea.ToLower() == "IC".ToLower()))
                //{
                //    throw new Message("Address area not support");
                //    //return false;
                //}

                if (addressArea.ToLower() == "IC".ToLower())
                {
                    if (iOffset >= InputStatusStartAddress && iOffset < InputStatusSize + InputStatusStartAddress)
                    {
                        Value = _inputStatus[iOffset - InputStatusStartAddress];
                        //LOG.Error(string.Format("读取变量{0}成功.值：{1}", Address, Value));
                        return true;
                    }
                    else
                    {
                        LOG.Error(string.Format("读取变量{0}出错.错误代码：{1}", Address, "地址超出范围！"));
                        return false;
                    }
                }
                else if (addressArea.ToLower() == "WC".ToLower())
                {
                    iOffset = iOffset + 16;
                    if (iOffset >= InputStatusStartAddress && iOffset < InputStatusSize + InputStatusStartAddress)
                    {
                        Value = _inputStatus[iOffset - InputStatusStartAddress];
                        //LOG.Error(string.Format("读取变量{0}成功.值：{1}", Address, Value));
                        return true;
                    }
                    else
                    {
                        LOG.Error(string.Format("读取变量{0}出错.错误代码：{1}", Address, "地址超出范围！"));
                        return false;
                    }
                }
                else
                {
                    LOG.Error(string.Format("读取变量{0}出错.错误代码：{1}", Address, "地址类型出错！"));
                    return false;
                }


            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("读取变量{0}出错.错误代码：{1}", Address, ex));
                return false;
            }


        }

        #endregion

        #region "Data Arrival Event"

        //ModbusTCPDataUpdatedEventArgs Evargs;
        public delegate void DataArrivalEventHandler(object sender, ModbusTCPDataUpdatedEventArgs args);
        private DataArrivalEventHandler DataArrivalEvent = null;

        public event DataArrivalEventHandler DataArrival
        {
            add
            {
                DataArrivalEvent = (DataArrivalEventHandler)System.Delegate.Combine(DataArrivalEvent, value);
            }
            remove
            {
                DataArrivalEvent = (DataArrivalEventHandler)System.Delegate.Remove(DataArrivalEvent, value);
            }
        }
        #endregion

        #region 写值

        public bool WriteItem(ushort Index, bool Value)
        {
            bool result = false;


            //Monitor.Enter(_portLocker);

            try
            {
                if (!Connected)
                {
                    // Monitor.Exit(_portLocker);
                    // LOG.Debug(string.Format("写操作已解锁"));
                    return false;
                }

                //try
                //{
                Index = (ushort)(Index + 16);
                //_master.WriteSingleCoil(1, Index, Value);
                _master.WriteSingleCoil(Index, Value);
                //Monitor.Pulse(_writingLocker);


                //}
                //catch (Message ex)
                //{
                //    LOG.Debug(string.Format("WriteItem出错：{0}",ex.Message));

                //}

                //Thread.Sleep(200);

                //RunFlag = false;

                result = true;
                //return true;
            }
            catch (Exception ex)
            {
                //WarnLog("clsDeviceOPC.WriteData() " + ex.Message);
                LOG.Error(string.Format("ModbusTCP的WriteItem出错：{0}", ex.Message));
                result = false;
            }
            return result;

        }

        //public bool WriteItem(ushort Index, ushort Value)
        //{
        //    try
        //    {

        //        if (!Connected)
        //        {
        //            return false;
        //        }
        //        bool result = false;


        //            //Monitor.Enter(_portLocker);
        //            // LOG.Debug(string.Format("写操作已加锁"));
        //            try
        //            {
        //                if (!Connected)
        //                {
        //                    //Monitor.Exit(_portLocker);
        //                    //LOG.Debug(string.Format("写操作已解锁"));
        //                    return false;
        //                }

        //                //try
        //                //{
        //                _master.WriteSingleRegister(_slaveID, Index, Value);

        //                //Monitor.Pulse(_writingLocker);


        //                //}
        //                //catch (Message ex)
        //                //{
        //                //    LOG.Debug(string.Format("WriteItem出错：{0}",ex..Message));

        //                //}

        //                Thread.Sleep(200);

        //                //RunFlag = false;

        //                result = true;
        //                //return true;
        //            }
        //            catch (Message ex)
        //            {
        //                //WarnLog("clsDeviceOPC.WriteData() " + ex.Message);
        //                LOG.Error(string.Format("WriteItem出错：{0}", ex.Message));
        //                result = false;
        //            }
        //            return result;
        //    }
        //    catch (Message ex)
        //    {
        //        //WarnLog("clsDeviceOPC.WriteData() " + ex.Message);
        //        LOG.Error(ex.Message);
        //        return false;
        //    }
        //}

        //public bool WriteItems(ushort Index, ushort[] Value)
        //{
        //    try
        //    {
        //        if (!Connected)
        //        {
        //            return false;
        //        }

        //        _master.WriteMultipleRegisters(Index, Value);return true;
        //    }
        //    catch (Message ex)
        //    {
        //        LOG.Error(ex.Message);
        //        return false;
        //    }
        //}

        #endregion
    }

    public class ModbusTCPDataUpdatedEventArgs : EventArgs
    {
        //public Modbus.Data.ModbusDataType DataType;
    }
    public enum ModbusAddressType
    {
        IC = 0,     //输入线圈
        //IS = 1,     //输入开关量 -- IS
        IR = 2,     //输入寄存器 -- IR
        HR = 3,     //保持寄存器 -- HR
        WC = 4,     // 输出线圈
        WR = 5      // 输出寄存器
    }
}
