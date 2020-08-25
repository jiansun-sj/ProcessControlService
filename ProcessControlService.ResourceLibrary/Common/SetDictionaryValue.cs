using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Common
{
    public class SetDictionaryValueAction:BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SetDictionaryValueAction));

        public SetDictionaryValueAction(string actionName) : base(actionName)
        {
        }

        public override void Execute()
        {
            try
            {
                var dictionaryParameter = ActionOutParameterManager.GetDictionaryParam("DictionaryParameter");

                foreach (var basicParameter in ActionInParameterManager.BasicParameters)
                {
                    dictionaryParameter.Clear();
                    dictionaryParameter.Add(basicParameter.Key,basicParameter.Value);
                }
            }
            catch (Exception e)
            {
                Log.Error($"设置字典型参数失败，异常：{e.Message}.");
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

        public override BaseAction Clone()
        {
            var basAction = new SetDictionaryValueAction(Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone()
            };

            return basAction;
        }
    }
}
