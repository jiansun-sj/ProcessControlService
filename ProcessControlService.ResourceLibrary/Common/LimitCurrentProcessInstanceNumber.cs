// ==================================================
// 文件名：LimitCurrentProcessInstanceNumber.cs
// 创建时间：2020/06/11 9:54
// ==================================================
// 最后修改于：2020/06/11 9:54
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Processes;

namespace ProcessControlService.ResourceLibrary.Common
{
    /// <summary>
    /// 检测某个指定名称的Process正在同时执行的数量是否达到限制值
    /// </summary>
    /// <remarks>
    /// add by sunjian 2020-06-11
    /// </remarks>
    public class LimitCurrentProcessInstanceNumber : BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LimitCurrentProcessInstanceNumber));

        public LimitCurrentProcessInstanceNumber(string actionName) : base(actionName)
        {
        }

        public override void Execute()
        {
            var processName = ActionInParameterManager["ProcessName"].GetValueInString();
            short.TryParse(ActionInParameterManager["LimitNumber"].GetValueInString(),out var limitNumber);
            var processInstancesNumber = ProcessManagement.ProcessInstanceManager.GetProcessInstancesNumber(processName);

            ActionOutParameterManager["IsReachLimit"].SetValue(processInstancesNumber >= limitNumber);

            if (processInstancesNumber>=limitNumber)
            {
                Log.Info($"当前Process:[{processName}],同时执行流程次数：[{processInstancesNumber}],达到设定上限：[{limitNumber}].");
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            return true;
        }

        protected override bool CreateParameters()
        {
            ActionInParameterManager.AddBasicParam(new BasicParameter<string>("ProcessName"));
            ActionInParameterManager.AddBasicParam(new BasicParameter<short>("LimitNumber"));
            ActionOutParameterManager.AddBasicParam(new BasicParameter<bool>("IsReachLimit"));
            return true;
        }
    }
}