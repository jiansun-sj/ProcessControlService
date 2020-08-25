// ==================================================
// 文件名：ProcessInstance.cs
// 创建时间：2020/05/02 15:36
// ==================================================
// 最后修改于：2020/06/08 15:36
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using log4net;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Common;
using ProcessControlService.ResourceLibrary.Processes.Conditions;
using ProcessControlService.ResourceLibrary.Processes.Steps;

namespace ProcessControlService.ResourceLibrary.Processes
{
    /// <summary>
    ///     过程实例参数类
    /// </summary>
    /// <remarks>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 added by sunjian 2019-12-31
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class ProcessInstance : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessInstance));

        private const string ResourceDicName = "ResourceDic";

        private readonly object _stepLocker = new object();
        private ProcessInstanceRecord _processInstanceRecord;

        public string Pid { get; set; }

        private short CurrentStepId { get; set; }

        public string ProcessName { get; set; }

        private Dictionary<short, Step> Steps { get; } = new Dictionary<short, Step>();

        public ParameterManager ProcessParameterManager { get; set; } = new ParameterManager();

        public Condition BreakCondition { get; set; }

        public ProcessStatus ProcessStatus { get; set; }

        public bool LogOutput { get; set; }
        public bool AllowAsync { get; set; }
        public List<ProcessLocker> ProcessLockers { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void StopWork()
        {
            foreach (var step in Steps) step.Value.Dispose();
            ProcessParameterManager.Dispose();
            Steps.Clear();
            BreakCondition = null;
            Dispose();
        }

        ~ProcessInstance()
        {
            Dispose();
        }

        public void Execute()
        {
            try
            {
                //2019年8月22日 16:15:34  夏  增加计时

                var sw = new Stopwatch();

                var isBreak = false;

                sw.Start();

                // 设定process状态
                // 根据step返回结果，跳转STEP的执行
                CurrentStepId = 1; // 从1开始

                //sunjian 2020-1-15 增加process 中断条件的检测
                while (CurrentStepId != -1)
                {
                    Log.Info($"当前StePid为：[{CurrentStepId}],  pid为：{Pid}.");

                    var stepSw = new Stopwatch();

                    stepSw.Start();

                    if (!LogOutput) //0521Robin改
                        Steps[CurrentStepId].LogOutput = false;

                    if (LogOutput)
                        Log.Info($"#################当前Process:{ProcessName} StepID:{CurrentStepId},Pid:{Pid} ");

                    //jiansun， 2019/11/8 如果当前step为空，则不执行
                    if (!Steps.ContainsKey(CurrentStepId))
                    {
                        sw.Stop();
                        Log.Error($"Process执行出错，Process：{ProcessName},Step ID:{CurrentStepId},Pid:{Pid}。");
                        return;
                    }

                    EnterProcessLocker(CurrentStepId);

                    Steps[CurrentStepId].Execute(); // 执行当前step

                    var stepResult = Steps[CurrentStepId].CheckResultWait(BreakCondition);

                    //过程中断不会给tempStepId赋值下一stepId，tempStepId保持-1，process执行结束。
                    short tempStepId = -1;

                    if (stepResult)
                        //如果Process当前Step运行正常，获取Process下一步执行StepId
                    {
                        tempStepId = Steps[CurrentStepId].GetNextStepId();
                    }
                    else
                    {
                        var process = (Process) ResourceManager.GetResource(ProcessName);

                        process.BreakCounts++;

                        _processInstanceRecord.BreakStepId = CurrentStepId;
                        _processInstanceRecord.BreakStepName = Steps[CurrentStepId].Name;

                        isBreak = true;
                    }

                    ExitProcessLocker(CurrentStepId);

                    stepSw.Stop();
                    Log.DebugFormat("执行Process：[{0}],单步过程[{1}]耗时：{2}ms,Pid:[{3}]", ProcessName,
                        Steps[CurrentStepId].Name, stepSw.ElapsedMilliseconds, Pid);

                    CurrentStepId = tempStepId; // 获取下一个step ID
                }

                // 设定process状态
                sw.Stop();

                Log.Info($"Read执行Process{ProcessName} 耗时[{sw.ElapsedMilliseconds}] ms");

                RecordProcessStatus(isBreak ? ProcessStatus : ProcessStatus.Finished);

                //sunjian 2019-12-31 执行完process需要对ProcessInstance执行相应操作，目前是清除ProcessParameter
                ProcessManagement.RemoveFinishedProcessInstance(this);
            }
            catch (Exception e)
            {
                Log.Error($"运行ProcessInstance:[{ProcessName}].[{Pid}]失败,异常为：[{e.Message}]");
                throw;
            }
        }

        private void ExitProcessLocker(short currentStepId)
        {
            if (ProcessLockers==null)
                return;

            foreach (var processLocker in ProcessLockers)
            {
                if (currentStepId!=processLocker.ExitLockerStep)
                    continue;
                
                var lockerResource = GetResourceFromResourceDic(processLocker.LockerKey);

                Monitor.Exit(lockerResource.ResourceLocker);
            }
        }

        private IResource GetResourceFromResourceDic(string resourceKey)
        {
            var lockerName = ProcessParameterManager.GetDictionaryParam(ResourceDicName).GetValue(resourceKey)
                .ToString();

            var lockerResource = ResourceManager.GetResource(lockerName);
            
            return lockerResource;
        }

        private void EnterProcessLocker(short currentStepId)
        {
            if (ProcessLockers==null)
                return;

            foreach (var processLocker in ProcessLockers)
            {
                if (currentStepId!=processLocker.EntryLockerStep)
                    continue;

                var lockerResource = GetResourceFromResourceDic(processLocker.LockerKey);
                
                Monitor.Enter(lockerResource.ResourceLocker);
            }
        }

        public void RecordProcessStatus(ProcessStatus processStatus)
        {
            _processInstanceRecord.EndTime = DateTime.Now;
            _processInstanceRecord.Parameters = ProcessParameterManager.GetValueInString();
            _processInstanceRecord.ProcessStatus = processStatus;
            ProcessRecordSqLiteUtil.LogProcessInstance(_processInstanceRecord);
        }

        public void AddProcessRecordMessage(Message message)
        {
            _processInstanceRecord.Messages.Add(message);
        }

        public void AddProcessRecordMessage(IEnumerable<Message> exList)
        {
            _processInstanceRecord.Messages.AddRange(new List<Message>(exList));
        }

        public void Initialize(Process process, ResourceDicModel<string> resourceDic)
        {
            try
            {
                _processInstanceRecord = new ProcessInstanceRecord
                    {ProcessName = ProcessName, Pid = Pid, StartTime = DateTime.Now};

                _processInstanceRecord.Messages.Add(new Message {Description = $"Pid: [{Pid}]"});
                
                //将主流程调用时选定的资源名同步到ProcessInstance当中。
                if (resourceDic != null)
                    ProcessParameterManager.GetDictionaryParam(resourceDic.ResourceDictionaryName)
                        .Replace(resourceDic.DictionaryParameter);

                foreach (var processStep in process.Steps)
                {
                    var step = processStep.Value.CreateInstance(ProcessParameterManager);

                    step.OwnerProcessInstance = this;

                    Steps.Add(processStep.Key, step);
                }
            }
            catch (Exception e)
            {
                Log.Error($"ProcessInstance进行初始化失败，{e.Message}");
            }
        }

        public List<ParameterInfo> GetParametersValue()
        {
            return ProcessParameterManager.GetValueInString();
        }

        public void StopProcess()
        {
            /*给所有step中的终止信号都写值防止终止Process失败*/
            lock (_stepLocker)
            {
                foreach (var step in Steps)
                    step.Value.GuiStopStep();
            }
        }

        public CurrentStepInfo GetCurrentStep()
        {
            var container = "";
            try
            {
                var id = CurrentStepId;

                if (id != -1)
                {
                    var actionActionContainer = (IResource) Steps[CurrentStepId].StepAction.Action.ActionContainer;
                    container = actionActionContainer.ResourceName;
                }

                return new CurrentStepInfo
                {
                    Id = id,
                    Name = id == -1 ? "结束" : Steps[id].Name,
                    Container = container
                };
            }
            catch (Exception e)
            {
                Log.Error($"获取当前步骤动作所属容器失败。异常为：[{e.Message}]。");
                return new CurrentStepInfo
                {
                    Id = 0,
                    Name = "Error",
                    Container = "Error"
                };
            }
        }
    }
}