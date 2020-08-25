using System;
using System.Xml;
using ProcessControlService.ResourceLibrary.Action;

//using ProcessControlService.Models;

// 修改记录 
// 
namespace ProcessControlService.ResourceLibrary.DataBinding
{
/*
    public class GroupBindingAction: BaseAction
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GroupBindingAction));

        public GroupBindingAction(string ActionName,GroupDataBinding owner) : base(ActionName)
        {
            OwnerDataBinding = owner;
        }
        public GroupDataBinding OwnerDataBinding
        {
            get => (GroupDataBinding)ActionContainer;
            set => ActionContainer = value;
        }

        public override void Execute()
        {
            OwnerDataBinding.ExecuteBinding();
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
            throw new NotImplementedException();
        }
    }
*/


    public class BindingAction : BaseAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(BindingAction));

        public BindingAction(string ActionName, DataBinding owner)
            : base(ActionName)
        {
            OwnerDataBinding = owner;
        }

        public DataBinding OwnerDataBinding
        {
            get { return (DataBinding)ActionContainer; }
            set { ActionContainer = value; }
        }

        #region "继承接口"
        
        public override BaseAction Clone()
        {
            var basAction = new BindingAction(Name,OwnerDataBinding)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;

        }

        public override void Execute()
        {
            OwnerDataBinding.ExecuteBinding();

        }


        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

  
}
