// ==================================================
// 文件名：ProcessInstanceManager.cs
// 创建时间：// 11:36
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/31 11:36
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using log4net;
using ProcessControlService.ResourceLibrary.Common;
using ProcessControlService.ResourceLibrary.Processes.Conditions;

namespace ProcessControlService.ResourceLibrary.Processes
{
    public class BreakConditionGenerator
    {
        private XmlElement _conditionConfig;
        
        public Condition BreakCondition { get; set; }

        public XmlElement ConditionConfig
        {
            get => _conditionConfig;
            set
            {
                _conditionConfig = value;

                if (!_conditionConfig.HasAttribute("Container")) return;
                var attribute = _conditionConfig.GetAttribute("Container");

                if (!attribute.Contains("Using")) return;

                _isBindResourceTemplate = true;

                var trimmedContainerString = attribute.Trim('{').Trim('}').Replace(" ", "").Trim();

                var containerAttribute = trimmedContainerString.Split(',');

                _bindKey = containerAttribute[2].Substring("Key=".Length, containerAttribute[2].Length - "Key=".Length);
            }
        }

        private bool _isBindResourceTemplate;

        private string _bindKey;

        public Condition GenerateBreakCondition(ResourceDicModel<string> resourceDic, Process process)
        {
            if (BreakCondition is null)
                return null;

            if (_isBindResourceTemplate)
            {
                var config = ConditionConfig;

                config.SetAttribute("Container", resourceDic.DictionaryParameter.GetAllValue()[_bindKey]);

                var breakCondition = /*BreakCondition;*/Condition.CreateFromConfig(config, process);
                breakCondition.LoadFromConfig(config);

                return breakCondition;
            }
            else
            {
                var breakCondition = /*BreakCondition;*/Condition.CreateFromConfig(ConditionConfig, process);
                breakCondition.LoadFromConfig(ConditionConfig);

                return breakCondition;
            }
        }
    }

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
    public class ProcessInstanceManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessInstanceManager));

        public List<ProcessInstance> ProcessInstances { get; set; } = new List<ProcessInstance>();

        /// <summary>
        ///     ProcessInstanceManager移除过程实例参数
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public void RemoveProcessInstance(string pid)
        {
            try
            {
                var processInstance = ProcessInstances.FirstOrDefault(a => a.Pid == pid);
                
                ProcessInstances.Remove(processInstance);

                Log.Info($"成功清除Pid:{pid}」的ProcessInstance, ProcessInstanceManager的ProcessInstances剩余数目为[{ProcessInstances.Count}]");
            }
            catch (Exception e)
            {
                Log.Error($"ProcessInstanceManager移除过程实例参数失败，{e.Message}");
            }
        }

        public ProcessInstance GetProcessInstance(string pid)
        {
            try
            {
                var processInstance = ProcessInstances.FirstOrDefault(a => a.Pid == pid);

                return processInstance;
            }
            catch (Exception e)
            {
                Log.Error($"ProcessInstanceManager获得实例失败，{e.Message}");
                return null;
            }
        }

        public bool RunInstance(string processName)
        {
            return ProcessInstances.Any(a => a.ProcessName == processName);
        }

        public int GetProcessInstancesNumber(string processName)
        {
            return ProcessInstances.Count(a => a.ProcessName == processName);
        }

        public IEnumerable<ProcessInstance> GetProcessInstances(string processName)
        {
            try
            {
                return ProcessInstances.Where(a => a.ProcessName == processName).ToList();
            }
            catch (Exception e)
            {
                Log.Error($"获取[{processName}]正在运行的所有实例失败，异常为:[{e.Message}]");
                return new List<ProcessInstance>();
            }
        }
    }
}