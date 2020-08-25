using System.Collections.Generic;
using System.Linq;

namespace ProcessControlService.ResourceLibrary.Action
{
    public class ActionCollection
    {
        private readonly Dictionary<string, BaseAction> Actions = new Dictionary<string, BaseAction>();

        public void AddAction(BaseAction action)
        {
            Actions.Add(action.Name,action);
        }

        public BaseAction GetAction(string name)
        {
            return Actions[name];
        }

        //public virtual void ExecuteAction(string name)  注释于20180426因为没有引用
        //{
        //    BaseAction _action = GetAction(name);
        //    _action.Execute();
        //}

        public string[] ListActionNames()
        {
            return Actions.Keys.ToArray();
        }

        //added by sunjian 2019-12-24
        public Dictionary<string, BaseAction> GetActions()
        {
            return Actions;
        }

    }
}
