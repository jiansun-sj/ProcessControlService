// ==================================================
// 文件名：Step.cs
// 创建时间：2020/05/02 10:43
// ==================================================
// 最后修改于：2020/06/02 10:43
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using log4net;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Processes.Conditions;
using ProcessControlService.ResourceLibrary.Processes.ParameterBind;

namespace ProcessControlService.ResourceLibrary.Processes.Steps
{
    public class Step : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Step));

        #region "callback"

        private readonly object _actionParameterLocker = new object();

        #endregion

        private bool _first = true;

        private List<StepCheck> _stepChecks = new List<StepCheck>();

        public Step(Process owner, string name, short id, short nextStepId)
        {
            OwnerProcess = owner;
            Name = name;
            StepId = id;
            NextStep = nextStepId;
        }

        public bool LogOutput { get; set; } = true;

        public short StepId { get; }

        public short NextStep { get; }

        public Process OwnerProcess { get; }

        public ProcessInstance OwnerProcessInstance { get; set; }

        public string Name { get; }

        private bool IsSync // Add by Dongmin 20191007
        {
            get;
            set;
        }

        public void Dispose()
        {
            OwnerProcessInstance?.Dispose();
            StepAction?.Dispose();
        }

        public void Execute()
        {
            try
            {
                if (LogOutput)
                    Log.Info(
                        $"执行单步过程：PROCESS:{OwnerProcess.ProcessName}.{Name}，当前同步模式为：{IsSync},pid为：【{OwnerProcessInstance.Pid}】");

                // 打印要求的过程参数
                if (ShowParameters.Count > 0)
                {
                    var processParameters = GetShowProcessParameters();
                    Log.Info("执行前.");
                    foreach (var strPar in ShowParameters)
                        if (processParameters.ContainsKey(strPar))
                            Log.Info(
                                $"参数：{strPar} 值：{processParameters[strPar].GetValueInString()}.pid为：【{OwnerProcessInstance.Pid}】");
                        else
                            Log.Error($"找不到过程参数：{strPar},pid为：【{OwnerProcessInstance.Pid}】");
                }

                if (StepAction != null)
                {
                    void ExeCuteStepCoreFunction()
                    {
                        // 把Step参数导入Action参数
                        foreach (var bind in InParameterBinds)
                            bind.AssignValue(OwnerProcessInstance.ProcessParameterManager,
                                StepAction.Action.ActionInParameterManager);

                        StepAction.Execute();
                    }

                    lock (_actionParameterLocker)
                    {
                        //Log.Info($"{StepAction.Action.Name}正在执行，Action参数已进锁，pid为：【{OwnerProcessInstance.Pid}】");
                        ExeCuteStepCoreFunction();
                        //Log.Info($"{StepAction.Action.Name}执行完毕，Action参数已解锁, pid为：【{OwnerProcessInstance.Pid}】");
                    }

                    // sunjian 2019-12-09 注释
                    if (IsSync) Thread.Sleep(Redundancy.SyncInterval + 100);
                }

                UnHoldStepIDs(); // David 2017年7月19日13:18:34
            }
            catch (Exception e)
            {
                Log.Error($"Step:[{Name}]执行异常，异常为{e}");
                throw new InvalidOperationException($"Step:[{Name}]执行异常，异常为{e}");
            }
        }

        private Dictionary<string, IParameter> GetShowProcessParameters()
        {
            var parameters = new Dictionary<string, IParameter>();

            if (ShowParameters != null)
                parameters = OwnerProcessInstance.ProcessParameterManager.GetParam(ShowParameters);

            return parameters;
        }


        /// <summary>
        ///     寻找下一个执行步骤
        ///     /add by David 20170523
        /// </summary>
        /// <returns></returns>
        public short GetNextStepId()
        {
            // short nextStepID = -1;      Alter By ZSY 2017年6月3日17:02:37
            if (_stepChecks.Count > 0) //2017年5月24日  add by:ZSY    判断step是否包含StepChecks
            {
                foreach (var item in _stepChecks)
                {
                    var tempStepId = item.GetNextStepId(OwnerProcessInstance.ProcessParameterManager);
                    if (tempStepId != 0) return tempStepId;
                }

                return NextStep;
            }

            return NextStep; //不包含StepChecks 则按默认的NextStepIndex执行，最后一步Step的nextStepIndex=-1
        }

        private void UnHoldStepIDs()
        {
            if (_stepChecks.Count <= 0) return;
            foreach (var item in _stepChecks)
                item.UnHold();
        }

        public Step CreateInstance(ParameterManager processParameterManager)
        {
            var step = new Step(OwnerProcess, Name, StepId, NextStep)
            {
                OwnerProcessInstance = OwnerProcessInstance,
                InParameterBinds = InParameterBinds.Select(inParameterBind => inParameterBind.CreateParameterBind())
                    .ToList(),
                OutParameterBinds = OutParameterBinds.Select(outParameterBind => outParameterBind.CreateParameterBind())
                    .ToList(),
                StepAction = StepAction.Create(processParameterManager),
                _msPerTicket = _msPerTicket,
                _waitTicks = _waitTicks
            };

            var showParameters = ShowParameters.ToList();

            var stepChecks = _stepChecks.Select(stepCheck => stepCheck.Create(this)).ToList();

            step.ShowParameters = showParameters;

            if (stepChecks.Count > 0)
                step._stepChecks = stepChecks;

            return step;
        }

        /// <summary>
        ///     GUI获取StepCheck
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StepCheck> GetStepChecks()
        {
            return _stepChecks;
        }

        #region "Resource"

        protected List<InParameterBind> InParameterBinds = new List<InParameterBind>();
        protected List<OutParameterBind> OutParameterBinds = new List<OutParameterBind>();

        protected List<string> ShowParameters = new List<string>();

        public StepAction StepAction = new StepAction();

        //public void SetStepAction(MachineAction StepAction)   Robin注释于20180503
        //{
        //    this.StepAction = StepAction;
        //}

        /// <summary>
        ///     Step加载Xml文件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 sunjian 2019-12-24  增加一类输入项，将list类型的参数值可以传入Step内，以便初始化List类型的Parameter。
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level0Item = (XmlElement) node;

                if (level0Item.HasAttribute("Wait"))
                {
                    var waitSeconds = Convert.ToInt32(level0Item.GetAttribute("Wait"));
                    _waitTicks = waitSeconds * 5; //每Tick 200ms，每秒5个Ticks
                }

                if (level0Item.HasAttribute("WaitInterval"))
                {
                    var mSeconds = Convert.ToInt32(level0Item.GetAttribute("WaitInterval"));
                    _msPerTicket = mSeconds; //每Tick 200ms，每秒5个Ticks
                }

                // add by Dongmin 20191007
                if (level0Item.HasAttribute("Sync")) IsSync = level0Item.GetAttribute("Sync").ToLower() == "true";


                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "StepAction"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    if (level1Item.Name.ToLower() == "step" + "action")
                    {
                        StepAction.InitFromXml(level1Item);

                        #region 指定参数捆绑关系

                        foreach (var level2Item in level1Node.Cast<XmlNode>()
                            .Where(level2Node => level2Node.NodeType != XmlNodeType.Comment).Cast<XmlElement>())
                            switch (level2Item.Name.ToLower())
                            {
                                case "in" + "parameter":
                                {
                                    //InParameter的参数捆绑有两种情况，第一种是ActionParameter和过程参数捆绑，另一种是，ActionParameter指定固定值。
                                    var strActionParameter = level2Item.GetAttribute("ActionParameter");

                                    if (level2Item.HasAttribute("ProcessParameter"))
                                    {
                                        var strProcessParameter = level2Item.GetAttribute("ProcessParameter");
                                        if (OwnerProcess.HasProcessParameter(
                                            strProcessParameter) /*校验是否存在这样的ProcessParameter*/)
                                        {
                                            var parameterBindType =
                                                OwnerProcess.GetParameterBindType(strProcessParameter);
                                            var inParameterBind = new InParameterBind(parameterBindType,
                                                strActionParameter,
                                                strProcessParameter);
                                            InParameterBinds.Add(inParameterBind);
                                        }
                                        else
                                        {
                                            Log.Error(
                                                $"装载step：{Name}出错,未找到过程参数{strProcessParameter}或是Action参数{strActionParameter}");
                                        }
                                    }
                                    else if (level2Item.HasAttribute("ConstValue"))
                                    {
                                        var strConstValue = level2Item.GetAttribute("ConstValue");

                                        var inParameterBind = new InParameterBind(
                                            ParameterBindType.ActionConstBasicParameterBind, strActionParameter,
                                            strConstValue);
                                        InParameterBinds.Add(inParameterBind);
                                    }

                                    break;
                                }
                                case "out" + "parameter":
                                {
                                    var strProcessParameter = level2Item.GetAttribute("ProcessParameter");
                                    var strActionParameter = level2Item.GetAttribute("ActionParameter");

                                    if (OwnerProcess.HasProcessParameter(
                                        strProcessParameter) /*校验是否存在这样的ProcessParameter*/)
                                    {
                                        var parameterBindType = OwnerProcess.GetParameterBindType(strProcessParameter);
                                        var outParameterBind = new OutParameterBind(parameterBindType,
                                            strActionParameter,
                                            strProcessParameter);
                                        OutParameterBinds.Add(outParameterBind);
                                    }
                                    else
                                    {
                                        Log.Error(
                                            $"装载step：{Name}出错,未找到过程参数{strProcessParameter}或是Action参数{strActionParameter}");
                                    }

                                    break;
                                }
                                case "show" + "parameter":
                                {
                                    var strProcessParameter = level2Item.GetAttribute("ProcessParameter");

                                    //只会去显示过程参数数值，和Action是谁没有关系。
                                    if (OwnerProcess.HasProcessParameter(
                                        strProcessParameter) /*校验是否存在这样的ProcessParameter*/)
                                        ShowParameters.Add(strProcessParameter);
                                    else
                                        Log.Error(
                                            $"装载step：{Name}出错,未找到过程参数{strProcessParameter}");
                                    break;
                                }
                            }

                        #endregion
                    }
                    else if (level1Item.Name == "StepChecks")
                    {
                        foreach (XmlNode n in level1Item.ChildNodes)
                        {
                            if (n is XmlComment) continue;
                            var stepCheck = new StepCheck(this);
                            stepCheck.LoadFromConfig(n);
                            _stepChecks.Add(stepCheck);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"装载step：{Name}出错:{ex.Message}");
                return false;
            }

            return true;
        }

        #endregion

        #region "check result"

        //private int _waitSeconds=5;  加载XML时的局部变量，此处无用 //Step执行反馈等待超时时间，缺省为1秒- 5个Ticks
        private int _waitTicks = 25; //默认等待5秒钟超时

        private int _msPerTicket = 200;

        private int _retry;

        /// <summary>
        ///     Gui中止ProcessStep使用参数
        /// </summary>
        private bool _stopWaiting;

        public void GuiStopStep()
        {
            _stopWaiting = true;
        }


        // 同步检查结果
        public bool CheckResultWait(Condition breakCondition)
        {
            bool ShouldContinue()
            {
                if (breakCondition == null || !breakCondition.CheckReady()) return true;
                Log.Info($"Process：[{breakCondition.OwnerProcess}]发出过程中断指令，当前ProcessPid为：{OwnerProcessInstance.Pid}");

                StepAction.Break();

                OwnerProcessInstance.ProcessStatus = ProcessStatus.ManualBreak;

                return false;
            }

            bool CheckStepResultAndBreakCondition()
            {
                while (!StepAction.CheckResult() && _retry < _waitTicks)
                {
                    if (_stopWaiting) break;
                    if (!ShouldContinue()) return false;

                    Thread.Sleep(_msPerTicket);
                    _retry++;
                }

                //Step IsFinished 之后，再给ProcessParameter赋值。
                AssignStepResult();

                return true;
            }

            _retry = 0;

            //检测Step是不是执行结束，有没有被中断，没有到超时时间，但是任务已经完成了，可以提前退出超时等待，sunj
            lock (_actionParameterLocker)
            {
                //Log.Info($"Action:[{StepAction.Action.Name}]正在检测是否完成，Action调用资源已加锁。");
                if (!CheckStepResultAndBreakCondition()) return false;

                if (!ShouldContinue())
                    return false;
            }

            ////////////////等待结束
            if (StepAction.Action != null && !StepAction.IsSuccessful())
            {
                Log.Error($"执行Step{StepId}出错,过程中断");
                return false; /*step 运行结束后，step结果依旧不是true，返回false。 sunjian 2019-11-26*/
            }

            if (_stopWaiting) // add by DongMin 20170815
            {
                Log.Warn($"执行单步过程{Name}被【主动】中断.");
                StepAction.Break();
                OwnerProcessInstance.ProcessStatus = ProcessStatus.ManualBreak;
                _stopWaiting = false;
                return false;
            }
            
            if (_retry >= _waitTicks)
            {
                Log.Warn($"执行单步过程{Name}超时.");
                
                //
                StepAction.Break();

                OwnerProcessInstance.ProcessStatus = ProcessStatus.TimeOut;

                return false;
            }

            if (LogOutput || _first)
                //LOG.Info(string.Format("执行单步过程{0}完毕.",Name));
                _first = false;
            return true;
        }

        private void AssignStepResult()
        {
            if (StepAction.Action.FeedBacks.Count > 0)
                OwnerProcessInstance.AddProcessRecordMessage(StepAction.Action.FeedBacks);

            // 把Action参数导入Step参数
            foreach (var bind in OutParameterBinds)
                bind.AssignValue(OwnerProcessInstance.ProcessParameterManager,
                    StepAction.Action.ActionOutParameterManager);

            // 打印要求的过程参数
            if (ShowParameters.Count <= 0) return;
            var processParameters = GetShowProcessParameters();
            Log.Info("执行后.");
            foreach (var strPar in ShowParameters)
                if (processParameters.ContainsKey(strPar))
                    Log.Info(
                        $"参数：{strPar} 值：{processParameters[strPar].GetValueInString()}.pid为：【{OwnerProcessInstance.Pid}】");
                else
                    Log.Error($"找不到过程参数：{strPar},pid为：【{OwnerProcessInstance.Pid}】");
        }

        #endregion
    }
}