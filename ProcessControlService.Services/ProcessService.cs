// ==================================================
// 文件名：ProcessService.cs
// 创建时间：2020/01/02 18:08
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/03/10 18:08
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using ProcessControlService.Contracts;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Processes;

namespace ProcessControlService.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, AutomaticSessionShutdown = false,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ProcessService : IProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessService));

        public ProcessService()
        {
            AddClientEventHandler(ClientEventType.MissHeartBeat, HeartbeatTimeout);
            AddClientEventHandler(ClientEventType.Disconnect, ClientDisconnect);
        }

        #region "心跳"

        private readonly HeartBeatManager _hbManager = new HeartBeatManager(); //心跳管理对象

        /// <summary>
        ///     定阅事件
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
        ///     心跳处理
        /// </summary>
        public void HeartBeat(object sender, ClientEventArg arg)
        {
            Log.Error("客户端心跳处理");
        }

        /// <summary>
        ///     客户端连接超时处理 callback
        /// </summary>
        public void HeartbeatTimeout(object sender, ClientEventArg arg)
        {
            var clientId = arg.ClientID;
            Log.Error($"客户端:{clientId}连接超时");
        }

        /// <summary>
        ///     客户端下线处理 callback
        /// </summary>
        public void ClientDisconnect(object sender, ClientEventArg arg)
        {
            var clientId = arg.ClientID;
            Log.Error($"客户端:{clientId}下线");
        }

        #endregion

        #region "接口实现"

        public void ConnectProcessHost(string clientId)
        {
            Log.Debug($"客户端{clientId}连接上线.");

            var clientHostName = OperationContext.Current.Channel.RemoteAddress.ToString();

            _hbManager.AddClient(clientId);
        }

        public void DisconnectProcessHost(string clientId)
        {
            Log.Debug($"客户端{clientId}连接下线.");

            _hbManager.RemoveClient(clientId);
        }

        public void HeartBeat(string clientId)
        {
            _hbManager.HeartBeat(clientId);
        }

        public Dictionary<string, string> ListProcessInParameters(string processName)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                Log.Error($"获取Process输入参数失败,[{e.Message}]");
                return null;
            }
        }

        public short GetProcessStep(string name)
        {
            try
            {
                var process = (Process) ResourceManager.GetResource(name);

                return 0; //process.GetProcessStepId();
            }
            catch (Exception ex)
            {
                Log.Error($"ProcessService:{name} 获取Step出错：{ex.Message}");
                return -1;
            }
        }

        public List<string> ReadStaticResourceNames()
        {
            try
            {
                var allResources = ResourceManager.GetAllResources();

                var resourceNames = new List<string>();

                resourceNames.AddRange(allResources.Where(a=>a.ResourceType!=nameof(Process)).Select(a=>a.ResourceName));

                return resourceNames;
            }
            catch (Exception e)
            {
               Log.Error($"读取非Process的静态资源名失败，异常为：{e.Message}.");

               return new List<string>();
            }
        }

        public void StartProcess(string name, Dictionary<string, string> containers, Dictionary<string, string> inParameters)
        {
            try
            {
                //获取Process资源,修改Process初始参数。
                var process = (Process) ResourceManager.GetResource(name);

                if (process==null)
                    throw new ArgumentNullException(name);

                process.ManualStart(inParameters, containers);
            }
            catch (Exception e)
            {
               Log.Error($"启动Process：【{name}】失败，异常为：{e.Message}.");   
            }
        }
        
        public List<ProcessInfoModel> GetProcessInfos()
        {
            var processList = ResourceManager.GetResources(nameof(Process)).Select(a => (Process) a);

            return processList.Select(resource => new ProcessInfoModel
            {
                ProcessName = resource.ResourceName, 
                ContainerNames =resource.GetContainerNames(), 
                TotalRunningTimes = resource.RunCounts,
                BreakCounts = resource.BreakCounts,
                RunningInstanceNumber = ProcessManagement.GetProcessInstancesNumber(resource.ProcessName),
            }).ToList();
        }
        
        public List<ProcessInstanceInfoModel> GetProcessInstanceInfoModels(string processName)
        {
            try
            {
                var processInstances = ProcessManagement.GetProcessInstances(processName);

                return processInstances.Select(processInstance =>
                    {
                        var currentStepInfo = processInstance.GetCurrentStep();

                        return new ProcessInstanceInfoModel
                        {
                            Index = processInstance.Pid,
                            ProcessName = processInstance.ProcessName,
                            CurrentStep = currentStepInfo.Name,
                            CurrentStepId = currentStepInfo.Id,
                            Container = currentStepInfo.Container,
                            ProcessInstanceParameters = processInstance.GetParametersValue()
                        };
                    })
                    .ToList();
            }
            catch (Exception e)
            {
                Log.Error($"获取Process实例观察参数失败，异常为：[{e.Message}]");
                return null;
            }
        }

        public List<ProcessInstanceRecord> ReadProcessInstanceRecords(string processName, int pageSize, DateTime startDate, DateTime endDate, int searchPage)
        {
            
            try
            {
                return ProcessRecordSqLiteUtil.ReadProcessRecord(processName, pageSize,startDate,endDate,searchPage);
            }
            catch (Exception e)
            {
                Log.Error($"获取Process历史执行记录失败，异常为：[{e.Message}]");
                return null;
            }
        }
        
        public Dictionary<short, List<string>> GetProcessAllStepsIdName(string name)
        {
            try
            {
                var process = (Process) ResourceManager.GetResource(name);
                
                return process.GetProcessAllStepsIdName();
            }
            catch (Exception ex)
            {
                Log.Error($"ProcessService:{name} GetProcessALLStepsIDName出错：{ex.Message}");
                return null;
            }
        }

/*
        public void StartProcess(string Name, Dictionary<string,string> InParameters)
        {
            try
            {
                Log.Info($"ProcessService:{Name} 手工启动");

                Process process = (Process)ResourceManager.GetResource(Name);

                // 循环写入参数值
                short index = 0;
                if (InParameters != null)
                {
                    foreach (string parName in InParameters.Keys)
                    {
                        process.SetParameterInString(parName, InParameters[parName]);
                        index++;
                    }
                }
                process.StartProcessInManualMode();
            }
            catch (Message ex)
            {
                Log.Error(string.Format("ProcessService:{0} 手工启动出错：{1}", Name, ex.Message));
            }
        }
*/

        public void StopProcessInstance(string pid)
        {
            try
            {
                var processInstance = ProcessManagement.ProcessInstanceManager.GetProcessInstance(pid);

                Log.Info($"ProcessService:【{processInstance.ProcessName}】，【{pid}】 手工停止");

                processInstance.StopProcess();
            }
            catch (Exception ex)
            {
                Log.Error($"ProcessService:{pid} 手工出错：{ex.Message}");
            }
        }

        public void SetProcessAuto(string name, bool autoRun)
        {
            try
            {
                Log.Info(autoRun
                    ? $"设置ProcessService:{name} 自动模式"
                    : $"设置ProcessService:{name} 手动模式");

                var process = (Process) ResourceManager.GetResource(name);
                process.AutoMode = autoRun;
            }
            catch (Exception ex)
            {
                Log.Error($"设置ProcessService:{name} 手自动模式出错：{ex.Message}");
            }
        }

        public bool GetProcessAuto(string name)
        {
            try
            {
                var process = (Process) ResourceManager.GetResource(name);
                return process.AutoMode;
            }
            catch (Exception ex)
            {
                Log.Error($"获取ProcessService:{name} 手自动模式出错：{ex.Message}");
                return false;
            }
        }

        public List<string> ListProcessNames()
        {
            try
            {
                var processes = ResourceManager.GetResourceNames("process");
                return processes;
            }
            catch (Exception ex)
            {
                Log.Error($"获取过程列表错误：{ex}");
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