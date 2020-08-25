using System;
using System.Timers;
using log4net;
using ProcessControlService.ResourceLibrary.Common;

namespace ProcessControlService.ResourceLibrary.Machines
{
   public class MachineWaitAction: MachineAction
    {
        private const int MaxWaitingSeconds = 1800 * 1000;

        private static readonly ILog Log = LogManager.GetLogger(typeof(WaitAction));

        private readonly Timer _timer = new Timer();

        private bool _timeOut;

        public MachineWaitAction(string name) : base(name)
        {
        }

        public override void Execute()
        {
            var waitTime = Convert.ToInt32(ActionInParameterManager["WaitTime"].GetValue());
            if (waitTime > 0 && waitTime <= MaxWaitingSeconds)
            {
                _timeOut = false;

                _timer.Interval = waitTime;

                _timer.Start();

                _timer.Elapsed += TimeOn;

                /*while (!_timeOut)
                {
                    Thread.Sleep(100);
                }*/
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

        public override void Break()
        {
            _timer.Stop();

            _timeOut = true;

            Log.Info($"WaitAction触发BreakCondition中断，定时器已停止。");
        }
    }
}
