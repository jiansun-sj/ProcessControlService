using System;
using System.Threading.Tasks;

namespace ProcessControlService.WCFClients
{
    /// <summary>
    /// 冗余远程HOST管理
    /// Created by David 20170704
    /// <?xml version="1.0" encoding="utf-8" ?>
    /// <WCFClientSetting>
    ///     <RedundancyRemoteHost>
	///	        <Host Name = "Host1" Service="PartnerHost" FirstAddress="net.tcp://127.0.0.1:12006/ResourceService" 
    ///	                SecondAddress="net.tcp://127.0.0.2:12006/ResourceService"/>
	///     </RedundancyRemoteHost>
    /// </WCFClientSetting>
    /// </summary>
    public class RedundancyHostConnection : IHostConnection, IDisposable
    {
        //private static readonly ILog LOG = LogManager.GetLogger(typeof(RedundancyHostConnection));

        #region Dispose
        public void Dispose()
        {
            Dispose(true);////释放托管资源
            GC.SuppressFinalize(this);//请求系统不要调用指定对象的终结器. //该方法在对象头中设置一个位，系统在调用终结器时将检查这个位
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)//_isDisposed为false表示没有进行手动dispose
            {
                if (disposing)
                {
                    //清理托管资源
                    StopConnect();
                }
                //清理非托管资源
            }
            _isDisposed = true;
        }

        private bool _isDisposed;

        ~RedundancyHostConnection()
        {
            Dispose(false);//释放非托管资源，托管资源由终极器自己完成了
        }
        #endregion

        public RedundancyHostConnection(HostConnectionType ProxyType, string Address1, string Address2)
        {
            _address1 = Address1;
            _address2 = Address2;

            HostType = ProxyType;
            _firstHostConnection = new HostConnection(HostType, Address1);
            _secondHostConnection = new HostConnection(HostType, Address2);

            _firstHostConnection.OnConnectedHander += SubHostConnectionOnConnected;
            _secondHostConnection.OnConnectedHander += SubHostConnectionOnConnected;

            _firstHostConnection.OnDisconnectedHander += SubHostConnectionOnDisConected;
            _secondHostConnection.OnDisconnectedHander += SubHostConnectionOnDisConected;

        }

        private void SubHostConnectionOnDisConected(ServerEventArg e)
        {
            if (!Connected)
            {
                OnDisconnectedHander?.Invoke(null);
            }
        }

        private void SubHostConnectionOnConnected(ServerEventArg e)
        {
            OnConnectedHander?.Invoke(null);
        }

        public string ConnectionAddress { get { return CurrentHost.ConnectionAddress; } }
        public int ConnectionClients = 0;

        public bool ConnectToAddress1 => (ConnectionAddress == _address1);
        public bool ConnectToAddress2 => (ConnectionAddress == _address2);

        private HostConnection _firstHostConnection = null;
        private HostConnection _secondHostConnection = null;

        private string _address1;
        private string _address2;

        #region "IHostConnection接口"

        public HostConnectionType HostType { get; set; }
        private HostConnection CurrentHost
        {
            get
            {
                if (_firstHostConnection != null && _firstHostConnection.Connected && _firstHostConnection.IsMaster)
                    return _firstHostConnection;
                else if (_secondHostConnection != null && _secondHostConnection.Connected && _secondHostConnection.IsMaster)
                    return _secondHostConnection;
                else
                    return null;
            }
        }

        public IProxyConnection GetProxy()
        {

            if (CurrentHost != null)
                return CurrentHost.GetProxy();
            else
                return null;

        }

        public bool Connected
        {
            get
            {
                return CurrentHost != null && CurrentHost.Connected;
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void StopConnect()
        {
            _firstHostConnection.StopConnect();
            _secondHostConnection.StopConnect();
        }


        #endregion

        //private HostConnection GetAlivedConnection()
        //{
        //    if (_firstHostAlived && _firstHostConnection != null)
        //    {
        //        return _firstHostConnection;
        //    }
        //    else if (_secondHostAlived && _secondHostConnection != null)
        //    {
        //        return _secondHostConnection;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}



        //private HostConnection PartnerHost
        //{
        //    get
        //    {
        //        if (_firstHostAlived)
        //            return _secondHostConnection;
        //        else if (_secondHostAlived)
        //            return _firstHostConnection;
        //        else
        //            return null;
        //    }
        //}


        //private void ChangeAlivedConnection()
        //{
        //    if (CurrentHost != null && CurrentHost.Connected)
        //    { // 这台连接正常
        //        if (CurrentHost.IsMaster) //服务器状态这台是主机
        //        { // 啥都不做
        //        }
        //        else
        //        { //服务器状态这台是从机
        //            if (PartnerHost != null && PartnerHost.Connected)
        //            { // 那台连接正常
        //                if (PartnerHost.IsMaster) //服务器状态那台是主机
        //                { // 连接到那台
        //                    MasterIsChange();

        //                }
        //                else //服务器状态那台是从机
        //                {

        //                }

        //            }
        //            else
        //            {// 那台连接断开

        //            }
        //        }
        //    }
        //    else
        //    { // 这台连接断开
        //        if (PartnerHost != null && PartnerHost.Connected)
        //        {// 那台连接正常
        //            if (PartnerHost.IsMaster) //服务器状态那台是主机
        //            { // 连接到那台
        //                MasterIsChange();

        //            }
        //            else //服务器状态那台是从机
        //            {

        //            }

        //        }
        //    }

        //}

        //private void MasterIsChange()
        //{

        //}
        #region 事件
        public OnConnected OnConnectedHander { get; set; }
        public OnDisonnected OnDisconnectedHander { get; set; }
        public OnConnectFault OnConnectFaultHander { get; set; }

        public void AddConnectedHandler(OnConnected handler)
        {
            OnConnectedHander += handler;
        }

        public void AddDisconnectedHandler(OnDisonnected handler)
        {
            OnDisconnectedHander += handler;
        }

        public void AddConnectFaultHandler(OnConnectFault handler)
        {
            OnConnectFaultHander += handler;
        }

        public void StartConnect()
        {
            new Task(() =>
            {
                _firstHostConnection.StartConnect();
            }).Start();

            new Task(() =>
            {
                _secondHostConnection.StartConnect();
            }).Start();
        }

        #endregion

    }


}
