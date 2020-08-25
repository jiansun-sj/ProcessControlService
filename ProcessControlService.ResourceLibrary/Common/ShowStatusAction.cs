using System;
using System.Xml;
using ProcessControlService.ResourceLibrary.Action;
using log4net;


namespace ProcessControlService.ResourceLibrary.Common
{
    /// <summary>
    /// 显示一些系统参数，
    /// 1. Redundancy模式
    /// Created by David Dong 20180311
    /// </summary>
    /// <StepAction Action="WaitAction" WaitTime="500"/>
    ///
    public class ShowStatusAction : BaseAction
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ShowStatusAction));

        public ShowStatusAction(string Name):base(Name)
        {
        }


        public override bool LoadFromConfig(XmlNode node)
        {
            throw new NotImplementedException();
        }

        #region "core functions"

        public override void Execute()
        {
            //LOG.Info(string.Format("----------------ShowStatusAction::正在以{0}方式运行", Mode.ToString()));
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        protected override bool CreateParameters()
        {
            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new ShowStatusAction(Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone()
            };
            return basAction;

        }

        #endregion



    }
}
