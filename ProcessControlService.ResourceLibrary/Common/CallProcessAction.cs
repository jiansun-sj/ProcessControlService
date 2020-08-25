// ==================================================
// 文件名：CallProcessAction.cs
// 创建时间：2019/11/13 15:13
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:13
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Processes;

namespace ProcessControlService.ResourceLibrary.Common
{
    /// <summary>
    ///     调用其他process的action
    ///     Created by David Dong 20170619
    /// </summary>
    /// <StepAction Container="CommonResource" Action="CallProcessAction">
    ///     <InParameter ActionParameter="ProcessName" ConstValue="" />
    /// </StepAction>
    public class CallProcessAction : BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CallProcessAction));

        public CallProcessAction(string name)
            : base(name)
        {
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            throw new NotImplementedException();
        }

        #region "Parameters"

        protected override bool CreateParameters()
        {
            ActionInParameterManager.AddBasicParam(new BasicParameter<string>("ProcessName"));
            ActionInParameterManager.AddDictionaryParam(new DictionaryParameter<string>("ParameterDictionary"));
            ActionInParameterManager.AddBasicParam(new BasicParameter<string>("SubProcessResources"));
            return true;
        }
        #endregion

        #region "core functions"

        private DateTime _startTime;
        private Process _process;
        private string _processName;

        public override void Execute()
        {
            try
            {
                _processName = ActionInParameterManager["ProcessName"].GetValueInString();
                var resourceDictionaryName = ActionInParameterManager["SubProcessResources"].GetValueInString();
                _process = (Process) ResourceManager.GetResource(_processName);
                var selectedResource = (DictionaryParameter<string>)ActionInParameterManager.GetDictionaryParam("ParameterDictionary");
                
                if (_process != null)
                {
                    _startTime = DateTime.Now;
                    Log.Debug($"开始调用其他Process：{_processName}");


                    ProcessManagement.CallProcessActionRunInstance(_process, new ResourceDicModel<string>
                        { ResourceDictionaryName = resourceDictionaryName, DictionaryParameter = selectedResource });
                }
                else
                {
                    Log.Error($"调用其他Process：{_processName}出错ProcessName名字可能错误");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"调用其他Process：{_processName}出错{ex}");
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override bool IsFinished()
        {
            try
            {
                //sunjian 2020-1-4 如果需要调用的process允许重入，则子程序的结束与否由子程序自行决定，主程序仅仅是执行了调用子程序的动作，调用完就结束。
                if (_process.AllowAsync)
                {
                    return true;
                }
                /*if (_process.GetProcessStatus() != ProcessStatus.Finished &&
                    _process.GetProcessStatus() != ProcessStatus.Break) return false;*/
                //结束
                var ts = DateTime.Now - _startTime;
                Log.Debug($"调用其他Process：{_processName}结束,耗时{ts.TotalSeconds}秒");

                return true;
            }
            catch (Exception)
            {
                Log.Error($"调用其他Process：{_processName} 判断结果引起异常出错");
                return false; //异常
            }
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

