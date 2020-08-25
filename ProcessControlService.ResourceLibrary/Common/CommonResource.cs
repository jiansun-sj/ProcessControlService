using System;
using System.Collections.Generic;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Common
{
    public sealed class CommonResource : Resource,IActionContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CommonResource));

        public static int Count;

        #region IResource 接口实现


        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name == "Actions")
                        LoadActionsFromXml(subNode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
            
            return true;
        }

        private void LoadActionsFromXml(XmlNode subNode)
        {
            foreach (XmlNode ac in subNode)
            {
                if (ac is XmlComment || ac.Name != "Action") continue;
                var level2Item = (XmlElement)ac;
                var name = level2Item.GetAttribute("Name");
                var actionType = level2Item.GetAttribute("Type");
                var action = ActionsManagement.CreateAction(actionType, name);

                try
                {
                    action.LoadFromConfig(level2Item);
                    action.ActionContainer = this;
                    AddAction(action);
                }
                catch (Exception e)
                {
                    Log.Error($"加载CommonResource: {ResourceName} Action:{name}出错,{e.Message}");
                }
            }
        }


        public override void FreeResource()
        { }
        #endregion

        #region "IActionContainer接口实现"
        // 与其它类IActionContainer不同

        private readonly List<string> _actionNames = new List<string>();

        private readonly Dictionary<string, BaseAction> _actions = new Dictionary<string, BaseAction>();

        //public abstract void CreateActions();

        public BaseAction GetAction(string name)
        {
            if (_actionNames.Contains(name))
            {
                var action = ActionsManagement.CreateAction(name, name);
                action.ActionContainer = this;
                return action;
            }

            return _actions[name];
        }

        public void AddAction(BaseAction action)
        {
            try
            {
                _actions.Add(action.Name, action);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void ExecuteAction(string name)
        {
            throw new NotSupportedException();
        }

        public string[] ListActionNames()
        {
            return _actionNames.ToArray();
        }

        private void RegisterActions()
        {
            // add actions
      
            _actionNames.Add(nameof(WaitAction));
            _actionNames.Add(nameof(ShowStatusAction));
            _actionNames.Add(nameof(CallProcessAction));
            _actionNames.Add(nameof(TwoParameterCalculateAction)); // add by dongmin 20190810

            _actionNames.Add(nameof(SelectNextParameterItemFromArrayAction)); //add by sunjian 2019-12-23
            _actionNames.Add(nameof(ConfigResourceDicAction));//add by sunjian 2020-06-09
            _actionNames.Add(nameof(LimitCurrentProcessInstanceNumber));//add by sunjian 2020-06-011
        }

        #endregion

        public CommonResource(string name) : base(name)
        {
            RegisterActions();
        }

    }
}
