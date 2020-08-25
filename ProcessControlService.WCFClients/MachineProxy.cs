// ==================================================
// 文件名：MachineProxy.cs
// 创建时间：2020/04/12 16:05
// ==================================================
// 最后修改于：2020/05/03 16:05
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using log4net;
using ProcessControlService.Contracts;

// 修改记录
// 20170525 David  1.修改了CLIENT_ID的赋值方式
//              2.修改了Dispose的实现方式
//                 3.增加连接属性

//                 
namespace ProcessControlService.WCFClients
{
    /// <summary>
    ///     机器服务代理，采用单工模式
    ///     Created by Dongmin,2017/05/11
    /// </summary>
    public class MachineProxy : ClientBase<IMachine>, IMachine, IProxyConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MachineProxy));

        private readonly string _clientId;

        public MachineProxy(Binding binding, EndpointAddress edpAddr)
            : base(binding, edpAddr)
        {
            _clientId = $"{GetType().Name}@{Dns.GetHostName()}";
        }

        /// <summary>
        ///     外部创建通信代理对象
        /// </summary>
        /// <param name="remoteAddress">远程地址,如"net.tcp://localhost:12005/MachineService"</param>
        /// <returns></returns>
        public static MachineProxy Create(string remoteAddress)
        {
            var edpTcp = new EndpointAddress(remoteAddress);

            // 创建Binding  
            var myBinding = new NetTcpBinding
            {
                OpenTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(10),
                Security = {Mode = SecurityMode.None},
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue
            };
            //myBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            //myBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            var client = new MachineProxy(myBinding, edpTcp);

            return client;
        }

        #region Dispose

        public void Dispose()
        {
            Dispose(true); ////释放托管资源
            GC.SuppressFinalize(this); //请求系统不要调用指定对象的终结器. //该方法在对象头中设置一个位，系统在调用终结器时将检查这个位
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed) //_isDisposed为false表示没有进行手动dispose
                if (disposing)
                    //清理托管资源
                    Close(); // 关闭WCF连接
            //清理非托管资源
            _isDisposed = true;
        }

        private bool _isDisposed;

        ~MachineProxy()
        {
            Dispose(false); //释放非托管资源，托管资源由终极器自己完成了
        }

        #endregion

        #region "IHostConnection接口实现"

        public bool Connected => State == CommunicationState.Opened;

        public bool Connect()
        {
            try
            {
                // Open();
                ConnectMachineHost(_clientId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"连接出错：{ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            if (State == CommunicationState.Opened)
                DisconnectMachineHost(_clientId);

            //if (State == CommunicationState.Faulted)
            //    Abort();
            //else
            //    Close();
        }

        public bool SendHeartBeat()
        {
            try
            {
                HeartBeat(_clientId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"发送心跳出错：{ex.Message}");
                return false;
            }
        }

        #endregion

        #region "接口实现"

        public void ConnectMachineHost(string clientId)
        {
            try
            {
                Channel.ConnectMachineHost(clientId);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void DisconnectMachineHost(string clientId)
        {
            try
            {
                Channel.DisconnectMachineHost(clientId);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void HeartBeat(string clientId)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    Channel.HeartBeat(clientId);
            }
            catch (Exception)
            {
                // ignored
            }
        }


        public MachineStatusModel GetMachineStatus(string machineName)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetMachineStatus(machineName);
            }
            catch (Exception)
            {
                Log.Error("连接服务端出错，无法获取机器状态");
            }

            return null;
        }


        /// <summary>
        ///     获取一个tag值
        /// </summary>
        public string GetMachineTag(string machineName, string tagName)
        {
            //加trycatch  防止hosting意外关闭时客户端崩溃
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetMachineTag(machineName, tagName);
            }
            catch (Exception ex)
            {
                Log.Error("服务端连接失败:" + ex);
            }

            return null;
        }


        /// <summary>
        ///     获取所有tag值
        ///     输入数组:Machine、Tag、""、""
        ///     返回数组:Machine、Tag、Value、AccessType
        /// </summary>
        public List<string[]> GetTagsValue(List<string[]> machineTag)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetTagsValue(machineTag);
            }
            catch (Exception ex)
            {
                Log.Error("服务端连接失败:" + ex);
            }

            return null;
        }

        /// <summary>
        ///     设置所有tag值
        ///     输入数组:Machine、Tag、Value
        ///     返回数组:Machine、Tag、Result
        /// </summary>
        public List<string[]> SetTagsValue(List<string[]> machineTag)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.SetTagsValue(machineTag);
            }
            catch (Exception ex)
            {
                Log.Error("服务端连接失败:" + ex);
            }

            return null;
        }

        /// <summary>
        ///     获取所有设备的数据源连接状态
        /// </summary>
        /// <returns></returns>
        public List<DataSourceConnectionModel> GetAllDataSourceConn()
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetAllDataSourceConn();
            }
            catch (Exception ex)
            {
                Log.Error("获取所有Machine的数据源状态失败:" + ex);
            }

            return null;
        }


        /// <summary>
        ///     获取机器数据源列表
        ///     输入参数:MachineName
        ///     返回列表:DatasourceName
        /// </summary>
        public List<string> GetMachineDataSourceName(string machineName)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetMachineDataSourceName(machineName);
            }
            catch (Exception ex)
            {
                Log.Error("服务端连接失败:" + ex);
            }

            return null;
        }

        /// <summary>
        ///     设置一个tag值
        /// </summary>
        public void SetMachineTag(string machineName, string tagName, string tagValue)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    Channel.SetMachineTag(machineName, tagName, tagValue);
            }
            catch (Exception ex)
            {
                Log.Error("服务端连接失败:" + ex);
            }
        }

        public Dictionary<string, string> RunMachineAction(string machineName, string actionName,
            Dictionary<string, string> inParameters)
        {
            //加trycatch  防止hosting意外关闭时客户端崩溃
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.RunMachineAction(machineName, actionName, inParameters);
            }
            catch (Exception ex)
            {
                Log.Error("服务端连接失败:" + ex);
            }

            return null;
        }

        public List<MachineAlarmModel> GetMachineAlarms(string machineName)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetMachineAlarms(machineName);
            }
            catch (Exception ex)
            {
                Log.Error("服务端连接失败:" + ex);
            }

            return null;
        }


        /// <summary>
        ///     获取所有可用的Machine
        /// </summary>
        public List<string> ListMachineNames() //获取可用机器列表
        {
            try
            {
                return Channel.ListMachineNames();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public List<DataSourceConnectionModel> ListMachineConnections(string machineName) //获取机器连接数据源状态
        {
            try
            {
                return Channel.ListMachineConnections(machineName);
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        ///     获取该Machine的Tag列表
        /// </summary>
        public List<string> ListMachineTags(string machineName) //获取可用的Tag列表
        {
            try
            {
                return Channel.ListMachineTags(machineName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> ListActionInParameters(string machineName, string actionName) //获得Action输入参数
        {
            try
            {
                return Channel.ListActionInParameters(machineName, actionName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> ListActionOutParameters(string machineName, string actionName) //获得Action输出参数
        {
            try
            {
                return Channel.ListActionOutParameters(machineName, actionName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsMaster()
        {
            try
            {
                return Connected && Channel.IsMaster();
            }
            catch (Exception)
            {
                Log.Error("连接服务端出错，无法调用资源服务");
                return false;
            }
        }

        #endregion
    }
}