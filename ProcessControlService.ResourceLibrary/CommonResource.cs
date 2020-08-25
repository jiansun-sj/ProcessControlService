using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary
{
    public class CommonResource : IResource,IActionContainer
    {
        private string _commonResourceName;

        #region IResource 接口实现
        public string ResourceName
        { 
            get 
            {
                return _commonResourceName;
            } 
        }

        public string ResourceType
        {
            get
            {
                return "CommonResource";
            }
        }

        public IResource GetResourceObject()
        {
            return this;
        }

        public bool LoadFromConfig(XmlNode node)
        {
            return true;
        }

        public IResourceExportService GetExportService()
        {
            return null;
        }

        public void StartWork()
        { }

        private RedundancyMode _mode = RedundancyMode.Unknown;

        public RedundancyMode Mode
        {
            get { return _mode; }
        }

        public void RedundancyModeChange(RedundancyMode Mode)
        {
            _mode = Mode;
        }

        public void FreeResource()
        { }
        #endregion

        #region "IActionContainer接口实现"

        private ActionCollection _actions = new ActionCollection();

        //public abstract void CreateActions();

        public BaseAction GetAction(string name)
        {
            return (BaseAction)_actions.GetAction(name);
        }

        public void AddAction(BaseAction action)
        {
            _actions.AddAction(action);
        }

        public virtual void ExecuteAction(string name)
        {
            BaseAction _action = (BaseAction)GetAction(name);
            _action.Execute(_mode);
        }

        public string[] ListActionNames()
        {
            return _actions.ListActionNames();
        }

        #endregion

        public CommonResource(string Name)
        {
            _commonResourceName = Name;
       
        }

    }
}
