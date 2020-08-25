// ==================================================
// 文件名：InParameterBind.cs
// 创建时间：2020/05/02 13:53
// ==================================================
// 最后修改于：2020/06/08 13:53
// 修改人：jians
// ==================================================

using System;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Processes.Steps;

namespace ProcessControlService.ResourceLibrary.Processes.ParameterBind
{
    public class InParameterBind : ParameterBind
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Step));

        public InParameterBind(ParameterBindType parameterBindType, string actionParameterName, string bindParameter)
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
                    ProcessParameterName = bindParameter;
                    break;
                case ParameterBindType.ActionConstBasicParameterBind:
                    ConstValueString = bindParameter;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameterBindType), parameterBindType, null);
            }
        }

        private InParameterBind()
        {
        }

        public void AssignValue(ParameterManager processParameterManager, ParameterManager actionInParameterManager)
        {
            try
            {
                switch (ParameterBindType)
                {
                    case ParameterBindType.ActionProcessBasicParameterBind:
                        /*StepAction.ActionInParameterManager*/
                        actionInParameterManager.SetBasicParamValue(ActionParameterName,
                            processParameterManager.GetBasicParam(ProcessParameterName).GetValue());
                        break;
                    case ParameterBindType.ActionConstBasicParameterBind:
                        /*StepAction.ActionInParameterManager*/
                        actionInParameterManager[ActionParameterName].SetValueInString(ConstValueString);
                        break;
                    case ParameterBindType.ActionProcessListParameterBind:
                        /*StepAction.ActionInParameterManager*/
                        actionInParameterManager.GetListParam(ActionParameterName)
                            .Replace(processParameterManager.GetListParam(ProcessParameterName));
                        /*ProcessInstance.ProcessParameterManager*/

                        break;
                    case ParameterBindType.ActionProcessDictionaryParameterBind:
                        /*StepAction.ActionInParameterManager*/
                        actionInParameterManager.GetDictionaryParam(ActionParameterName).Replace(processParameterManager.GetDictionaryParam(ProcessParameterName));
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
                throw new InvalidOperationException($"InParameterBind数据赋值失败，{e.Message}");
            }
        }

        public InParameterBind CreateParameterBind()
        {
            return new InParameterBind
            {
                ActionParameterName = ActionParameterName,
                ProcessParameterName = ProcessParameterName,
                ConstValueString = ConstValueString,
                ParameterBindType = ParameterBindType
            };
        }
    }
}