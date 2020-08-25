// ==================================================
// 文件名：InParameterBind.cs
// 创建时间：2020/01/02 14:56
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/02/27 14:56
// 修改人：jians
// ==================================================

using System;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Processes
{
    public enum ParameterBindType
    {
        ActionProcessBasicParameterBind,
        ActionProcessListParameterBind,
        ActionProcessDictionaryParameterBind,
        ActionProcessDictionaryBasicParameterBind,
        ActionProcessBasicDictionaryParameterBind,
        ActionConstBasicParameterBind,
        InvalidBind
    }

    public class ParameterBind
    {
        /*public Parameter ActionParameter { get; set; }

        public Parameter BindParameter { get; set; }*/

        public string ActionParameterName { get; set; }

        public string ProcessParameterName { get; set; }

        public string ConstValueString { get; set; }

        // public BaseAction StepAction { get; set; }

        public ParameterBindType ParameterBindType { get; set; }

        //public ProcessInstance ProcessInstance { get; set; }

    }

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
                        actionInParameterManager.AlterBasicParameterValue(ActionParameterName,
                            /*ProcessInstance.ProcessParameterManager*/
                            processParameterManager.GetBasicParameter(ProcessParameterName).GetValue());
                        break;
                    case ParameterBindType.ActionConstBasicParameterBind:
                        /*StepAction.ActionInParameterManager*/
                        actionInParameterManager.AlterParameterValueInString(ActionParameterName,
                            ConstValueString);
                        break;
                    case ParameterBindType.ActionProcessListParameterBind:
                        /*StepAction.ActionInParameterManager*/
                        actionInParameterManager.ReplaceListParameterValue(ActionParameterName,
                            /*ProcessInstance.ProcessParameterManager*/
                            processParameterManager.GetListParameter(ProcessParameterName).GetValue());
                        break;
                    case ParameterBindType.ActionProcessDictionaryParameterBind:
                        /*StepAction.ActionInParameterManager*/
                        actionInParameterManager.ReplaceDictionaryParameterValue(ActionParameterName,
                            /*ProcessInstance.ProcessParameterManager*/
                            processParameterManager.GetDictionaryParameter(ProcessParameterName).GetValue());
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


        //动态指定参数捆绑
        /*
                public void SetParameterBind(ProcessInstance processInstance, StepAction stepAction)
                {
                    StepAction = stepAction.Action;
                    ProcessInstance = processInstance;

                    if (ParameterBindType != ParameterBindType.ActionConstBasicParameterBind) return;
                    var constParameter = new BasicParameter("ConstParameter",
                        StepAction.ActionInParameterManager.GetBasicParameter(ActionParameterName).StrType);
                    constParameter.SetDefaultValueInString(ConstValueString);
                    BindParameter = constParameter;
                    /*switch (ParameterBindType)
                    {
                        case ParameterBindType.ActionProcessBasicParameterBind:
                            BindParameter =parameterManager.GetBasicParameter(ProcessParameterName);
                            break;

                        case ParameterBindType.ActionConstBasicParameterBind:
                            var constParameter =new BasicParameter("ConstParameter",
                                ActionParameter.StrType);
                            constParameter.SetDefaultValueInString(ConstValueString);
                            BindParameter = constParameter;
                            break;

                        case ParameterBindType.ActionProcessListParameterBind:
                            BindParameter = parameterManager.GetListParameter(ProcessParameterName);
                            break;

                        case ParameterBindType.ActionProcessDictionaryParameterBind:
                            BindParameter = parameterManager.GetDictionaryParameter(ProcessParameterName);
                            break;

                        case ParameterBindType.ActionProcessDictionaryBasicParameterBind:
                            break;
                        case ParameterBindType.ActionProcessBasicDictionaryParameterBind:
                            break;
                        case ParameterBindType.InvalidBind:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }#1#
                }
        */
    }
}