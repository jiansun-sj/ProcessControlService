// ==================================================
// 文件名：OutParameterBind.cs
// 创建时间：2020/01/02 15:32
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/01/19 15:32
// 修改人：jians
// ==================================================

using System;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Processes
{
    public class OutParameterBind : ParameterBind
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Step));

        public OutParameterBind(ParameterBindType parameterBindType, string actionParameterName,
            string bindParameterName)
        {
            ParameterBindType = parameterBindType;

            ActionParameterName = actionParameterName;

            switch (parameterBindType)
            {
                case ParameterBindType.ActionProcessBasicParameterBind:
                case ParameterBindType.ActionProcessListParameterBind:
                case ParameterBindType.ActionProcessDictionaryParameterBind:
                case ParameterBindType.ActionProcessDictionaryBasicParameterBind:
                case ParameterBindType.ActionProcessBasicDictionaryParameterBind:
                    ProcessParameterName = bindParameterName;
                    break;
                case ParameterBindType.ActionConstBasicParameterBind:
                    ConstValueString = bindParameterName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameterBindType), parameterBindType, null);
            }
        }

        private OutParameterBind()
        {
        }

        public void AssignValue(ParameterManager processParameterManager, ParameterManager actionOutParameterManager)
        {

            try
            {
                switch (ParameterBindType)
                {
                    case ParameterBindType.ActionProcessBasicParameterBind:
                        /*ProcessInstance.ProcessParameterManager.*/processParameterManager.AlterBasicParameterValue(ProcessParameterName,
                            /*StepAction.ActionOutParameterManager.*/actionOutParameterManager.GetBasicParameter(ActionParameterName).GetValue());
                        break;
                    case ParameterBindType.ActionProcessListParameterBind:
                        /*ProcessInstance.ProcessParameterManager.*/processParameterManager.ReplaceListParameterValue(ProcessParameterName,
                            /*StepAction.ActionOutParameterManager.*/actionOutParameterManager.GetListParameter(ActionParameterName).GetValue());
                        break;
                    case ParameterBindType.ActionProcessDictionaryParameterBind:
                        /*ProcessInstance.ProcessParameterManager.*/processParameterManager.ReplaceDictionaryParameterValue(ProcessParameterName,
                            /*StepAction.ActionOutParameterManager.*/actionOutParameterManager.GetDictionaryParameter(ActionParameterName).GetValue());
                        break;
                    case ParameterBindType.ActionProcessDictionaryBasicParameterBind:
                        break;
                    case ParameterBindType.ActionProcessBasicDictionaryParameterBind:
                        break;
                    case ParameterBindType.InvalidBind:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"OutParameterBind数据赋值失败，{e.Message}");
            }
        }

        public OutParameterBind CreateParameterBind()
        {
            return new OutParameterBind
            {
                ActionParameterName = ActionParameterName,
                ProcessParameterName = ProcessParameterName,
                ConstValueString = ConstValueString,
                ParameterBindType = ParameterBindType
            };
        }
    }
}