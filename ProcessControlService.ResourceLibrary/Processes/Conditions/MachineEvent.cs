// ==================================================
// 文件名：MachineEvent.cs
// 创建时间：2019/10/30 15:47
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:47
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Xml;
using log4net;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    /// <summary>
    ///     MachineEvent类
    ///     描述机器触发事件
    ///     Created by: DongMin 20170325
    /// </summary>
    public class MachineEvent : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MachineEvent));

        private readonly List<CheckTagEventItem> _checkTags = new List<CheckTagEventItem>();

        public readonly Machine OwnerMachine;

        public MachineEvent(Machine ownerMachine, string name, Process ownerProcess)
            : base(name, ownerProcess)
        {
            OwnerMachine = ownerMachine;
        }

        public override bool LoadFromConfig(XmlElement node)
        {
            try
            {
                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "ActionItem"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    if (level1Item.Name.ToLower() == "event" + "item")
                    {
                        var strType = level1Item.GetAttribute("Type");
                        var strTag = level1Item.GetAttribute("Tag");
                        var strConstValue = level1Item.GetAttribute("ConstValue");

                        var tag = OwnerMachine.GetTag(strTag);
                        var constValue = Convert.ToInt32(strConstValue);

                        if (string.Equals(strType, "CheckTag", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var checkTag = new CheckTagEventItem(tag, constValue);
                            _checkTags.Add(checkTag);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        public void AddCheckTagEventItem(CheckTagEventItem EventItem)
        {
            _checkTags.Add(EventItem);
        }


        #region "接口实现"

        public override bool CheckReady()
        {
            if (CheckTags())
            {
                Log.Info($"检查过程Tag触发条件{Name}.");

                return true;
            }

            return false;
        }

        #endregion

        private bool CheckTags()
        {
            foreach (var checkItem in _checkTags)
                if (!checkItem.Check())
                    return false;
            return true;
        }
    }

    public class CheckTagEventItem
    {
        private readonly Tag _tag;
        private readonly Tag _lastTag;
        private readonly object _targetValue;

        public CheckTagEventItem(Tag tag, object Value)
        {
            _tag = tag;
            _targetValue = Value;
            _lastTag = (Tag) _tag.Clone();
        }

        public bool Check()
        {
            if (!Tag.ValueEqual(_tag, _lastTag))
            {
                if ((bool) _lastTag.TagValue == false)
                {
                    _lastTag.TagValue = _tag.TagValue;

                    return Tag.ValueEqual(_tag, _targetValue);
                }

                _lastTag.TagValue = _tag.TagValue;
            }

            return false;
        }
    }
}