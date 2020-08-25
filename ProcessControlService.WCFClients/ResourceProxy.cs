// ==================================================
// 文件名：ResourceProxy.cs
// 创建时间：2020/01/02 11:48
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/01/05 11:48
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using ProcessControlService.Contracts;
using ProcessControlService.Contracts.ProcessData;


namespace ProcessControlService.WCFClients
{
    /// <summary>
    /// 资源WCF接口 Dongmin 20180221
    /// </summary>
    public class ResourceProxy : ClientBase<IResourceService>, IResourceService, IProxyConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourceProxy));

        private readonly string _clientId;

        public ResourceProxy(Binding binding, EndpointAddress edpAddress)
            : base(binding, edpAddress)
        {
            _clientId = $"{GetType().Name}@{Dns.GetHostName()}";
        }

        /// <summary>
        ///     外部创建通信代理对象
        /// </summary>
        /// <param name="remoteAddress">远程地址,如"net.tcp://localhost:12005/MachineService"</param>
        /// <returns></returns>
        public static ResourceProxy Create(string remoteAddress)
        {
/*            EndpointAddress edpTcp = new EndpointAddress(RemoteAddress);

            // 创建Binding  
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.Security.Mode = SecurityMode.None;
            //myBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            //myBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            ResourceProxy client = new ResourceProxy(myBinding, edpTcp);

            return client;*/

            /*code2.1中的代码*/
            var edpTcp = new EndpointAddress(remoteAddress);

            // 创建Binding  
            var myBinding = new NetTcpBinding
            {
                Security = {Mode = SecurityMode.None},
                OpenTimeout = TimeSpan.FromSeconds(20),
                ReceiveTimeout = TimeSpan.FromSeconds(20),
                SendTimeout = TimeSpan.FromSeconds(20),
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue
            };
            
            //myBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            //myBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            var client = new ResourceProxy(myBinding, edpTcp);

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

        ~ResourceProxy()
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
                //Open();
                ConnectResourceHost(_clientId);
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
                DisconnectResourceHost(_clientId);

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

        public void ConnectResourceHost(string clientId)
        {
            try
            {
                Channel.ConnectResourceHost(clientId);
            }
            catch (Exception e)
            {
                Log.Error($"连接资源代理出错,{e.Message}");
            }
        }

        public void DisconnectResourceHost(string clientId)
        {
            try
            {
                Channel.DisconnectResourceHost(clientId);
            }
            catch (Exception e)
            {
                Log.Error($"断开资源代理出错，{e.Message}");
            }
        }

        public void HeartBeat(string clientId)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    Channel.HeartBeat(clientId);
            }
            catch (Exception e)
            {
                Log.Error($"心跳HeartBeat出错，{e.Message}");
            }
        }

        public List<ResourceServiceModel> GetResourceService(string resourceName)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetResourceService(resourceName);
            }
            catch (Exception)
            {
                Log.Error("连接服务端出错，无法获取资源服务名");
            }

            return null;
        }

        public List<string> GetResources(string resourceType)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetResources(resourceType);
            }
            catch (Exception)
            {
                Log.Error("连接服务端出错，无法调用资源服务");
            }

            return null;
        }

        public List<string> GetAllResources()
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetAllResources();
            }
            catch (Exception)
            {
                Log.Error("连接服务端出错，无法调用资源服务");
            }

            return null;
        }
        
        public string CallResourceService(string resourceName, string serviceName, string strParameter)
        {
            try
            {
                return Channel.CallResourceService(resourceName, serviceName, strParameter);
            }
            catch (Exception ex)
            {
                Log.Error("连接服务端出错，无法调用资源服务" + ex.StackTrace + "\n" +
                          $"ResourceName: {resourceName} \n ServiceName: {serviceName} \n StrParameter: {strParameter}");
            }

            return null;
        }
        
        public async Task<string> CallResourceServiceAsc(string resourceName, string serviceName, string strParameter)
        {
            try
            {
                var task = await Channel.CallResourceServiceAsc(resourceName, serviceName, strParameter);

                return task;
            }
            catch (Exception ex)
            {
                Log.Error("连接服务端出错，无法调用资源服务" + ex.StackTrace + "\n" +
                          $"ResourceName: {resourceName} \n ServiceName: {serviceName} \n Parameters: {strParameter}");
                return null;
            }
        }

        public string CallResourceServiceParams(string resourceName, string serviceName, params object[] parameters)
        {
            try
            {
                return Channel.CallResourceServiceParams(resourceName, serviceName, parameters);
            }
            catch (Exception ex)
            {
                Log.Error("连接服务端出错，无法调用资源服务" + ex.StackTrace + "\n" +
                          $"ResourceName: {resourceName} \n ServiceName: {serviceName} \n Parameters: {parameters}");
            }

            return null;
        }

        public async Task<string> CallResourceServiceParamsAsc(string resourceName, string serviceName, params object[] parameters)
        {
            try
            {
                return await Channel.CallResourceServiceParamsAsc(resourceName, serviceName, parameters);
            }
            catch (Exception ex)
            {
                Log.Error("连接服务端出错，无法调用资源服务" + ex.StackTrace + "\n" +
                          $"ResourceName: {resourceName} \n ServiceName: {serviceName} \n Parameters: {parameters}");
            }

            return null;
        }

        public DataSet GetDataSet(string cnnName, CommandType ct, string sql)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetDataSet(cnnName, ct, sql);
            }
            catch (Exception ex)
            {
                Log.Error("连接服务端出错，无法调用资源服务" + $"{ex.Message}");
            }

            return null;
        }

        public int ExecuteNonQuery(string cnnName, CommandType ct, string sql)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.ExecuteNonQuery(cnnName, ct, sql);
            }
            catch (Exception ex)
            {
                Log.Error("连接服务端出错，无法调用资源服务"+$"{ex.Message}");
            }

            return -1;
        }

        public string ExecuteOne(string conStr, string sql)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.ExecuteOne(conStr, sql);
            }
            catch (Exception)
            {
                Log.Error("连接服务端出错，无法调用资源服务");
            }
            
            return null;
        }

        public List<MemoryAndCpuData> GetMemoryAndCpuData(DateTime checkDate)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    return Channel.GetMemoryAndCpuData(checkDate);
            }
            catch (Exception)
            {
                Log.Error("获取程序指定日期内存占用和Cpu使用率的情况。");
            }

            return null;
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