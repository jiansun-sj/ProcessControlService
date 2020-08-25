// ==================================================
// 文件名：StepCheck.cs
// 创建时间：// 13:02
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/31 13:02
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Machines;
using ProcessControlService.ResourceLibrary.Processes.Conditions;

namespace ProcessControlService.ResourceLibrary.Processes.Steps
{
    public class StepCheck
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StepCheck));
        private readonly Step _owner;

        private StepCheckType _checkType;

        private readonly Condition _condition = null;

        private bool _holding;
        private short _nextStepId;
        private IBasicParameter _parameter;
        private object _value;

        public StepCheck(Step owner)
        {
            _owner = owner;
        }

        public string GetDefineNextStepId()
        {
            return _nextStepId.ToString();
        }

        public bool LoadFromConfig(XmlNode node)
        {
            // 绑定Paramenter
            try
            {
                var level0Item = (XmlElement) node;

                if (level0Item.HasAttribute("ProcessParameter"))
                {
                    // 检查类型 Parameter
                    if (node.Attributes != null)
                    {
                        var parameter = node.Attributes["ProcessParameter"].Value;

                        _parameter = _owner.OwnerProcess.ProcessParameterManager[parameter];

                        _nextStepId = Convert.ToInt16(node.Attributes["NextStepIndex"].Value);

                        var type = _parameter.GetType();
                        var constVal = node.Attributes["ConstValue"].Value;
                        _value = Parameter.CreateBasicValue(type.ToString(),constVal);
                    }

                    _checkType = StepCheckType.Parameter;
                }
                else
                {
                    throw new Exception("StepCheck没有ProcessParameter节点");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"加载StepCheck出错{_owner.Name},{ex}");
                return false;
            }
        }

        /// <summary>
        ///     判断下一步骤号
        ///     Edit by ZSY 2017年6月3日
        /// </summary>
        /// <param name="parameterManager"></param>
        /// <returns>返回下一步骤号，如果是0为不满足条件</returns>
        public short GetNextStepId(ParameterManager parameterManager)
        {
            short nextStepId = 0; // David 2017年7月19日12:38:22
            if (_checkType == StepCheckType.Parameter)
            {
                //参数判断
                var basicParameter = parameterManager.GetBasicParam(_parameter.Name);
                if (Tag.ValueEqual(basicParameter.GetValue(), _value)) // maybe wrong
                    nextStepId = _nextStepId;
            }
            else
            {
                //条件判断
                Log.Debug("执行条件判断");
                if (!_holding)
                {
                    if (_condition != null)
                        if (_condition.CheckReady())
                        {
                            nextStepId = _nextStepId;
                            _holding = true;
                        }
                }
                else
                {
                    nextStepId = _nextStepId;
                }
            }

            return nextStepId;
        }

        public void UnHold()
        {
            _holding = false;
        }

        public StepCheck Create(Step owner)
        {

            return new StepCheck(owner)
            {
                _parameter = _parameter,
                _checkType = _checkType,
                _nextStepId = _nextStepId,
                _value = _value
            };
        }
    }
}