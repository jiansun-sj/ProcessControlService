// ==================================================
// 文件名：WaitTagOnOffAction.cs
// 创建时间：2020/06/10 12:00
// ==================================================
// 最后修改于：2020/06/10 12:00
// 修改人：jians
// ==================================================

using System;
using System.Threading;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Machines.Actions
{
    /// <summary>
    /// 每隔_waitCycle查询周期，一直查询指定的Tag值跳转为需要的Tag值。  add by sunjian 2020-06
    /// </summary>
    public class WaitExpectedTagValueAction : MachineAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaitExpectedTagValueAction));
        private string _waitTagName;
        private IBasicParameter _expectedValue;

        private short _waitCycle=500;

        public WaitExpectedTagValueAction(string name) : base(name)
        {
        }

        public override void Execute()
        {
            try
            {
                _waitTagName = ActionInParameterManager["WaitTagName"].GetValueInString();
                _expectedValue = ActionInParameterManager["ExpectedValue"];
                _waitCycle= (short)ActionInParameterManager["CheckCycle"].GetValue();
            }
            catch (Exception e)
            {
                Log.Error("等待TagOnOrOff出错" + e);
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override bool IsFinished()
        {
            var tagValue = OwnerMachine.GetTag(_waitTagName).TagValue;

            if (_expectedValue.Equals(tagValue))
            {
                Log.Info($"{_waitTagName}的值为[{_expectedValue}]，退出等待");
                Thread.Sleep(_waitCycle);

                return true;
            }

            Log.Info($"Machine: [{OwnerMachine.ResourceName}]的Tag：[{_waitTagName}]的值为{tagValue},不为{_expectedValue.GetValueInString()}");

            return false;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        public override BaseAction Clone()
        {
            var baseAction = (WaitExpectedTagValueAction)base.Clone();

            baseAction.OwnerMachine = OwnerMachine;
            
            return baseAction;
        }
    }
}