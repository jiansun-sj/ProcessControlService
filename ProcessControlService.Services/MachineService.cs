using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ProcessControlService.Contracts;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MachineService : IMachine
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MachineService));

        //   private ProcessFactory ps_factory;

        private static object _locker = new object();


        public MachineService()
        {
            //  ps_factory = pc;

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

        public void ConnectMachineHost(string clientId)
        {
            Log.Debug($"客户端{clientId}连接上线.");

            //IMachineCallBack client = OperationContext.Current.GetCallbackChannel<IMachineCallBack>();

            //    string sessionid = OperationContext.Current.SessionId;//获取当前机器Sessionid--------------------------如果多个客户端在同一台机器，就使用此信息。
            var clientHostName = OperationContext.Current.Channel.RemoteAddress.ToString();
            //    //.Uri.Host;//获取当前机器名称-----多个客户端不在同一台机器上，就使用此信息。

            _hbManager.AddClient(clientId);
        }

        public void DisconnectMachineHost(string clientId)
        {
            Log.Debug($"客户端{clientId}连接下线.");

            _hbManager.RemoveClient(clientId);
        }

        public void HeartBeat(string clientId)
        {
            //LOG.Debug(string.Format("客户端{0}发来心跳信号.", ClientID));

            _hbManager.HeartBeat(clientId);
        }

        public MachineStatusModel GetMachineStatus(string machineName)
        {
            try
            {
                //LOG.Debug(string.Format("获取机器：{0}状态服务", MachineName));

                var machine = (Machine)ResourceManager.GetResource(machineName);

                var model = new MachineStatusModel(machine.ResourceName);

                var allStatusName = machine.ListStatusName(); // 获得机器状态列表

                // 更新机器状态
                machine.UpdateAllStatus();

                // 构建状态模型
                foreach (var statusName in allStatusName)
                {
                    model.SetStatus(statusName, machine.GetStatusString(statusName));
                    // 临时

                    //LOG.Debug(string.Format("{0}的值：{1}", statusName, machine.GetStatusString(statusName)));
                }

                return model;
            }
            catch (Exception ex)
            {
                Log.Error($"获取状态:服务调用出错：{ex.Message}");
                return null;
            }
        }

        public string GetMachineTag(string machineName, string tagName)
        {
            try
            {
                var machine = (Machine)ResourceManager.GetResource(machineName);

                var tag = machine.GetTag(tagName);
                if (tag != null && tag.Quality == Quality.Good)
                {
                    return tag.TagValue.ToString();
                }
                else
                {
                    return "";
                }
                //var strValue = machine.GetTag(tagName).TagValue.ToString();

                //Log.Debug($"读取机器：{machineName} Tag：{tagName}的值：{strValue}.");

                //return strValue;
            }
            catch (Exception ex)
            {
                Log.Error($"读取机器：{machineName} Tag：{tagName} 调用出错：{ex.Message}");
                return null;
            }
        }

        public List<string[]> GetTagsValue(List<string[]> machineTags)
        {
            try
            {
                Machine machine = null;
                foreach (var machineTag in machineTags)
                {
                    if (machine == null)
                    {
                        machine = (Machine)ResourceManager.GetResource(machineTag[0]);
                    }
                    else if (machine.ResourceName != machineTag[0])
                    {
                        machine = (Machine)ResourceManager.GetResource(machineTag[0]);
                    }
                    var tag = machine.GetTag(machineTag[1]);

                    if (tag.TagValue != null)
                    {
                        machineTag[2] = tag.TagValue.ToString();
                    }
                    else
                    {
                        machineTag[2] = "null";
                    }
                    machineTag[3] = tag.AccessType.ToString();

                    machineTag[4] = tag?.TagType;
                }
                return machineTags;

            }
            catch (Exception ex)
            {
                Log.Error($"机器接口GetTagsValue出错{ex.Message}");
                return null;
            }
        }
        public List<string[]> SetTagsValue(List<string[]> machineTags)
        {
            try
            {
                Machine machine = null;
                foreach (var machineTag in machineTags)
                {
                    if (null == machine)
                    {
                        machine = (Machine)ResourceManager.GetResource(machineTag[0]);
                    }
                    else if (machine.ResourceName != machineTag[0])
                    {
                        machine = (Machine)ResourceManager.GetResource(machineTag[0]);
                    }
                    var tag = machine.GetTag(machineTag[1]);
                    var result = machine.WriteTagWithStrValue(tag, machineTag[2]);
                    machineTag[2] = result.ToString();
                }
                return machineTags;

            }
            catch (Exception ex)
            {
                Log.Error($"机器接口SetTagsValue出错{ex.Message}");
                return null;
            }
        }

        public void SetMachineTag(string machineName, string tagName, string tagValue)
        {
            try
            {
                Log.Debug($"写入机器：{machineName} Tag：{tagName}的值：{tagValue}.");

                var machine = (Machine)ResourceManager.GetResource(machineName);

                // changed by David 20170320
                var tag = machine.GetTag(tagName);
                machine.WriteTagWithStrValue(tag, tagValue);
            }
            catch (Exception ex)
            {
                Log.Error($"写入机器：{machineName} Tag：{tagName} 调用出错：{ex.Message}");
            }
        }

        /// <summary>
        /// Created by Dongmin 2017/03/10
        /// Last modified by Dongmin 2017/03/10
        /// 运行机器的Action
        /// </summary>
        /// <param name="machineName"></param>
        /// <param name="actionName"></param>
        /// <param name="inParameters"></param>
        public Dictionary<string, string> RunMachineAction(string machineName, string actionName, Dictionary<string, string> inParameters)
        {
            try
            {
                /*
                                //LOG.Debug(string.Format("调用机器：{0} 动作：{1}.", MachineName, ActionName));

                                var machine = (Machine)ResourceManager.GetResource(machineName);

                                var allActionName = machine.ListActionNames(); // 获得机器Action列表
                                var outActionParameters = new Dictionary<string, string>();

                                // 构建Action模型
                                foreach (var action in allActionName)
                                {
                                    if (actionName.Equals(action))
                                    {
                                        var _action = machine.GetAction(action);
                                        // 循环写入参数值
                                        if (inParameters!=null)
                                            foreach (var strParameter in inParameters.Keys)
                                                _action.ActionInParameterManager.SetParameterValueInString(strParameter,
                                                    inParameters[strParameter]);

                                        // 执行机器动作
                                        _action.Execute();

                                        // 获取输出参数
                                        var parOutput = _action.GetOutParameters();

                                        foreach (var par in parOutput)
                                            outActionParameters.Add(par.Name, par.GetValueInString());
                                    }
                                }

                                return outActionParameters;
                */
                return new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                Log.Error($"执行机器{machineName}动作{actionName}服务调用出错：{ex.Message}");
                return null;
            }
        }

        public List<MachineAlarmModel> GetMachineAlarms(string machineName)
        {
            try
            {
                //LOG.Debug(string.Format("读取机器：{0} 报警.", MachineName));

                var machine = (Machine)ResourceManager.GetResource(machineName);
                var alarm = machine.GetAlarms();
                return alarm; // 编译不通过
                //return null;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取机器:{0}报警错误." + ex.Message, machineName));
                return null;
            }
        }

        //获取可用机器列表
        public List<string> ListMachineNames()
        {
            try
            {
                var machines = ResourceManager.GetResourceNames("machine");
                return machines;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取机器列表错误." + ex.Message));
                return null;
            }
        }

        //获取机器数据源连接状态，返回List<string[]>
        public List<DataSourceConnectionModel> GetAllDataSourceConn()
        {
            try
            {
                var dataSourceConnectionModels = new List<DataSourceConnectionModel>();

                var machines = ResourceManager.GetAllResources().OfType<Machine>();

                foreach (var machine in machines)
                {
                    var listDataSource = machine.ListDataSource();

                    var sourceConnectionModels = listDataSource.Select(a => new DataSourceConnectionModel
                    {
                        MachineName = machine.ResourceName,
                        Name = a.SourceName,
                        Type = a.GetType().Name,
                        Status = a.IsConnect() ? "Connected" : "Disconnected"
                    });

                    dataSourceConnectionModels.AddRange(sourceConnectionModels);
                }

                return dataSourceConnectionModels;
            }
            catch (Exception ex)
            {
                Log.Error($"机器接口GetMachineDataSourceConn出错{ex.Message}");
                return null;
            }
        }

        //获取机器数据源列表，返回List<string>
        public List<string> GetMachineDataSourceName(string machineName)
        {
            try
            {
                var machine = (Machine)ResourceManager.GetResource(machineName);
                return machine.ListDataSourceName();
            }
            catch (Exception ex)
            {
                Log.Error($"机器接口GetMachineDataSourceName出错{ex.Message}");
                return null;
            }
        }

        //获取机器连接数据源状态
        public List<DataSourceConnectionModel> ListMachineConnections(string machineName)
        {
            try
            {
                //LOG.Debug(string.Format("读取机器：{0} 数据源连接状态.", MachineName));

                var machine = (Machine)ResourceManager.GetResource(machineName);

                return machine.ListDataSource().Select(ds => new DataSourceConnectionModel
                {
                    Name = ds.SourceName,
                    Type = ds.GetType().Name,
                    Status = ds.IsConnect() ? "Connected" : "Disconnected"
                }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取机器:{0}数据源连接状态错误." + ex.Message, machineName));
                return null;
            }
        }

        //获取可用的Tag列表
        public List<string> ListMachineTags(string machineName)
        {
            try
            {
                //LOG.Debug(string.Format("读取机器：{0} Tag列表.", MachineName));

                var machine = (Machine)ResourceManager.GetResource(machineName);

                return machine.ListTagNames();

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("获取机器:{0}数据源连接状态错误." + ex.Message, machineName));
                return null;
            }
        }

        //获取Action输入参数列表，前一个是参数名，后一个是默认值 -- David 20170603
        // 返回值： Dict[0]:参数1名，
        //          Dict[1]:参数1类型，
        //          Dict[1]:参数1默认值，
        //          ...
        // Add by David 20170603
        public List<string> ListActionInParameters(string machineName, string actionName)
        {
            try
            {/*
                Log.Debug($"获取机器：{machineName} 动作：{actionName}的输入参数.");

                var machine = (Machine)ResourceManager.GetResource(machineName);

                var inActionParameters = new List<string>();

                // 构建Action模型
                var action = machine.GetAction(actionName);
                        
                // 获取输入参数
                var parInput = action.GetInParameters();

                foreach (var par in parInput)
                {
                    Log.Debug($"获取输入参数{par.Name}:{par.GetDefaultValueInString()}");
                    inActionParameters.Add(par.Name);    // 参数名
                    inActionParameters.Add(par.GetTypeString());    // 类型
                    inActionParameters.Add(par.GetDefaultValueInString());    // 默认值
                }

                return inActionParameters;*/
                return new List<string>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取机器：{machineName} 动作：{actionName}的输入参数出错 {ex.Message}");
                return null;
            }
        }

        //获取Action输出参数列表，前一个是参数名，后一个是返回值 -- David 20170603
        // 返回值： Dict[0]:参数1名，
        //          Dict[2]:参数1类型，
        //          Dict[3]:参数1默认值，
        //          ...
        // Add by David 20170603
        public List<string> ListActionOutParameters(string machineName, string actionName)
        {
            try
            {
                /*
                                Log.Debug($"获取机器：{machineName} 动作：{actionName}的输出参数.");

                                var machine = (Machine)ResourceManager.GetResource(machineName);

                                var outActionParameters = new List<string>();

                                // 构建Action模型
                                var action = machine.GetAction(actionName);

                                // 获取输出参数
                                var parOutput = action.GetOutParameters();

                                foreach (var par in parOutput)
                                {
                                    outActionParameters.Add(par.Name);    // 参数名
                                    outActionParameters.Add(par.GetTypeString());    // 类型
                                    outActionParameters.Add(par.GetValueInString());    // 默认值
                                }

                                return outActionParameters;
                */
                return new List<string>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取机器：{machineName} 动作：{actionName}的输出参数出错 {ex.Message}");
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
