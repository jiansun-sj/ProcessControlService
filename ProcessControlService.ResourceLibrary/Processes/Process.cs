// ==================================================
// 文件名：Process.cs
// 创建时间：2020/05/02 10:11
// ==================================================
// 最后修改于：2020/05/11 10:11
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using log4net;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.Attributes;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Common;
using ProcessControlService.ResourceLibrary.Processes.Conditions;
using ProcessControlService.ResourceLibrary.Processes.ParameterBind;
using ProcessControlService.ResourceLibrary.Processes.Steps;

namespace ProcessControlService.ResourceLibrary.Processes
{
    public sealed class Process : Resource, IRedundancy, IWork
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Process));

        public List<ProcessLocker> ProcessLockers { get; set; }=new List<ProcessLocker>();

        public Process(string name) : base(name)
        {
            ProcessName = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(name)) : name;
        }

        public long RunCounts { get; set; }

        public long BreakCounts
        {
            get => _breakCounts;
            set => _breakCounts = value < long.MaxValue ? value : 0;
        }

        public bool LogOutput { get; private set; } = true;

        private bool IsProcessRecord { get; set; }

        private bool Enabled { get; set; }
        
        #region parameters

        public ParameterManager ProcessParameterManager { get; set; } = new ParameterManager();

        public bool HasProcessParameter(string strProcessParameter)
        {
            return ProcessParameterManager.ContainsKey(strProcessParameter);
        }

        public ParameterBindType GetParameterBindType(string strProcessParameter)
        {
            var parameterType = ProcessParameterManager.GetParameterType(strProcessParameter);

            if (parameterType.Contains("Basic")) return ParameterBindType.ActionProcessBasicParameterBind;

            if (parameterType.Contains("List")) return ParameterBindType.ActionProcessListParameterBind;

            return parameterType.Contains("Dictionary")
                ? ParameterBindType.ActionProcessDictionaryParameterBind
                : ParameterBindType.InvalidBind;
        }

        #endregion

        #region "Resource definition"

        public readonly string ProcessName;

        public bool AutoMode { get; set; } = true;

        private BreakConditionGenerator BreakConditionModel { get; set; } = new BreakConditionGenerator();

        //#endregion
        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level0Item = (XmlElement) node;
                
                var strEnable = level0Item.GetAttribute("Enable");
                Enabled = Convert.ToBoolean(strEnable);

                // level0 -- "NeedDataSync" --add by Dongmin 20191107
                //sunjian -- 2020-1-18 暂时屏蔽掉冗余功能
                /*if (level0Item.HasAttribute("NeedDataSync"))
                {
                    var strNeedDataSync = level0Item.GetAttribute("NeedDataSync");
                    NeedDataSync = strNeedDataSync.ToLower() == "true";
                }*/

                if (level0Item.HasAttribute("AllowAsync"))
                {
                    AllowAsync = Convert.ToBoolean(level0Item.GetAttribute("AllowAsync"));

                    var allowAsync = AllowAsync ? "允许重入操作" : "禁止重入功能";

                    Log.Info($"{ResourceName} {allowAsync}.");
                }

                if (level0Item.HasAttribute("LogOutput")) //0521Robin加
                    LogOutput = Convert.ToBoolean(level0Item.GetAttribute("LogOutput"));

                if (level0Item.HasAttribute("IsProcessRecord")) //0610 guyang process记录数据库功能
                    IsProcessRecord = Convert.ToBoolean(level0Item.GetAttribute("IsProcessRecord"));

                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "condition","Step"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    switch (level1Item.Name.ToLower())
                    {
                        case "process" + "parameters":
                        {
                            #region ProcessParameter

                            foreach (var level2Element in level1Node.Cast<XmlNode>()
                                .Where(level2Node => level2Node.NodeType != XmlNodeType.Comment).Cast<XmlElement>())
                            {
                                if(level2Element.Name.ToLower()=="process" + "parameter") 
                                    Parameter.LoadParameterFromConfig(level2Element,ProcessParameterManager);
                            }
                            break;

                            #endregion
                        }
                        case "lockers":
                        {
                            #region Lockers

                            foreach (var level2Element in level1Node.Cast<XmlNode>()
                                .Where(level2Node => level2Node.NodeType != XmlNodeType.Comment).Cast<XmlElement>())
                            {
                                if(level2Element.Name.ToLower()=="locker") 
                                    ProcessLockers.Add(ProcessLocker.LoadFromConfig(level2Element));
                            }
                            break;

                            #endregion
                        }
                        case "condition":
                        {
                            #region Condition

                            var condition = Condition.CreateFromConfig(level1Item, this);
                            condition.LoadFromConfig(level1Item);
                            Condition = condition;

                            #endregion

                            break;
                        }
                        case "steps":
                        {
                            #region Steps

                            foreach (XmlNode level2Node in level1Node)
                            {
                                if (level2Node.NodeType == XmlNodeType.Comment)
                                    continue;

                                var level2Item = (XmlElement) level2Node;

                                var stepName = level2Item.GetAttribute("Name");

                                var stepId = Convert.ToInt16(level2Item.GetAttribute("Index"));
                                short nextStepId;
                                if (level2Item.HasAttribute("NextStepIndex"))
                                    nextStepId = Convert.ToInt16(level2Item.GetAttribute("NextStepIndex"));
                                else
                                    nextStepId = -1;

                                var step = new Step(this, stepName, stepId, nextStepId);

                                if (step.LoadFromConfig(level2Item) == false)
                                    return false;

                                //step.StepFinished += StepStatusFinished;

                                Steps.Add(step.StepId, step);
                            }

                            #endregion

                            break;
                        }
                        case "break" + "condition":

                            #region BreakCondition

                            var breakCondition = Condition.CreateFromConfig(level1Item, this);
                            BreakConditionModel = new BreakConditionGenerator
                                {BreakCondition = breakCondition, ConditionConfig = level1Item};

                            #endregion

                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"装载Process时出错：{ex}");
                return false;
            }
        }

        public ProcessInstance InitializeProcessInstance(ResourceDicModel<string> resourceDicModel = null)
        {
            if (LogOutput) Log.Info($"过程: {ProcessName} Times:{RunCounts} 开始创建过程实例");
            var pid = ProcessManagement.GeneratePid();
            ProcessInstance processInstance = null;
            try
            {
                if (pid == "-1")
                {
                    Log.Error("ProcessInstanceManager创建过程实例参数失败,Pid为错误编号-1");
                    return null;
                }

                processInstance = new ProcessInstance
                {
                    Pid = pid,
                    ProcessName = ProcessName,
                    LogOutput = LogOutput,
                    BreakCondition = BreakConditionModel.GenerateBreakCondition(resourceDicModel, this),
                    AllowAsync = AllowAsync,
                    ProcessLockers = ProcessLockers,
                    ProcessParameterManager = ProcessParameterManager.Clone()
                };

                return processInstance;

            }
            catch(Exception e)
            {
                Log.Error($"ProcessInstanceManager创建过程实例参数失败,异常为：{e}");
                return processInstance;
            }
        }
        
        public void CreateProcessInstance(ProcessInstance processInstance,ResourceDicModel<string> resourceDicModel = null)
        {
            try
            {
                if (processInstance==null)
                    throw new ArgumentNullException(nameof(processInstance));

                using (processInstance)
                {
                    processInstance.Initialize(this, resourceDicModel);

                    Log.Info($"成功创建ProcessInstance，实例编号为[{processInstance.Pid}]");

                    ProcessManagement.RunProcessInstance(processInstance);
                }

                if (RunCounts < long.MaxValue)
                    RunCounts++;
                else
                    RunCounts = 1;

                if (LogOutput) Log.Info($"过程: {ProcessName} Times:{RunCounts - 1} 自动执行完毕");
            }
            catch (Exception e)
            {
                //sunjian 2020-1-21 如果processInstance执行出错，也需要移除该过程实例，防止发生内存泄漏。
                if (processInstance != null)
                {
                    processInstance.AddProcessRecordMessage(new Message {Description = $"运行过程实例参数失败，{e.StackTrace}"});

                    processInstance.RecordProcessStatus(ProcessStatus.Error);

                    ProcessManagement.RemoveFinishedProcessInstance(processInstance);
                }

                BreakCounts++;
                Log.Error($"ProcessInstanceManager运行过程实例参数失败，{e.Message}");
            }
        }

        public bool AllowAsync { get; set; }

        #region IWork

        public bool WorkStarted { get; private set; }

        public void StartWork()
        {
            if (WorkStarted) return;
            ProcessManagement.StartCheckCondition();
            WorkStarted = true;
        }

        public void StopWork()
        {
            ProcessManagement.StopCheckCondition();
            WorkStarted = false;
        }

        #endregion

        // add by David 20170917
        public override void FreeResource()
        {
            // StopProcessInstance();
            StopWork();
        }

        #endregion

        #region Redanduncy

        public bool NeedDataSync { get; } = false;

        public RedundancyMode CurrentRedundancyMode { get; private set; } = RedundancyMode.Unknown;

        public void RedundancyModeChange(RedundancyMode mode)
        {
            Log.Debug($"Process{ProcessName}运行模式改变为{mode}");

            CurrentRedundancyMode = mode;

            if (CurrentRedundancyMode == RedundancyMode.Master)
                StartWork();
            else
                StopWork();
        }

        public string BuildSyncData()
        {
            throw new NotImplementedException();
        }

        public void ExtractSyncData(string data)
        {
            throw new NotImplementedException();
        }

/*        public bool NeedDataSync { get; private set; }

        private readonly object _lockBuildSyncData = new object();*/

        /*//同步数据改为已执行完毕的step参数。
        public string BuildSyncData()
        {
            try
            {
                /*string executedStepParameters;

                lock (_lockBuildSyncData)
                {
                    executedStepParameters = JsonConvert.SerializeObject(ExecutedStepsParameters);

                    ExecutedStepsParameters.Clear();
                }

                return executedStepParameters;#1#

                return GetStatus();
            }
            catch (Message e)
            {
                Log.Error("创建主机同步数据失败", e);
                return "";
            }
        }*/

        /*private static ProcessStatusModel UnpackData(string strData)
        {
            var ser = new DataContractJsonSerializer(typeof(ProcessStatusModel));

            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(strData)))
                {
                    var obj = ser.ReadObject(ms) as ProcessStatusModel;
                    return obj;
                }
            }
            catch (IOException ex)
            {
                Log.Error($"反序列化ProcessStatusModel：{strData}出错：{ex}");
                return null;
            }
        }

        public void ExtractSyncData(string data)
        {
            try
            {
                var masterStatus = UnpackData(data);


                // 同步参数
                foreach (var keyValuePair in masterStatus.Parameters)
                    SetParameterInString(keyValuePair.Key, keyValuePair.Value);

                // 执行同步Step
                var currentMasterStepId = masterStatus.CurrentStepID;

                var step = GetStep(currentMasterStepId);
                if (step != null)
                {
                    var bSyncStep = GetStep(currentMasterStepId).IsSync;
                    Log.Debug($"主站Process:{ResourceName},当前StepID：{currentMasterStepId},是否同步：{bSyncStep}");

                    if (bSyncStep)
                        Task.Run(() =>
                        {
                            ExecuteMasterStep(currentMasterStepId,step.ProcessInstancePid);
                        }); // change by dongmin 201109 , 取消异步 sunjian 2019-12-09
                }
                else
                {
                    ClearMasterStep();
                }
            }
            catch (Message e)
            {
                Log.Error("同步主站Process出错", e);
            }
        }*/

/*        private readonly List<short> _masterStepsRunning = new List<short>();

        private readonly object _lockerExecuteMasterStep = new object();

        private static int _stepRunCount;

        private void ExecuteMasterStep(short stepId,string pid)
        {
            bool masterStepRunning;
            lock (_lockerExecuteMasterStep)
            {
                masterStepRunning = _masterStepsRunning.Contains(stepId);
            }

            if (masterStepRunning) return;
            _stepRunCount++;

            //LOG.Debug(string.Format("开始执行主站Process:{0},StepID：{1}", ResourceName, StepID));

            lock (_lockerExecuteMasterStep)
            {
                _masterStepsRunning.Add(stepId);
            }

            Log.Debug(string.Format("第{2}次执行主站Process:{0},StepID：{1}", ResourceName, stepId, _stepRunCount));
            //Thread.Sleep(200);//模拟执行耗时
            ExecuteStep(stepId,pid);
        }

        private void ClearMasterStep()
        {
            lock (_lockerExecuteMasterStep)
            {
                _masterStepsRunning.Clear();
            }
        }*/

        #endregion

        #region Conditions

        public Condition Condition;

        public bool CheckCondition()
        {
            try
            {
                if (Enabled) return Condition != null && Condition.CheckReady();

                //GetConditionModel().Status = ConditionModel.ConditionStatus.Unknown;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }

        [ResourceService]
        public string AlterMultiTagsConditionTrigTagMonitorStatus(string machineName, string tagName,
            bool shouldMonitor)
        {
            try
            {
                if (Condition is MultiTagsCondition multiTagsCondition)
                    return multiTagsCondition.AlterTrigTagMonitorStatus(machineName, tagName, shouldMonitor);

                return $"Process:[{ProcessName}]的触发条件不是MultiTagsCondition，无法屏蔽Tag";
            }
            catch (Exception e)
            {
                Log.Error($"屏蔽MultiTagsCondition监控Tag失败，异常为[{e.Message}]");
                return $"屏蔽MultiTagsCondition监控Tag失败，异常为[{e.Message}]";
            }
        }

        #endregion

        #region "Steps"

        public Dictionary<short, Step>
            Steps = new Dictionary<short, Step>(); // 从LIST改成 directionary -- David 20170523

        public int InstancesLimit = 100;
        private long _breakCounts;

        public List<string> ExecutedStepsParameters { get; set; } = new List<string>(); // 该process在执行周期内执行完成的所有step的参数。

        public List<string> GetContainerNames()
        {
            try
            {
                var containerNames = new List<string>();

                if (!ProcessParameterManager.HasParam("ResourceDic")) return containerNames;

                var dictionaryParameter = ProcessParameterManager.GetDictionaryParam("ResourceDic");

                if (dictionaryParameter != null && dictionaryParameter.StrType.ToUpper() == "STRING")
                    containerNames.AddRange(collection: ((DictionaryParameter<string>)dictionaryParameter).ToValueList());

                return containerNames;
            }
            catch (Exception e)
            {
                Log.Error($"获取Process的参数化Container失败。异常为：【{e.Message}】");
                return null;
            }
        }

        #endregion

        #region "status"

        // add by Dongmin 20191007
        /*public Step GetStep(short stepId)
        {
            return Steps.ContainsKey(stepId) ? Steps[stepId] : null;
        }

        private ProcessStatus _status;
        private short _currentStepId;*/

        /*
        public void StopProcessInstance()
        {
            // 停止当前正在运行的step  add by Dongmin 20170815
            Steps[_currentStepId].StopWaiting();
        }
*/

        /*
                private void ExecuteStep(short stepId,string pid) //Modify by Dongmin 20191007
                {
                    Log.Info($"过程:{ResourceName} 执行单步ID:{stepId}");

                    var stepSw = new Stopwatch();
                    stepSw.Start();

                    var step = Steps[stepId];

                    step.Execute(pid);
                    step.CheckResultNoWait();

                    stepSw.Stop();
                    Log.DebugFormat("单步执行Process[{0}],Step[{1}]耗时：{2}ms", ResourceName, Steps[stepId].Name,
                        stepSw.ElapsedMilliseconds);
                }
        */

        /*
                private void StepStatusFinished(object sender, StepFinishedEventArgs args)
                {
                    var step = (Step) sender;

                    if (LogOutput || First)
                        Log.Info($"过程:{ResourceName} 执行单步: Name:{step.Name} 结果:{step.Info.Status.ToString()}");
                }
        */

        /*
                public virtual string GetStatus()
                {
                    //build process parameters in string
                    var pars = ProcessParameters.ToDictionary(par => par.Key, par => par.Value.GetValueInString());

                    var statusModel = new ProcessStatusModel(_status, _currentStepId, pars);
                    var json = new DataContractJsonSerializer(statusModel.GetType());
                    string szJson;
                    using (var stream = new MemoryStream())
                    {
                        json.WriteObject(stream, statusModel);
                        szJson = Encoding.UTF8.GetString(stream.ToArray());
                    }

                    return szJson;
                }
        */

        //private ProcessStatus _status = new ProcessStatus();

        /*
                public string GetProcessStatusInString()
                {
                    return _status.ToString();
                }
        */

        /*
                public ProcessStatus GetProcessStatus()
                {
                    return _status;
                }
        */

        /*
                public void SetProcessStatus(ProcessStatus status)
                {
                    _status = status;
                }
        */

        /*
                public short GetProcessStepId()
                {
                    return _currentStepId;
                }
        */

        #endregion
        
        #region GUI接口

        /// <summary>
        ///     GUI手动启动Process
        /// </summary>
        /// <param name="inParameters"></param>
        /// <param name="containers"></param>
        public void ManualStart(Dictionary<string, string> inParameters, Dictionary<string, string> containers)
        {
            if (inParameters != null && inParameters.Count > 0)
            {
                //todo: Process手动启动参数初始化。
            }

            //初始化ProcessStep的实际执行资源
            if (containers != null && containers.Count > 0)
            {
                var selectedResources = new DictionaryParameter<string>("SelectedResources");

                foreach (var container in containers)
                    selectedResources.Add(container.Key, container.Value);

                var resourceDicModel = new ResourceDicModel<string>
                    {ResourceDictionaryName = "SelectedResources", DictionaryParameter = selectedResources};

                var initializeProcessInstance = InitializeProcessInstance(resourceDicModel);

                CreateProcessInstance(initializeProcessInstance,resourceDicModel);
            }
            else
            {
                CreateProcessInstance(null);
            }
        }

        /// <summary>
        ///     GUI获取Process的Step名称和跳转关系。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dictionary<short, List<string>> GetProcessAllStepsIdName()
        {
            var stepsIdName = new Dictionary<short, List<string>>();
            foreach (var step in Steps)
            {
                var defineNextStepId = new List<string> {step.Value.Name, step.Value.NextStep.ToString()};
                foreach (var stepCheck in step.Value.GetStepChecks())
                {
                    var nextStepId = stepCheck.GetDefineNextStepId();
                    if (!defineNextStepId.Contains(nextStepId)) defineNextStepId.Add(nextStepId);
                }

                stepsIdName.Add(step.Key, defineNextStepId);
            }

            return stepsIdName;
        }

        #endregion
    }
}