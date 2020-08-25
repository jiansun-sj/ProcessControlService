// ==================================================
// 文件名：EventCondition.cs
// 创建时间：2019/10/30 15:47
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:47
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceLibrary.Event;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    /// <summary>
    ///     事件触发条件
    ///     Created by David Dong
    ///     Created at 20180809
    /// </summary>
    public class EventCondition : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventCondition));

        private BaseEvent _event;

        public EventCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        public override bool LoadFromConfig(XmlElement level1Item)
        {
            try
            {
                var strContainer = level1Item.GetAttribute("Container");
                var strEvent = level1Item.GetAttribute("Event");

                var container = EventsManagement.GetContainer(strContainer);
                _event = container.GetEvent(strEvent);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"装载EventCondition {Name}时出错：{ex}");
                return false;
            }
        }

        public override bool CheckReady()
        {
            return _event != null && _event.IsTriggered();
        }
    }
}