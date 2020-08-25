using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.ResourceTemplate;

namespace ProcessControlService.ResourceLibrary.Processes.Steps
{
    public class StepAction:IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Step));

        private string _actionName;

        private string _bindingDictionaryName;
        private string _bindKey;

        private bool _bindToResourceTemplate;

        public BaseAction Action { get; set; }

        public void InitFromXml(XmlElement xmlElement)
        {
            var containerName = xmlElement.GetAttribute("Container");
            _actionName = xmlElement.GetAttribute("Action");

            //step action的container绑定了resource Template， sunjian 2019-12-25

            if (containerName.Contains("{Using"))
            {
                _bindToResourceTemplate = true;

                var trimmedContainerString = containerName.Trim('{').Trim('}').Replace(" ", "").Trim();

                var containerAttribute = trimmedContainerString.Split(',');

                var resourceTemplateName = containerAttribute[0].Substring("Using".Length,
                    containerAttribute[0].Length - "Using".Length);

                _bindingDictionaryName = containerAttribute[1]
                    .Substring("Binding{".Length, containerAttribute[1].Length - "Binding{".Length);

                _bindKey = containerAttribute[2].Substring("Key=".Length, containerAttribute[2].Length - "Key=".Length);

                var resourceTemplate =
                    (MachineResourceTemplate) ResourceManager.GetResourceTemplate(resourceTemplateName);

                if (!resourceTemplate.HasAction(_actionName))
                    Log.Error($"ResourceTemplate:[{resourceTemplate}]中不存Action:[{_actionName}]");
            }

            else if (!string.IsNullOrEmpty(containerName))
            {
                _bindToResourceTemplate = false;
                var container = ActionsManagement.GetContainer(containerName);
                Action = container.GetAction(_actionName);
            }
        }

        public bool CheckResult()
        {
            return Action == null || Action.IsFinished();
            //sunjian 2019-11-26
        }

        public void AssignAction(ParameterManager processParameterManager)
        {
            //if (!_bindToResourceTemplate) return;
            var dictionaryParameter = processParameterManager.GetDictionaryParam(_bindingDictionaryName);

            var selectedResource = dictionaryParameter.GetValue(_bindKey).ToString();

            var resource =  ActionsManagement.GetContainer(selectedResource);

            var baseAction = resource.GetAction(_actionName);

            Action = baseAction.Clone();
            Action.ActionContainer = baseAction.ActionContainer;

            Log.Info($"当前StepAction绑定的是Machine：[{selectedResource}]的Action:[{_actionName}].");
        }

        public StepAction Create(ParameterManager processParameterManager)
        {
            try
            {
                var stepAction = new StepAction
                    {_actionName = _actionName, _bindKey = _bindKey, _bindingDictionaryName = _bindingDictionaryName};

                if (_bindToResourceTemplate)
                {
                    stepAction.AssignAction(processParameterManager);
                }

                else
                {
                    stepAction.Action = Action.Clone();
                    stepAction.Action.ActionContainer = Action.ActionContainer;
                }

                return stepAction;
            }
            catch (Exception e)
            {
                Log.Error($"创建StepAction实例失败，Step名称为：【{_actionName}】,异常为{e.Message}");
                return null;
            }
        }

        public void Execute()
        {
            Action.Execute();
        }

        public void Break()
        {
            Action.Break();
        }

        public bool IsSuccessful()
        {
            return Action.IsSuccessful();
        }

        public void Dispose()
        {
            Action?.Dispose();
        }
    }
}