using System;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;
using log4net;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory.ParameterType;


namespace ProcessControlService.ResourceLibrary.Common
{
    /// <summary>
    /// 参数计算
    /// Created by David Dong 20190810
    /// </summary>
    /// <StepAction Container="Common" Action="TwoParameterCalculateAction">
	///	    <InParameter ProcessParameter="ProcessParameter1" ActionParameter="ActionParameter1"/>
    ///	    <InParameter ProcessParameter="ProcessParameter1" ActionParameter="ActionParameter2"/>
    ///	    <InParameter ActionParameter="CalculateType" ConstValue="Add"/>
    ///	    <OutParameter ProcessParameter="ResultProcessParameter" ActionParameter="ResultParameter"/>
	/// </StepAction>
    ///
    public class TwoParameterCalculateAction : BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TwoParameterCalculateAction));
        
        public TwoParameterCalculateAction(string name) : base(name)
        {
            //_name = Name;
            //_waitTime = WaitTime;
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            throw new NotImplementedException();
            //return CreateParameters();
        }

        #region "core functions"

        public override void Execute()
        {

            try
            {
                var initValue1 = ActionInParameterManager["ActionParameter1"];
                var value1 = initValue1.GetValue();
                var initValue2 = ActionInParameterManager["ActionParameter2"];
                var value2 = initValue2.GetValue();

                var calculateType = BasicValueCalculateD2.GetCalculateType(ActionInParameterManager["CalculateType"].GetValueInString());

                var value3 = BasicValueCalculateD2.Calculate(calculateType, value1, value2);

                FeedBacks.Add(new Message{Description = $"参数Value1：[{value1}],Value2：[{value2}],Value3：[{value3}]"});
               
                ActionOutParameterManager["ResultParameter"].SetValue(value3);

            }
            catch (Exception ex)
            {
                Log.Error($"执行TwoParameterCalculateAction 出错{ex}");
            }
        }

        private bool CheckParameterTypeSupport(Parameter par)
        {
            var strType = par.GetTypeString();
            if (strType == "int16" || strType == "int32" || strType == "int") //暂时支持这些 David
            {
                return true;
            }

            Log.Error($"参数计算不支持改类型。参数：{par.Name} 类型：{strType}");
            return false;
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
            ActionInParameterManager.AddBasicParam(new BasicParameter<dynamic>("ActionParameter1"));
            ActionInParameterManager.AddBasicParam(new BasicParameter<dynamic>("ActionParameter2"));
            ActionInParameterManager.AddBasicParam(new BasicParameter<string>("CalculateType", "string"));
            ActionOutParameterManager.AddBasicParam(new BasicParameter<dynamic>("ResultParameter"));
            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new TwoParameterCalculateAction(Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;
        }

        #endregion
    }

  
}
