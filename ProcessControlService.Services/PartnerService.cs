using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ProcessControlService.Contracts;
using log4net;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.Services
{
     [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PartnerService : IPartner
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(PartnerService));

       // private ProcessFactory pc_controller;

        public PartnerService()
        {
           // pc_controller = pc;

            //AddClientEventHandler(ClientEventType.HeartBeat, HeartBeat);
            AddClientEventHandler(ClientEventType.MissHeartBeat, HeartbeatTimeout);
            AddClientEventHandler(ClientEventType.Disconnect, ClientDisconnect);

        }

        #region "心跳"
        private HeartBeatManager _hbManager = new HeartBeatManager(); //心跳管理对象

        /// <summary>
        /// 定阅事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="handler">事件处理方法</param>
        public void AddClientEventHandler(ClientEventType type, EventHandler<ClientEventArg> handler)
        {
            switch (type)
            {

                case ClientEventType.MissHeartBeat:
                    {
                        _hbManager.AddMissingHBHandler(handler);
                        break;
                    }
                //case ClientEventType.HeartBeat:
                //    {
                //        _hbManager.AddHBHandler(handler);
                //        break;
                //    }
                case ClientEventType.Disconnect:
                    {
                        _hbManager.AddDisconnectHandler(handler);
                        break;
                    }

            }
        }

        /// <summary>
        /// 心跳处理
        /// </summary>
        public void HeartBeat(object sender, ClientEventArg arg)
        {
            LOG.Error(string.Format("客户端心跳处理"));
        }

        /// <summary>
        /// 客户端连接超时处理 callback
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        public void HeartbeatTimeout(object sender, ClientEventArg arg)
        {
            string ClientID = arg.ClientID;
            LOG.Error(string.Format("客户端:{0}连接超时", ClientID));

        }

        /// <summary>
        /// 客户端下线处理 callback
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        public void ClientDisconnect(object sender, ClientEventArg arg)
        {
            string ClientID = arg.ClientID;
            LOG.Error(string.Format("客户端:{0}下线", ClientID));

            Redundancy _redundancy = ResourceManager.GetRedundancy();
            _redundancy.OnDisconnectFromPartner();

        }

        #endregion

        #region "接口实现"

        public void ConnectResourceHost(string ClientID)
        {
            //LOG.Debug(string.Format("冗余伙伴{0}连接上线.", ClientID));

            string ClientHostName = OperationContext.Current.Channel.RemoteAddress.ToString();

            _hbManager.AddClient(ClientID);

            Redundancy _redundancy = ResourceManager.GetRedundancy();
            _redundancy.OnConnectFromPartner();
        }

        public void DisconnectResourceHost(string ClientID)
        {
            //LOG.Debug(string.Format("冗余伙伴{0}连接下线.", ClientID));

            _hbManager.RemoveClient(ClientID);

            //Redundancy _redundancy = ResourceManager.GetRedundancy();
            //_redundancy.OnDisconnectPartner();
        }

        public void HeartBeat(string ClientID)
        {
            //LOG.Debug(string.Format("客户端{0}发来心跳信号.", ClientID));

            _hbManager.HeartBeat(ClientID);
        }

        public Int16 GetPartnerMode()
        {
            Redundancy _redundancy = ResourceManager.GetRedundancy();
            return Convert.ToInt16(_redundancy.Mode);
        }

        //public void TogglePartnerMode()
        //{
        //    Redundancy _redundancy = ResourceManager.GetRedundancy();
        //    _redundancy.TogglePartnerMode();
        //}



        public long GetPartnerRunTime()
        {
            Redundancy _redundancy = ResourceManager.GetRedundancy();
            return Convert.ToInt64(_redundancy.RunTime);
        }

        public void ExchangeData(string ResourceName, string ExchangeData)
        {
            Redundancy _redundancy = ResourceManager.GetRedundancy();
            _redundancy.OnDataSyncFromPartner(ResourceName, ExchangeData);
        }

        public bool IsMaster()
        {
            return ResourceManager.GetRedundancyMode() == RedundancyMode.Master;
        }

        #endregion



    }
}
