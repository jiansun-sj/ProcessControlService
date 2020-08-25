using NLog;
using System.Collections.Generic;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Storage.Rules
{
    public abstract class RuleBase<T> where T : Storage
    {
        protected Logger LOG;
        protected T OwnerStorage { get; private set; }
        public RuleBase(T storage)
        {
            OwnerStorage = storage;
            LOG = LogManager.GetLogger($"Storage/{storage.StorageName}");
            LOG.SetProperty("filename", storage.StorageName);
        }
        public abstract string RuleName { get; set; }

        public virtual bool SetRuleFeature(Dictionary<string, string> dic) { return true; }
        public virtual string GetRuleFeature() { return ""; }

        public virtual bool LoadFormConfig(XmlElement ruleSet)
        {
            return true;
        }
    }
}
