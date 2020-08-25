using System;
using System.Timers;
using System.Xml;
using ProcessControlService.ResourceLibrary.Action;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;
using Timer = System.Timers.Timer;


namespace ProcessControlService.ResourceLibrary.Common
{
    /// <summary>
    /// 等待动作
    /// Created by David Dong 20170619
    /// </summary>
    /// <StepAction Container="CommonResource" Action="WaitAction">
	///	    <InParameter ActionParameter="WaitTime" ConstValue="5000"/>
	/// </StepAction>
    ///
    public class WaitAction : BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaitAction));

        public WaitAction(string name):base(name)
        {
        }

        private const int MaxWaitingSeconds = 1800*1000;

        public override bool LoadFromConfig(XmlNode node)
        {
            throw new NotImplementedException();
        }

        private readonly Timer _timer=new Timer();

        private bool _timeOut;

        #region "core functions"

        public override void Execute()
        {
            var waitTime = (int)ActionInParameterManager["WaitTime"].GetValue();
            if (waitTime > 0 && waitTime <= MaxWaitingSeconds)
            {
                _timeOut = false;

                _timer.Interval = waitTime;

                _timer.Start();

                _timer.Elapsed += TimeOn;
            }
            else
            {
                Log.Error("WaitAction的等待时间不支持小于0或大于1800的秒数");
                throw new NotSupportedException();
            }
        }

        private void TimeOn(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            _timeOut = true;
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override bool IsFinished()
        {
            return _timeOut;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        protected override bool CreateParameters()
        {
            ActionInParameterManager.AddBasicParam(new BasicParameter<int>("WaitTime"));
            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new WaitAction(Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;

        }

        public override void Break()
        {
            _timer.Stop();
            
            _timeOut = true;

            Log.Info($"WaitAction触发BreakCondition中断，定时器已停止。");
        }

        #endregion

       

    }
}
