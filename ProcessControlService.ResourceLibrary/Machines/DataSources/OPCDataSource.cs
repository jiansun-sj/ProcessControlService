using log4net;
using ProcessControlService.ResourceFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OPCAutomation;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class OPCDataSource : DataSource
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(OPCDataSource));

        private Hashtable OPCIndex = new Hashtable();

        public OPCDataSource(string Name, Machine machine)
            : base(Name, machine)
        {
        }

        protected override void Dispose(bool disposing)
        {
            _lastRunning = true;
            base.Dispose(disposing);
        }

        private bool _lastRunning = false;

        public override void Disconnect()
        {
            lock (this)
            {
                if (_lastRunning)
                {
                    //ContinueReading = false;
                    if (Connected)
                    {

                        //Connected = false;

                        Thread.Sleep(1000); // Waiting for the auto read OPC thread stop

                        StopOPCClient();
                    }
                }
            }
        }

        public override bool LoadFromConfig(XmlNode node)
        {

            XmlElement level1_item = (XmlElement)node;

            OPCServerName = level1_item.GetAttribute("OPCServerName");
            if (level1_item.HasAttribute("AliveCheck")) // add by David 20170705
            { // 增加通信检测Tag
                string strAliveCheck = level1_item.GetAttribute("AliveCheck");

                Tag new_tag = new Tag("AliveCheck", Owner, TagAccessType.Read);
                new_tag.Address = strAliveCheck;
                new_tag.TagType = "bool";
                AddItem(new_tag);
            }
            else
            {
                LOG.Error(string.Format("OPC数据源{0}未设定AliveCheck属性", this));
            }
            return base.LoadFromConfig(node);
        }

        private bool _firstConnect = true;
        protected override bool Connect()
        {
            lock (this)
            {

                if (_firstConnect) //OPC客户端之需在程序启动时连接，不需每次断线重连
                {
                    _firstConnect = false;
                    try
                    {
                        if (StartOPCClient())
                        {
                            Thread.Sleep(100);
                            if (CheckConnected())
                            {
                                LOG.Info(string.Format("连接到OPC{0}成功.", SourceName));
                                return true;
                            }
                        }
                        LOG.Info(string.Format("连接OPC{0}失败.", SourceName));
                        return false;

                    }
                    catch (Exception ex)
                    {
                        LOG.Error(string.Format("启动OPC{0}设备连接失败:{0}", SourceName, ex));
                        return false;
                    }
                }
                else
                {
                    //检查是否重连
                    if (CheckConnected())
                    {
                        LOG.Info(string.Format("检查是否重连,_firstConnect为false; 连接到OPC{0}.", SourceName));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }

        }

        public override void AddItem(Machine Owner, string Name, string Type, string Address)
        {
            base.AddItem(Owner, Name, Type, Address);

            AddOpcItem(Name, Address);
        }

        public override void AddItem(Tag tag)
        {
            base.AddItem(tag);

            AddOpcItem(tag.TagName, tag.Address);

        }

        private int updateCount = 0;

        public override bool UpdateAllValue()
        {
            lock (this) // David 20170705
            {
                try
                {

                    //AsyncReadAllData(); //异步读取

                    // 同步读取 David 20170705
                    //SyncReadAllData();
                    if (updateCount == 50)
                    {
                        GC.Collect(GC.GetGeneration(this));
                        updateCount = 0;
                    }

                    if (SyncReadAllData() == false)
                    {
                        LOG.Error(string.Format("无法读取OPC数据"));
                        if (CheckConnected())
                        {
                            LOG.Info(string.Format("检查是否重连,_firstConnect为false; 连接到OPC{0}.", SourceName));
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    updateCount++;

                    return true;

                }
                catch (Exception ex)
                {

                    LOG.Error(string.Format("更新OPC所有数据出错：{0}", ex.Message));
                    //Disconnect();

                    return false;
                }
            }
        }

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            if (Owner.Mode != RedundancyMode.Master)
            {
                return true;
            }

            WriteOpcItem(Convert.ToInt16(OPCIndex[tag.TagName]), value);
            return true;

        }

        private long checkCount = 0;
        // 检查通信连接 - David 20170709
        protected override bool CheckConnected()
        {
            lock (this)
            {
                try
                {
                    object CommOK = new bool();
                    checkCount++;
                    //LOG.Debug(string.Format("开始检测连接状态 {0}", checkCount));
                    if (ReadOpcItem(1, ref CommOK)) //读取列表里面第一个Tag
                    {
                        if ((bool)CommOK)
                        {
                            //LOG.Debug(string.Format("连接状态OK {0}", checkCount));

                            Connected = (bool)CommOK;
                            return Connected;
                        }
                        else
                        {
                            //LOG.Debug(string.Format("连接状态断开 {0}", checkCount));

                            Connected = false;
                            return false;
                        }
                    }
                    else
                    {
                        LOG.Debug(string.Format("连接状态检测出错 {0}", checkCount));

                        Connected = false;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("检测OPC连接状态出错:{0}", ex));
                    return false;
                }
            }
        }


        #region "opc functions"
        public string OPCServerName = "";
        //public string GroupName = "";
        //public Int16 OPCUpdateInterval;

        private OPCAutomation.OPCServer ObjOPCServer;
        private OPCAutomation.OPCGroups ObjOPCGroups;
        private OPCAutomation.OPCGroup ObjOPCGroup;
        private OPCAutomation.OPCItems ObjOPCItems;
        //Private ObjOPCItem As OPCAutomation.OPCItem

        protected int NoOfItems = 0; //no of memory areas to save data from OPC device
        private List<string> List_Address = new List<string>(); // OPC device address
        private List<string> List_TagName = new List<string>(); // OPC device address
        private List<bool> List_Readonly = new List<bool>(); // Readonly setting areas
        public List<object> List_Values = new List<object>(); // memory areas
        public List<object> Saved_List_Values = new List<object>(); // memory areas

        //private List<string> Array_Items = new List<string>();
        //private List<int> Array_ClientHanlers = new List<int>();
        //private List<int> Array_ServerHandlers = new List<int>();
        private Array Array_Address;
        private Array Array_ClientHanlers;
        private Array Array_ServerHandlers;

        //private int readClientTransactionID = 1976; //Use for async read OPC data   注释Robin20180203
        //private int readServerTransactionID; //Use for async read OPC data           注释 Robin20180203

        //int writeClientTransactionID = 1977; //Use for async write OPC data
        //int writeServerTransactionID; //Use for async write OPC data

        #region "Data Arrival Event"
        private OPCDataUpdatedEventArgs Evargs;
        public delegate void DataArrivalEventHandler(object sender, OPCDataUpdatedEventArgs args);
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

        private void Init_opc()
        {
            //AddOpcItem("DummyITem", "DummyAddress");
        }

        //################ Sub procedure to initialize OPC Server ######
        private bool StartOPCClient()
        {
            int liCounter = 0;
            Array gpServerHandlers = null;
            Array gsErrors = null;
            string GroupName = SourceName;
            try
            {
                ObjOPCServer = new OPCServer();
                //##### Initialize OPC Server ################################
                ObjOPCServer.Connect(OPCServerName, "");
                ObjOPCGroups = ObjOPCServer.OPCGroups;
                ObjOPCGroup = ObjOPCGroups.Add(GroupName);
                ObjOPCGroup.AsyncReadComplete += ObjOPCGroup_AsyncReadComplete;
                //Add OPCGroup
                ObjOPCGroup.UpdateRate = UpdateInterval;
                ObjOPCGroup.IsActive = false;
                ObjOPCGroup.IsSubscribed = ObjOPCGroup.IsActive;
                ObjOPCItems = ObjOPCGroup.OPCItems;

                //Create opc items
                Array_Address = Array.CreateInstance(typeof(string), NoOfItems + 1);
                Array_ClientHanlers = Array.CreateInstance(typeof(int), NoOfItems + 1);
                Array_ServerHandlers = Array.CreateInstance(typeof(int), NoOfItems + 1);



                //Build OPCItems Array
                for (liCounter = 1; liCounter <= NoOfItems; liCounter++)
                {
                    Array_Address.SetValue(List_Address[liCounter - 1], liCounter);
                    Array_ClientHanlers.SetValue(liCounter, liCounter);
                }

                ObjOPCItems.AddItems(NoOfItems, ref Array_Address, ref Array_ClientHanlers, out
                    gpServerHandlers, out gsErrors);

                //Get the server handlers
                bool HasError = false;
                for (liCounter = 1; liCounter <= NoOfItems; liCounter++)
                {
                    if ((int)gsErrors.GetValue(liCounter) == 0)
                    {
                        Array_ServerHandlers.SetValue((int)gpServerHandlers.GetValue(liCounter), liCounter);
                    }
                    else
                    {
                        LOG.Error("Tag \'" + List_Address[liCounter - 1] + "\' has problem连接失败错误");
                        HasError = true;
                    }
                }

                if (!HasError)
                {
                    LOG.Info(" 启动OPC组 \'" + SourceName + "\'");
                }

                return !HasError;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("连接OPC出错：读取资源{0}{1}", GroupName, ex.Message));
                return false;
            }

        }

        //################ Sub procedure to initialize OPC Server ######
        private void StopOPCClient()
        {
            //       ==
            //	Description     :	断开连接OPCServer，返回值False为断开失败，True为断开成功
            //	Created			:	2009/01/19
            //	Author			:	zgh
            //   Update          :
            //   Updater         :
            //       ==
            //bool value = false;
            try
            {
                ObjOPCGroup.IsActive = false;
                //ObjOPCGroup.Remove(ObjOPCGroup.ServerHandle)
                ObjOPCGroups.RemoveAll();
                ObjOPCItems = null;
                ObjOPCGroups = null;
                ObjOPCGroup = null;
                //ObjOPCGroup.AsyncReadComplete += ObjOPCGroup_AsyncReadComplete;
                ObjOPCServer.Disconnect();
                ObjOPCServer = null;
                //colTagName = Nothing
                //value = true;

                //MessageLog("clsDeviceOPC.StopOPCClient(): Disconnected from OPC device \'" + DeviceName + "\'");
                LOG.Info("断开OPC连接 \'" + SourceName + "\'");


            }
            catch (Exception ex)
            {
                //value = false;
                //WarnLog(ex.StackTrace + "\r\n" + ex.Message);
                //  MsgBox(ex.Message)
                LOG.Error(string.Format("OPCDataSource断开连接出错行：{0},", ex.Message));
            }
            finally
            {
                System.GC.Collect(); //强制垃圾回收
            }
        }

        private short AddOpcItem(string Name, string ItemAddress)
        {
            short returnValue = 0;

            returnValue = (short)NoOfItems;

            NoOfItems++;

            List_Address.Add(ItemAddress);
            List_TagName.Add(Name);
            List_Values.Add(new object());
            Saved_List_Values.Add(new object());

            //Array_Items.Add(null);
            //Array_ClientHanlers.Add(0);
            //Array_ServerHandlers.Add(0);

            //add to hashtable
            OPCIndex.Add(Name, NoOfItems);

            return returnValue;
        }

        //public bool CreateItemList(string[] itemList)
        //{

        //    string item = "";

        //    foreach (string tempLoopVar_item in itemList)
        //    {
        //        item = tempLoopVar_item;
        //        AddDataItem(item);
        //    }

        //    return false;
        //}

        //-----------------------------------------------------------------
        // write by ItemID

        private bool WriteOpcItem(short Index, object Value)
        {
            Array gsErrors = null;
            try
            {
                if (!Connected)
                {
                    LOG.Error("未连接到OPC写点失败");
                    return false;

                }

                if (ObjOPCGroup == null)
                {
                    return false;
                }

                //Add OPCItems
                Array tempArray_ServerHanlers = Array.CreateInstance(typeof(int), 2);
                tempArray_ServerHanlers.SetValue(Array_ServerHandlers.GetValue(Index), 1);

                Array tempArray_Values = Array.CreateInstance(typeof(object), 2);
                tempArray_Values.SetValue(Value, 1);

                //ObjOPCGroup.IsSubscribed = True

                ObjOPCGroup.SyncWrite(1, ref tempArray_ServerHanlers, ref tempArray_Values, out gsErrors);

                bool HasError = false;
                if ((int)gsErrors.GetValue(1) != 0)
                {
                    //MessageLog("clsDeviceOPC.WriteData(): Tag \'" + Array_Items[Index] + "\' has problem");
                    LOG.Error("Tag \'" + List_Address[Index - 1] + "\' 有问题写失败错误");
                    HasError = true;
                }

                return !HasError;

            }
            catch (Exception ex)
            {
                //WarnLog("clsDeviceOPC.WriteData() " + ex.Message);
                LOG.Error(string.Format("OPCDataSource出错354行：{0},", ex.Message));
                return false;
            }
        }

        //-----------------------------------------------------------------
        // read by ItemID
        private bool ReadOpcItem(short Index, ref object Value)
        {

            Array gsErrors = null;
            try
            {
                //if (!Connected)
                //    return false;

                if (ObjOPCGroup == null)
                    return false;

                Array tempArray_ServerHanlers = Array.CreateInstance(typeof(int), 2);
                tempArray_ServerHanlers.SetValue(Array_ServerHandlers.GetValue(Index), 1);


                //ObjOPCGroup.IsSubscribed = True


                ObjOPCGroup.SyncRead((short)OPCAutomation.OPCOPCDevice, 1, ref tempArray_ServerHanlers, out Array tempArray_Values, out gsErrors, out object null_object, out object null_object2);

                bool HasError = false;
                if (tempArray_Values.GetValue(1) == null)
                {
                    //WarnLog("clsDeviceOPC.ReadData(): Tag \'" + Array_Items[Index] + "\' has problem");
                    LOG.Error("Tag \'" + List_Address[Index - 1] + "\' 有问题读失败错误");
                    HasError = true;
                }
                else
                {
                    // Copy the result value from temp array to real array
                    List_Values[Index - 1] = tempArray_Values.GetValue(1);
                    // return the real value
                    Value = List_Values[Index - 1];
                }

                return !HasError;

            }
            catch (Exception ex)
            {
                //WarnLog("clsDeviceOPC.ReadData() " + ex.Message);
                LOG.Error(string.Format("OPCDataSource出错406行：{0},", ex.Message));
                return false;
            }

        }

        private bool SyncReadAllData()
        {
            int liCounter = 0;

            Array gsErrors = null;
            try
            {
                //if (!Connected)
                //    return false;

                if (ObjOPCGroup == null)
                    return false;

                //int[] tempArray_ServerHanlers =  new int[NoOfItems];
                //Array_ServerHandlers.CopyTo(tempArray_ServerHanlers, 1);


                //'ObjOPCGroup.IsSubscribed = True

                object tempQualities = new object();
                object timestamps = new object();

                ObjOPCGroup.SyncRead((short)OPCAutomation.OPCOPCDevice, NoOfItems, ref Array_ServerHandlers,
                    out Array tempArray_Values, out gsErrors, out tempQualities, out timestamps);

                bool HasError = false;
                for (liCounter = 1; liCounter <= NoOfItems; liCounter++)
                {
                    if ((int)gsErrors.GetValue(liCounter) == 0)
                    {
                        //Array_ServerHandlers.SetValue((int)tempArray_Values.GetValue(liCounter), liCounter);
                        List_Values[liCounter - 1] = tempArray_Values.GetValue(liCounter);
                        // send to Tag
                        Tags[List_TagName[liCounter - 1]].TagValue = List_Values[liCounter - 1];
                    }
                    else
                    {
                        //WarnLog("clsDeviceOPC.StartOPCClient(): Tag \'" + Array_Items[liCounter] + "\' has problem");
                        HasError = true;
                    }
                }

                //if (!HasError)
                //{
                //    //MessageLog("clsDeviceOPC.StartOPCClient(): Connected to OPC device \'" + DeviceName + "\'");
                //    LOG.Error(" Connected to OPC group \'" + SourceName + "\'");
                //}

                // 数据变化通知事件
                if (DataArrivalEvent != null)
                {
                    Evargs = new OPCDataUpdatedEventArgs();

                    for (int i = 0; i < List_Values.Count; i++)
                    {
                        if (List_Values[i] == null || Saved_List_Values[i] == null)
                        {//如果空值则不比较 David 20170705
                            continue;
                        }

                        if (!Tag.ValueEqual(Saved_List_Values[i], List_Values[i]))
                        { // value update
                            Saved_List_Values[i] = List_Values[i];

                            Evargs.AddUpdateTag(List_TagName[i], List_Values[i]);
                        }
                    }

                    DataArrivalEvent(this, Evargs);
                }

                return (!HasError);
            }
            catch (Exception ex)
            {
                //WarnLog("clsDeviceOPC.SyncReadAllData() " + ex.Message)
                LOG.Error(string.Format("OPCDataSource同步读取出错：{0},", ex.Message));
                return false;
            }

        }

        private void ObjOPCGroup_AsyncReadComplete(int TransactionID, int NumItems, ref System.Array ClientHandles, ref System.Array ItemValues, ref System.Array Qualities, ref System.Array TimeStamps, ref System.Array Errors)
        {
            try
            {

                int index = 0;

                for (index = 1; index <= NumItems; index++)
                {
                    List_Values[(int)ClientHandles.GetValue(index) - 1] = ItemValues.GetValue(index);

                    // send to Tag
                    Tags[List_TagName[(int)ClientHandles.GetValue(index) - 1]].TagValue = ItemValues.GetValue(index);

                    //Array_Values(ClientHandles(index)) = index
                }


                if (DataArrivalEvent != null)
                {
                    Evargs = new OPCDataUpdatedEventArgs();

                    for (int i = 0; i < List_Values.Count; i++)
                    {
                        if (!Tag.ValueEqual(Saved_List_Values[i], List_Values[i]))
                        { // value update
                            Saved_List_Values[i] = List_Values[i];

                            Evargs.AddUpdateTag(List_TagName[i], List_Values[i]);
                        }
                    }

                    DataArrivalEvent(this, Evargs);
                }

                //if (ContinueReading)
                //{
                //    AsyncReadAllData();
                //    LOG.Info(string.Format("{0} read again.", SourceName));
                //}

            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("OPCDataSource异步读取出错：{0},", ex.Message));
                //WarnLog("clsDeviceOPC.ObjOPCGroup_AsyncReadComplete() " + ex.Message);
            }

        }

        #endregion


    }

    public class OPCDataUpdatedEventArgs : EventArgs
    {
        private List<string> _tags = new List<string>();
        private List<object> _tagValues = new List<object>();

        public void AddUpdateTag(string TagName, object Value)
        {
            _tags.Add(TagName);
            _tagValues.Add(Value);
        }

        public string GetTag(int index)
        {
            return _tags[index];
        }

        public object GetValue(int index)
        {
            return _tagValues[index];
        }

        public int Count => _tags.Count;


    }

}
