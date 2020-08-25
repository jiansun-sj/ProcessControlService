// ==================================================
// 文件名：ProcessProxy.cs
// 创建时间：2020/01/02 15:06
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/03/06 15:06
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using log4net;
using ProcessControlService.Contracts;
using ProcessControlService.Contracts.ProcessData;

namespace ProcessControlService.WCFClients
{
    /// <summary>
    ///     过程服务代理，采用单工模式
    ///     Created by Dongmin,2017/05/11
    /// </summary>
    public class ProcessProxy : ClientBase<IProcess>, IProcess, IProxyConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessProxy));

        private readonly string _clientId;

        public ProcessProxy(Binding binding, EndpointAddress edpAddr)
            : base(binding, edpAddr)
        {
            _clientId = $"{GetType().Name}@{Dns.GetHostName()}";
        }

        /// <summary>
        ///     外部创建通信代理对象
        /// </summary>
        /// <param name="remoteAddress" />
        /// <returns></returns>
        public static ProcessProxy Create(string remoteAddress)
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
            var client = new ProcessProxy(myBinding, edpTcp);

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

        ~ProcessProxy()
        {
            Dispose(false); //释放非托管资源，托管资源由终极器自己完成了
        }

        #endregion

        #region "IHostConnection接口实现"

        //private bool _connected;
        public bool Connected => State == CommunicationState.Opened || State == CommunicationState.Opening;


        public bool Connect()
        {
            try
            {
                //Open();
                ConnectProcessHost(_clientId);
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
                DisconnectProcessHost(_clientId);
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

        public void ConnectProcessHost(string clientId)
        {
            Channel.ConnectProcessHost(clientId);
        }

        public void DisconnectProcessHost(string clientId)
        {
            Channel.DisconnectProcessHost(clientId);
        }

        public void HeartBeat(string clientId)
        {
            Channel.HeartBeat(clientId);
        }

        /// <summary>
        ///     获取该Process当前StepID
        /// </summary>
        public short GetProcessStep(string name)
        {
            return Channel.GetProcessStep(name);
        }

        public void StartProcess(string name, Dictionary<string, string> containers,
            Dictionary<string, string> inParameters)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    Channel.StartProcess(name, containers, inParameters);
            }
            catch (Exception ex)
            {
                Log.Error("过程服务连接失败:" + ex);
            }
        }

        public void StopProcessInstance(string name)
        {
            try
            {
                if (State == CommunicationState.Opened)
                    Channel.StopProcessInstance(name);
            }
            catch (Exception ex)
            {
                Log.Error($"停止过程{name}失败:{ex}");
            }
        }

        public List<string> ReadStaticResourceNames()
        {
            try
            {
                return State == CommunicationState.Opened ? Channel.ReadStaticResourceNames() : new List<string>();
            }
            catch (Exception ex)
            {
                Log.Error($"读取非Process的资源名称失败:{ex}");
                return new List<string>();
            }
        }

        public void SetProcessAuto(string name, bool autoRun)
        {
            Channel.SetProcessAuto(name, autoRun);
        }

        public bool GetProcessAuto(string name)
        {
            return Channel.GetProcessAuto(name);
        }


        /// <summary>
        ///     获取可用的process列表
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public List<string> ListProcessNames()
        {
            return Channel.ListProcessNames();
        }

        public List<ProcessInfoModel> GetProcessInfos()
        {
            try
            {
                return Channel.GetProcessInfos();
            }
            catch (Exception e)
            {
                Log.Error($"获取Process基本信息失败，异常为:[{e.Message}]");
                return null;
            }
        }

        public List<ProcessInstanceInfoModel> GetProcessInstanceInfoModels(string processName)
        {
            try
            {
                return Channel.GetProcessInstanceInfoModels(processName);
            }
            catch (Exception e)
            {
                Log.Error($"获取ProcessInstance详情失败，异常为：[{e.Message}]");
                return null;
            }
        }

        public List<ProcessInstanceRecord> ReadProcessInstanceRecords(string processName, int pageSize, DateTime startDate, DateTime endDate, int searchPage)
        {
            try
            {
                return Channel.ReadProcessInstanceRecords(processName, pageSize, startDate, endDate, searchPage);
            }
            catch (Exception e)
            {
                Log.Error($"获取Process历史执行记录失败，异常为：[{e.Message}]");
                return null;
            }
        }
        
        /// <summary>
        ///     获取该process所有的StepID和Name
        /// </summary>
        public Dictionary<short, List<string>> GetProcessAllStepsIdName(string name)
        {
            return Channel.GetProcessAllStepsIdName(name);
        }

        public Dictionary<string, string> ListProcessInParameters(string processName)
        {
            return Channel.ListProcessInParameters(processName);
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