using System;
using System.Collections.Generic;
using System.Threading;
using log4net;

namespace ProcessControlService.Services
{
    /// <summary>
    /// 心跳管理类型
    /// </summary>
    public class HeartBeatManager
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(MachineService));

        //心跳记录
        private Dictionary<string, DateTime> _hbRecord = new Dictionary<string, DateTime>();
        //丢失心跳事件
        private event EventHandler<ClientEventArg> _missingHBHandlers;
        //private event EventHandler _missingHBHandlers;
        //心跳事件
        //private event EventHandler<ClientEventArg> _hbHandlers;   //暂时没用，Robin注释于20180418
        //断线事件
        private event EventHandler<ClientEventArg> _disconnectHandlers;
        //检查心跳的定时器
        private Timer _timer;
        //2秒检查一次心跳
        private static readonly long _checkInterval = 2*1000;
        //15秒得不到心跳消息则视为心跳丢失
        public static readonly TimeSpan HBTimeout = TimeSpan.FromSeconds(15);
        private static object _hbRecordLocker = new object(); 
        /// <summary>
        /// 构造方法
        /// </summary>
        public HeartBeatManager()
        {
            _timer = new Timer(new TimerCallback(CheckHeartBeat), null, 0, _checkInterval);
        }

        /// <summary>
        /// 订阅丢失心跳事件
        /// </summary>
        /// <param name="handler">事件处理方法</param>
        public void AddMissingHBHandler(EventHandler<ClientEventArg> handler)
        {
            if (_missingHBHandlers == null)
                _missingHBHandlers = new EventHandler<ClientEventArg>(handler);
            else
                _missingHBHandlers += handler;
        }

        /// <summary>
        /// 订阅心跳事件
        /// </summary>
        /// <param name="handler">事件处理方法</param>
        //public void AddHBHandler(EventHandler<ClientEventArg> handler)
        //{
        //    if (_hbHandlers == null)
        //        _hbHandlers = new EventHandler<ClientEventArg>(handler);
        //    else
        //        _hbHandlers += handler;
        //}

        /// <summary>
        /// 订阅断线事件 -- Dongmin 20180310
        /// </summary>
        /// <param name="handler">事件处理方法</param>
        public void AddDisconnectHandler(EventHandler<ClientEventArg> handler)
        {
            if (_disconnectHandlers == null)
                _disconnectHandlers = new EventHandler<ClientEventArg>(handler);
            else
                _disconnectHandlers += handler;
        }

        /// <summary>
        /// 增加客户端
        /// </summary>
        /// <param name="ClientID"></param>
        public void AddClient(string ClientID)
        {
            lock (_hbRecordLocker)
            {
                if (_hbRecord.ContainsKey(ClientID))
                    _hbRecord[ClientID] = DateTime.Now;
                else
                    _hbRecord.Add(ClientID, DateTime.Now);
            }
        }

        /// <summary>
        /// 删除客户端
        /// </summary>
        /// <param name="CientID"></param>
        public void RemoveClient(string ClientID)
        {
            lock (_hbRecordLocker)
            {
                _hbRecord.Remove(ClientID);
            }
        }
       
        /// <summary>
        /// 检查心跳
        /// </summary>
        private void CheckHeartBeat(object sender)
        {
            lock (_hbRecordLocker)
            {
                try
                {
                    string removedClient = null;
                    foreach (string client in _hbRecord.Keys)
                    {
                        DateTime time = _hbRecord[client];
                        if (DateTime.Now.Subtract(time) > HBTimeout &&
                            _missingHBHandlers != null)
                        {
                            //这里出发丢失心跳事件
                            ClientEventArg arg = new ClientEventArg(client, ClientEventType.MissHeartBeat);
                            _missingHBHandlers(this, arg);

                            removedClient = client;

                        }
                    }
                    if (removedClient != null)
                    {

                        //这里发出客户端下线事件
                        ClientEventArg arg = new ClientEventArg(removedClient, ClientEventType.Disconnect);
                        _disconnectHandlers(this, arg);

                        RemoveClient(removedClient);
                        LOG.Error(string.Format("删除客户端:{0}", removedClient));

                    }
                }
                catch (Exception ex)
                {

                    LOG.Error(string.Format("检测心跳出错：{0}",ex.Message));
                }
                
            }
        }
        /// <summary>
        /// 触发心跳事件
        /// </summary>
        /// <param name="bedNo">床号</param>
        /// <param name="time">心跳时间</param>
        /// <param name="status">客户端状态</param>
        public void HeartBeat(string ClientID)
        {
            _hbRecord[ClientID] = DateTime.Now;
        }
    }
}
