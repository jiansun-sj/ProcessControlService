using System;
using System.Threading;
using log4net;

namespace ProcessControlService.WCFClients
{
    /// <summary>
    /// 连接远程HOST管理
    /// Created by David 20170704
    /// </summary>
    ///<WCFClientSetting>
    ///    <RemoteHost>
    ///        <Host Name="Host1" Service="MachineHost" Address="net.tcp://192.168.1.231:12005/MachineService"/>
    ///        <Host Name="Host2" Service="ProcessHost" Address="net.tcp://192.168.1.231:12005/ProcessService"/>
    ///        <Host Name="Host3" Service="StorageHost" Address="net.tcp://192.168.1.231:12005/StorageService"/>		
    ///    </RemoteHost>
    ///</WCFClientSetting>

    public class HostConnection : IHostConnection
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(HostConnection));

        #region Dispose
        public void Dispose()
        {
            this.Dispose(true);////释放托管资源
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

        ~HostConnection()
        {
            this.Dispose(false);//释放非托管资源，托管资源由终极器自己完成了
        }
        #endregion

        public HostConnection(HostConnectionType ProxyType, string HostAddress)
        {
            HostType = ProxyType;
            RemoteHostAddress = HostAddress;

            StartHeartbeat();
            StartReconnect();
            //StartConnect();
        }


        #region Private Fields

        private IProxyConnection ConnectionInstance { get; set; } = null;

        private string RemoteHostAddress { get; set; } = "";


        #endregion

        #region Public Fields
        public HostConnectionType HostType { get; set; }
        public int ConnectionClients { get; set; } = 0;
        public bool Connected
        {
            get
            {
                return (ConnectionInstance != null && ConnectionInstance.Connected);
            }
        } 

        public string ConnectionAddress { get { return RemoteHostAddress; } }
        //public bool IsMaster { get; private set; } = true;
        //public bool IsMaster { get; set; } = false;
        public bool IsMaster
        {
            get
            {
                return (ConnectionInstance != null && ConnectionInstance.IsMaster());
            }
        }
        #endregion



        #region Public Methods

        public IProxyConnection GetProxy()
        {
            if (Connected)
                return ConnectionInstance;
            else
                return null;
        }


        public void StopConnect()
        {
            if (ConnectionInstance != null)
            {
                ConnectionInstance.Disconnect();
                ConnectionInstance = null;
            }
            StopHeartbeat();

            Thread.Sleep(200);
        }

        public void StartConnect()
        {
            if (Connected)
                return;

            // 创建
            switch (HostType)
            {
                case HostConnectionType.Machine:
                    ConnectionInstance = MachineProxy.Create(RemoteHostAddress);
                    break;
                case HostConnectionType.Process:
                    ConnectionInstance = ProcessProxy.Create(RemoteHostAddress);
                    break;
                case HostConnectionType.Resource:
                    ConnectionInstance = ResourceProxy.Create(RemoteHostAddress);
                    break;
                case HostConnectionType.Partner:
                    ConnectionInstance = PartnerProxy.Create(RemoteHostAddress);
                    break;
                case HostConnectionType.Admin:
                    ConnectionInstance = AdminProxy.Create(RemoteHostAddress);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // 连接
            ConnectionInstance?.Connect();

            if (Connected)
            {
                //连接事件
                ServerEventArg arg = new ServerEventArg("Unknown", ServerEventType.Connected);

                LOG.Info($"启动{HostType.ToString()}对方端口{RemoteHostAddress}成功");

                OnConnectedHander?.BeginInvoke(arg, null, null);

                //启动心跳
                //StartHeartbeat();
            }
            else
            {
                //连接失败
                ServerEventArg arg = new ServerEventArg("Unknown", ServerEventType.Fault);

                LOG.Debug($"启动{HostType.ToString()}对方端口{RemoteHostAddress}失败");


                OnConnectFaultHander?.BeginInvoke(arg, null, null);

                ConnectionInstance = null;
            }
        }
        #endregion

        #region 心跳

        //检查心跳的定时器
        private Timer _heartbeatTimer = null;
        //1秒检查一次心跳
        private static readonly long _heartbeatInterval = 1 * 1000;

        private void SendHeartBeatThread(object sender)
        {
            if(Connected)
            {
                //IsMaster = ConnectionInstance.IsMaster();
                if (!ConnectionInstance.SendHeartBeat())
                {
                    LOG.Error(string.Format("心跳检测错误"));

                    //连接错误
                    ServerEventArg arg = new ServerEventArg("Unknown", ServerEventType.Disconnected);
                    OnConnectFaultHander?.BeginInvoke(arg, null, null);
                }
            }
           

        }

        private void StartHeartbeat()
        {
            _heartbeatTimer = _heartbeatTimer ?? new Timer(new TimerCallback(SendHeartBeatThread), null, 0, _heartbeatInterval);
        }

        private void StopHeartbeat()
        {
            _heartbeatTimer.Dispose();
        }

        #endregion

        #region 重连
        //检查心跳的定时器
        private Timer _retryTimer = null;
        //1秒检查一次心跳
        private static readonly long _reconnectInterval = 5 * 1000;
        private void StartReconnect()
        {
            // 启动心跳计时器
            _retryTimer = new Timer(ReconnectThread, null, 0, _reconnectInterval);

        }

        private void StopReconnect()
        {
            if (_retryTimer != null)
            {
                _retryTimer.Dispose();
                _retryTimer = null;
            }
        }

        private bool _isFirstRun = true;

        private void ReconnectThread(object sender)
        {
            if (_isFirstRun) //第一次不执行 dongmin 20191109
            {
                _isFirstRun = false;
                return;
            }

            if (!Connected)
            {
                //LOG.Debug("尝试连接");
                //LOG.Info(string.Format("尝试{0}", strConnectionType));


                StartConnect();
            }
        }

        #endregion

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

        #endregion

    }


}
