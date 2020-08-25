// ==================================================
// 文件名：TimerCondition.cs
// 创建时间：2019/10/30 15:48
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:48
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;

//using ProcessControlService.Models;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    /// <summary>
    ///     定时触发条件
    ///     Created by David Dong
    ///     Created at 20170328
    /// </summary>
    public class TimerCondition : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TimerCondition));

        private int _accumulateSeconds;
        private bool _first = true; //Robin0521改
        private int _interval;
        private bool _switch;

        public TimerCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        public override bool LoadFromConfig(XmlElement level1Item)
        {
            var strInterval = level1Item.GetAttribute("Interval_0.1s");
            var strInit = level1Item.GetAttribute("Init");

            _interval = Convert.ToInt32(strInterval);

            if (strInit.ToLower() == "true") // 启动定时器事件
                Start();

            return true;
        }

        public override bool CheckReady()
        {
            if (_switch)
            {
                _accumulateSeconds++;

                if (_accumulateSeconds >= _interval)
                {
                    if (_first) //Robin0521改
                    {
                        Log.Debug($"定时条件{Name}触发.");
                        _first = false;
                    }

                    _accumulateSeconds = 0;

                    return true;
                }
            }

            return false;
        }

        public override void Reset()
        {
            base.Reset();
            _switch = false;
        }


        // 启动定时器条件
        public void Start()
        {
            _switch = true;
        }

        // 关闭定时器条件
        public void Stop()
        {
            _switch = false;
        }
    }
}