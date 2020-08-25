using log4net;
using ProcessControlService.Contracts;
using System;
using System.ServiceModel;

/// <summary>
/// 资源WCF接口 Dongmin 20180221
/// </summary>
namespace ProcessControlService.WCFClients
{
    /// <summary>
    /// 系统管理服务代理，采用单工模式
    /// Created by Dongmin,2019/11/9
    /// </summary>
    public class AdminProxy : ClientBase<IAdmin>, IAdmin, IProxyConnection, IDisposable
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(PartnerProxy));

        private string CLIENT_ID;

        /// <summary>
        /// 外部创建通信代理对象
        /// </summary>
        /// <param name="RemoteAddress">远程地址,如"net.tcp://localhost:12005/MachineService"</param>
        /// <returns></returns>
        public static AdminProxy Create(string RemoteAddress)
        {
            EndpointAddress edpTcp = new EndpointAddress(RemoteAddress);

            // 创建Binding  
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.Security.Mode = SecurityMode.None;
            //myBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            //myBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            AdminProxy client = new AdminProxy(myBinding, edpTcp);

            return client;
        }

        public AdminProxy(System.ServiceModel.Channels.Binding binding, EndpointAddress edpAddr)
            : base(binding, edpAddr)
        {
            CLIENT_ID = string.Format("{0}@{1}", GetType().Name, System.Net.Dns.GetHostName());
        }

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
                    Close();// 关闭WCF连接
                }
                //清理非托管资源
            }
            _isDisposed = true;
        }

        private bool _isDisposed;

        ~AdminProxy()
        {
            Dispose(false);//释放非托管资源，托管资源由终极器自己完成了
        }
        #endregion

        #region "IHostConnection接口实现"
        //private bool _connected;
        public bool Connected =>
                //return _connected && (State == CommunicationState.Opened || State == CommunicationState.Opening);
                //return (State == CommunicationState.Opened || State == CommunicationState.Opening);
                (State == CommunicationState.Opened);

        public bool Connect()
        {
            try
            {
                //Open();
                ConnectResourceHost(CLIENT_ID);
                //if (Connected)
                //{
                //    LOG.Info(string.Format("成功连接到冗余伙伴"));
                //}
                //else
                //{
                //    LOG.Info(string.Format("没有连接到冗余伙伴"));
                //}

                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("连接到冗余伙伴出错：{0}", ex.Message));
                return false;
            }
        }

        public void Disconnect()
        {
            if (State == CommunicationState.Opened)
            {
                DisconnectResourceHost(CLIENT_ID);
            }

            //if (State == CommunicationState.Faulted)
            //    Abort();
            //else
            //    Close();
        }

        public bool SendHeartBeat()
        {
            try
            {
                HeartBeat(CLIENT_ID);
                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("发送心跳出错：{0}", ex.Message));
                return false;
            }
        }

        #endregion

        #region "接口实现"
        public void ConnectResourceHost(string ClientID)
        {
            try
            {
                base.Channel.ConnectResourceHost(ClientID);
            }
            catch (Exception)
            {


            }

        }

        public void DisconnectResourceHost(string ClientID)
        {
            try
            {
                base.Channel.DisconnectResourceHost(ClientID);
            }
            catch (Exception)
            {


            }

        }

        public void HeartBeat(string ClientID)
        {
            try
            {
                if (base.State == CommunicationState.Opened)
                {
                    base.Channel.HeartBeat(ClientID);
                }
            }
            catch (Exception)
            {
                //LOG.Error(string.Format("心跳发送失败"));                
            }

        }

        public short GetRedundancyMode()
        {
            try
            {
                if (base.State == CommunicationState.Opened)
                {
                    return base.Channel.GetRedundancyMode();
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                LOG.Error("连接服务端出错，无法调用资源服务");
                return 0;
            }


        }

        public void ToggleRedundancyMode()
        {
            try
            {
                if (base.State == CommunicationState.Opened)
                {
                    base.Channel.ToggleRedundancyMode();
                }
            }
            catch (Exception)
            {
                LOG.Error("连接服务端出错，无法调用资源服务");
            }


        }
        public bool IsMaster()
        {
            throw new NotSupportedException();
        }

        #endregion

    }
}
