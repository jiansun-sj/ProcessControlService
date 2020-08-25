using Newtonsoft.Json;
using ProcessControlService.Contracts;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.DBUtil;
using ProcessControlService.ResourceFactory.MemoryAndCpuUtil;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using IResourceService = ProcessControlService.Contracts.IResourceService;
using ResourceManager = ProcessControlService.ResourceFactory.ResourceManager;

namespace ProcessControlService.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class ResourceWebService : IResourceService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ResourceWebService));

        // private ProcessFactory pc_controller;
        public ResourceWebService()
        {
            // pc_controller = pc;
            //AddClientEventHandler(ClientEventType.HeartBeat, HeartBeat);
            AddClientEventHandler(ClientEventType.MissHeartBeat, HeartbeatTimeout);
            AddClientEventHandler(ClientEventType.Disconnect, ClientDisconnect);
        }

        #region "心跳"

        private readonly HeartBeatManager _hbManager = new HeartBeatManager(); //心跳管理对象

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

                case ClientEventType.None:
                    break;
                case ClientEventType.HeartBeat:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// 心跳处理
        /// </summary>
        public void HeartBeat(object sender, ClientEventArg arg)
        {
            Log.Error("客户端心跳处理");
        }

        /// <summary>
        /// 客户端连接超时处理 callback
        /// </summary>
        public void HeartbeatTimeout(object sender, ClientEventArg arg)
        {
            var clientId = arg.ClientID;
            Log.Error($"客户端:{clientId}连接超时");
        }

        /// <summary>
        /// 客户端下线处理 callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        public void ClientDisconnect(object sender, ClientEventArg arg)
        {
            var clientId = arg.ClientID;
            Log.Error($"客户端:{clientId}下线");

        }

        #endregion

        #region "接口实现"

        public void ConnectResourceHost(string clientId)
        {
            Log.Debug($"客户端{clientId}连接上线.");

            var clientHostName = OperationContext.Current.Channel.RemoteAddress.ToString();

            // OperationContext.Current.Channel.c

            _hbManager.AddClient(clientId);
        }

        public void DisconnectResourceHost(string clientId)
        {
            Log.Debug($"客户端{clientId}连接下线.");

            _hbManager.RemoveClient(clientId);
        }

        public void HeartBeat(string clientId)
        {
            //LOG.Debug(string.Format("客户端{0}发来心跳信号.", ClientID));

            _hbManager.HeartBeat(clientId);
        }

        public List<ResourceServiceModel> GetResourceService(string resourceName)
        {
            try
            {
                Log.Debug($"客户端{resourceName}获取接口服务列表.");

                var resource = ResourceManager.GetResource(resourceName);
                var export = resource.GetExportService();

                var result = export.GetExportServices();
                return result;
            }
            catch (Exception ex)
            {
                Log.Error("ResourceService异常报错" + ex);
                return null;
            }

        }

        public List<string> GetResources(string resourceType)
        {
            try
            {
                Log.Debug($"客户端获取资源列表.类型：{resourceType}");

                return ResourceManager.GetResourceNames(resourceType);
            }
            catch (Exception ex)
            {
                Log.Error("ResourceService异常报错" + ex.ToString());
                return null;
            }

        }

        public List<string> GetAllResources()
        {
            try
            {
                Log.Debug("客户端获取资源列表.");

                return ResourceManager.GetAllResourceNames();
            }
            catch (Exception ex)
            {
                Log.Error("ResourceService异常报错" + ex.ToString());
                return null;
            }
        }

        public string CallResourceService(string resourceName, string serviceName, string strParameter)
        {
            try
            {
                //LOG.Debug(string.Format("客户端{0}调用接口{1}.", ResourceName, ServiceName));

                var resource = ResourceManager.GetResource(resourceName);
                var export = resource.GetExportService();

                var strResult = export.CallExportService(serviceName, strParameter);
                return strResult;
            }
            catch (Exception ex)
            {
                Log.Error($"调用ResourceService异常报错: 资源名：{resourceName},服务方法：{serviceName},参数：{strParameter}" + ex);
                return null;
            }

        }

        public Task<string> CallResourceServiceAsc(string resourceName, string serviceName, string strParameter)
        {
            throw new NotImplementedException();
        }

        public string CallResourceServiceParams(string resourceName, string serviceName, params object[] parameters)
        {
            var serializeObject = "";
            try
            {
                var serviceParameterModels = new List<ServiceParameterModel>();

                if (parameters != null)
                    serviceParameterModels.AddRange(parameters.Select((t, i) => new ServiceParameterModel
                        {Name = $"Item{i}", Value = t.ToString(), Type = t.GetType().Name}));

                serializeObject = JsonConvert.SerializeObject(serviceParameterModels);

                var strResult = CallResourceService(resourceName, serviceName, serializeObject);
                return strResult;
            }
            catch (Exception ex)
            {
                Log.Error($"调用ResourceService异常报错: 资源名：{resourceName},服务方法：{serviceName},参数：{serializeObject}" + ex);
                return null;
            }
        }

        public Task<string> CallResourceServiceParamsAsc(string resourceName, string serviceName, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 数据库访问相关

        public DataSet GetDataSet(string cnnName, CommandType ct, string sql)
        {
            DataBaseHelper dataBaseHelper = new DataBaseHelper(cnnName);
            return dataBaseHelper.GetDataSet(ct, sql);
        }

        public int ExecuteNonQuery(string cnnName, CommandType ct, string sql)
        {
            DataBaseHelper dataBaseHelper = new DataBaseHelper(cnnName);
            return dataBaseHelper.ExecuteNonQuery(ct, sql);
        }

        public string ExecuteOne(string conStr, string sql)
        {
            return new DataBaseHelper(conStr).ExecuteOne(sql);
        }

        public List<MemoryAndCpuData> GetMemoryAndCpuData(DateTime checkDate)
        {
            return MemoryMonitor.GetMemoryAndCpuData(checkDate);
        }
        
        #endregion

        #region 实现IServiceCall接口
        public string CallService(string serviceCallParameter)
        {
            var resourceMethodCallModel = JsonConvert.DeserializeObject<ResourceMethodCallModel>(serviceCallParameter);

            try
            {
                return CallResourceService(resourceMethodCallModel.ResourceName, resourceMethodCallModel.MethodName, JsonConvert.SerializeObject(resourceMethodCallModel.Parameters));
            }
            catch (Exception e)
            {

                // Log.DebugFormat("调用CallService出错{0}，调用信息是:{1}", e.Message, serviceCallParameter);
                return null;
            }
        }

        public bool IsMaster()
        {
            return ResourceManager.GetRedundancyMode() == RedundancyMode.Master;
        }

        #endregion
    }
}
