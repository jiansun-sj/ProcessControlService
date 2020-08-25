using System;
using System.Diagnostics;
using System.Threading;
using ProcessControlService.WCFClients;

namespace ProcessControlService.ResourceFactory
{
    public class Redundancy : IWork, IDisposable
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Redundancy));

        //private bool _master = false;

        public RedundancyMode Mode { get; private set; } = RedundancyMode.Unknown;

        public RedundancyMode PartnerMode { get; private set; } = RedundancyMode.Unknown;
        private RedundancyMode _lastPartnerMode = RedundancyMode.Unknown;

        private long _partnerRunTime = 0;

        private long _startTime = Stopwatch.GetTimestamp();
        public long RunTime => (Stopwatch.GetTimestamp() - _startTime);

        private IHostConnection _host = null;

        public Redundancy()
        {

        }

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
                    if (_host != null)
                    {
                        // _host.ReleaseConnection(HostConnectionType.Partner);
                        _host = null;
                        StopWork();
                    }
                }
                //清理非托管资源
            }
            _isDisposed = true;
        }

        private bool _isDisposed;

        ~Redundancy()
        {
            this.Dispose(false);//释放非托管资源，托管资源由终极器自己完成了
        }
        #endregion


        #region IWork interface
        public void StartWork()
        {
            _worked = true;
            PartnerAlive = false;

            InitPartnerConnection();

            StartCheckPartnerStatus();

            //StartSync();
        }

        private bool _worked;
        public bool WorkStarted => _worked;

        public void StopWork()
        {
            if (_worked)
            {
                _worked = false;
                StopCheckPartnerStatus();
                StopSync();

            }

        }
        #endregion

        #region ConnectionManagement
        private void InitPartnerConnection()
        {

            //StartCheckPartnerStatus();

            _host = HostConnectionManager.CreateConnection(HostConnectionType.Partner);

            _host.AddConnectedHandler(OnConnectToPartner);
            _host.AddDisconnectedHandler(OnDisconnectToPartner);
            _host.AddConnectFaultHandler(OnConnectToPartnerFault);

            _host.StartConnect();

        }

        //public RedundancyMode GetRedundancyMode()
        //{
        //    return RedundancyMode.Unknown;
        //}


        public void OnConnectFromPartner()
        {
            Log.Debug("RedundancyServer:检测到冗余伙伴上线");

            //StartCheckPartnerStatus(); //打开检测伙伴线程

        }

        public void OnDisconnectFromPartner()
        {
            Log.Debug("RedundancyServer:检测到冗余伙伴断线");

            //StopCheckPartnerStatus(); //关闭伙伴检测线程

        }

        private void OnConnectToPartner(ServerEventArg arg)
        {
            Log.Debug("RedundancyClient:连接到冗余伙伴");
            PartnerAlive = true;

        }

        private void OnConnectToPartnerFault(ServerEventArg arg)
        {
            Log.Debug("RedundancyClient:冗余伙伴连接错误");

            //ChangeMode(RedundancyMode.Master);
            PartnerAlive = false;


        }

        private void OnDisconnectToPartner(ServerEventArg arg)
        {
            Log.Debug("RedundancyClient:断开到冗余伙伴连接");
            PartnerAlive = false;

        }

        public void ChangeMode(RedundancyMode mode)
        {
            if (mode == Mode)
            { //没有变化
                return;
            }
            // 有变化
            Log.Debug($"主从模式从{Mode}切换成{mode}");

            Mode = mode;

            // 通知所有资源对象

            //先启动Machine资源  sunjian 2020-3-2
            foreach (var resource in ResourceManager.GetAllResources())
            {
                if (!(resource is IRedundancy res)) continue;
                if (resource.ResourceType != "Machine") continue;

                if (res.CurrentRedundancyMode != mode)
                    res.RedundancyModeChange(mode);
            }

            //启动非Machine和非Process的资源 sunjian 2020-3-2
            foreach (var resource in ResourceManager.GetAllResources())
            {
                if (!(resource is IRedundancy res)) continue;
                if (resource.ResourceType == "Machine" || resource.ResourceType == "Process") continue;

                if (res.CurrentRedundancyMode != mode)
                    res.RedundancyModeChange(mode);
            }

            //最后启动非Machine和非Process的资源 sunjian 2020-3-2
            foreach (var resource in ResourceManager.GetAllResources())
            {
                if (!(resource is IRedundancy res)) continue;
                if (resource.ResourceType != "Process") continue;

                if (res.CurrentRedundancyMode != mode)
                    res.RedundancyModeChange(mode);
            }

            if (mode == RedundancyMode.Master)
            {
                StartSync(); //启动同步
            }
            else
            {
                StopSync(); //关闭同步
            }
        }

        public void TogglePartnerMode()
        {
            Log.Debug("Redundancy主从切换原因:手动切换");

            if (Mode == RedundancyMode.Master)
            {
                //重置开启时间
                _startTime = Stopwatch.GetTimestamp();
                ChangeMode(RedundancyMode.Slave);
            }
            else
            {
                ChangeMode(RedundancyMode.Master);
            }
        }

        #endregion

        #region Partner status check
        private bool PartnerAlive = false;

        //检查同步的定时器
        private Timer _statusCheckTimer = null;
        //1秒同步一次
        private static readonly int _statusCheckInterval = 1 * 1000;
        public static int StatusCheckInterval => _statusCheckInterval;


        //private Thread _slaveCheck = null;

        //private bool IsPartnerModeChange()
        //{
        //    if (_lastPartnerMode != PartnerMode)
        //    {
        //        LOG.Debug(string.Format("Redundancy:冗余伙伴的模式从{0}变化成{1}", _lastPartnerMode.ToString(),PartnerMode.ToString()));

        //        _lastPartnerMode = PartnerMode;
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        private void RedundancyStatusCheck(object sender)
        {
            // 获得对方的模式
            if (PartnerAlive)
            {
                //LOG.Debug("Redundancy:检测对方正在运行");

                PartnerProxy _proxy = (PartnerProxy)_host.GetProxy();
                if (_proxy != null)
                {
                    var result = _proxy.GetPartnerMode();

                    PartnerMode = (RedundancyMode)(Enum.ToObject(typeof(RedundancyMode), result));

                    _partnerRunTime = _proxy.GetPartnerRunTime();
                    //LOG.Debug(string.Format("RedundancyClient:冗余伙伴的模式为:{0}", PartnerMode.ToString()));
                }

                //模式比较
                if (Mode == RedundancyMode.Master)
                {
                    if (PartnerMode == RedundancyMode.Master)
                    {
                        if (!WeAreFirstRun())
                        {//对方先运行
                            Log.Debug("Redundancy主从切换原因:双方Master，对方先启动");
                            ChangeMode(RedundancyMode.Slave);
                        }
                    }

                }
                else if (Mode == RedundancyMode.Slave)
                {
                    if (PartnerMode == RedundancyMode.Slave)
                    {
                        if (WeAreFirstRun())
                        {//我方先运行
                            Log.Debug("Redundancy主从切换原因:对方Slave,我方Slave,我方先启动");
                            ChangeMode(RedundancyMode.Master);
                        }
                    }
                }

                else if (Mode == RedundancyMode.Unknown)
                {
                    if (PartnerMode == RedundancyMode.Master)
                    {
                        Log.Debug("Redundancy主从切换原因:对方Master,我方是Unknown");
                        ChangeMode(RedundancyMode.Slave);
                    }
                    else if (PartnerMode == RedundancyMode.Slave)
                    {
                        Log.Debug("Redundancy主从切换原因对方Slave,我方是Unknown");
                        ChangeMode(RedundancyMode.Master);
                    }
                    else if (PartnerMode == RedundancyMode.Unknown)
                    {
                        if (WeAreFirstRun())
                        {//我方先运行
                            Log.Debug("Redundancy主从切换原因对方Unknown,我方是Unknown,我方先启动");
                            ChangeMode(RedundancyMode.Master);
                        }
                    }
                }

                PartnerUnliveCounts = 0;

            }
            else
            { // 连接断开
                PartnerMode = RedundancyMode.Unknown;

                if (Mode != RedundancyMode.Master)
                {
                    if (PartnerUnliveCounts >= ConfirmPartnerUnliveLimitation)
                    {
                        PartnerUnliveCounts = 0;
                        Log.Debug("Redundancy主从切换原因:对方不在线,已经过了验证期");
                        ChangeMode(RedundancyMode.Master);
                    }
                    else
                    {
                        Log.Debug(string.Format("Redundancy：对方不在线,验证{0}次", PartnerUnliveCounts));
                        PartnerUnliveCounts++;
                    }
                }

            }



        }

        private const int ConfirmPartnerUnliveLimitation = 10;//sunjian 2019-12-09 验证期更改为10次
        private int PartnerUnliveCounts = 0;

        //返回true,表示我们是先运行
        private bool WeAreFirstRun()
        {
            Log.Debug(string.Format("Redundancy:对方Runtime={0},对方Runtime={1}", _partnerRunTime, RunTime));

            return (RunTime > _partnerRunTime);
        }

        private void StartCheckPartnerStatus()
        {

            //_slaveCheck = new Thread(new ThreadStart(CheckPartnerStatus)); //开启从站检测线程
            //_slaveCheck.Start();

            // 启动计时器
            if (_statusCheckTimer == null)
            {
                _statusCheckTimer = new Timer(new TimerCallback(RedundancyStatusCheck), null, 0, _statusCheckInterval);
            }
        }

        private void StopCheckPartnerStatus()
        {
            _statusCheckTimer = null;
        }

        #endregion

        #region Data synchronization
        //检查同步的定时器
        private Timer _syncTimer = null;
        //1秒同步一次
        private static int _syncInterval = 1 * 500;

        /// <summary>
        /// 修改冗余同步时间，xml中可以设置新同步间隔。
        /// jiansun 2019-11-10
        /// </summary>
        /// <param name="syncInterval"></param>
        public void ChangeSyncInterval(string syncInterval)
        {
            Log.Info(int.TryParse(syncInterval, out _syncInterval)
                ? $"冗余同步定时器时间间隔设置为：{syncInterval} ms"
                : $"冗余同步定时器时间间隔设置失败，请检查Config/Redundancy/SyncInterval属性设置。冗余同步间隔仍为初始值500ms。");
        }

        public static int SyncInterval => _syncInterval; //Add by Dongmin 20191007

        private void StartSync()
        {
            // 启动同步计时器
            if (_syncTimer == null)
            {
                _syncTimer = new Timer(new TimerCallback(OnDataSyncToPartner), null, 0, _syncInterval);
            }

        }
        private void StopSync()
        {
            // 停止同步计时器
            _syncTimer = null;

        }

        object DataSyncLocker = new object();
        // 主站调用 Dongmin 20191005
        public void OnDataSyncToPartner(object sender)
        {
            //夏 2019年8月22日 17:46:53  判断host是否为null
            if (_host == null)
                return;

            // David 20191005         
            if (!PartnerAlive)
                return;

            PartnerProxy _proxy = (PartnerProxy)_host.GetProxy();
            if (_proxy != null)
            {
                //_proxy.ExchangeData("testresource","testdata"); //测试

                foreach (IResource resource in ResourceManager.GetAllResources())
                {
                    if (resource is IRedundancy)
                    {
                        IRedundancy res = (IRedundancy)resource;

                        if (res.NeedDataSync)
                        {
                            string dataToSend = res.BuildSyncData();

                            //LOG.Debug(string.Format("RedundancyServer:向从站同步数据 resource:{0},data:{1}", resource.ResourceName, dataToSend));
                            _proxy.ExchangeData(resource.ResourceName, dataToSend);
                        }
                    }
                }
            }



        }

        public void OnDataSyncFromPartner(string ResourceName, string ExchangeData)
        {
            if (Mode != RedundancyMode.Slave) //Dongmin 20191109
            {
                return;
            }

            //LOG.Debug(string.Format("RedundancyServer:从主站资源{0}同步数据",ResourceName));

            try
            {
                /*var sw=new Stopwatch();
                sw.Start();*/

                IResource resource = ResourceManager.GetResource(ResourceName);

                if (resource is IRedundancy)
                {
                    IRedundancy res = (IRedundancy)resource;

                    if (res.NeedDataSync)
                    {
                        res.ExtractSyncData(ExchangeData);
                    }
                }

                /*sw.Stop();
                LOG.Info($"冗餘数据同步執行時間：{sw.ElapsedMilliseconds}ms");*/

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获得主站资源：{0}出错：{1}", ResourceName, ex));
            }
        }
        #endregion

    }

    public enum RedundancyMode
    {
        Unknown = 0,
        Master = 1,
        Slave = 2,
    }
}
