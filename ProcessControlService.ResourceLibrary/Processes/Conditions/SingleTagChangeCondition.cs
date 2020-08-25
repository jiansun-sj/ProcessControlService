// ==================================================
// 文件名：SingleTagChangeCondition.cs
// 创建时间：2019/11/23 15:47
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:47
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    /// <summary>
    ///     当tag的值发生变化时触发，拉动系统用，仅支持int32，int16
    ///     时间：2019年7月11日 17:59:31  夏平安
    /// </summary>
    public class SingleTagChangeCondition : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SingleTagChangeCondition));
        private bool _first = true;
        private bool _ignoreFirst;

        private Tag _lastTag;
        private Tag _tag;

        public SingleTagChangeCondition(string name, Process owner)
            : base(name, owner)
        {
        }
        
        public override bool CheckReady()
        {
            return ValueChanged();
        }

        private bool ValueChanged()
        {
            try
            {
                if (_tag.TagValue == null) return false; // 忽略启动时tag值是初始值的情况
                if (_first)
                {
                    _first = false;

                    if (_ignoreFirst)
                    {
                        _lastTag.TagValue = _tag.TagValue;
                        return false; // 忽略启动时的变化
                    }
                }

                var currentTagValue = Convert.ToInt32(_tag.TagValue);  //强转会失败，改为Convert sunjian 2020-1-4
                var lastTagValue = Convert.ToInt32(_lastTag.TagValue);

                if (currentTagValue == lastTagValue) return false;
                
                _lastTag.TagValue = currentTagValue;
                    
                return currentTagValue  > 0;
            }
            catch (Exception ex)
            {
                Log.Error($"检查SingleTagChangeCondition时出错：{ex.Message}.");
                return false;
            }
        }

        public override bool LoadFromConfig(XmlElement level1Item)
        {
            var strName = level1Item.GetAttribute("Name");
            var strMachine = level1Item.GetAttribute("Container");
            var strTag = level1Item.GetAttribute("Tag");

            var machine = (Machines.Machine) ResourceManager.GetResource(strMachine);
            if (machine == null)
            {
                Log.Error($"机器{strMachine}装载出错.");
                return false; // add by dongmin 20170517
            }

            var conditionTag = machine.GetTag(strTag);
            if (conditionTag == null)
            {
                Log.Error($"条件Tag：{strTag}装载出错.");
                return false; // add by dongmin 20170517
            }

            if (conditionTag.TagType != "int16" && conditionTag.TagType != "int32")
            {
                Log.Error($"触发条件Tag:{conditionTag.TagName}不是Int量，出错！");
                return false;
            }

            //_conditionTag.SubscribeValueChange(OnTriggered);
            if (level1Item.HasAttribute("IgnoreFirst")) // 忽略第一次变化 - David 20170716
            {
                var strIgnoreFirst = level1Item.GetAttribute("IgnoreFirst");

                _ignoreFirst = Convert.ToBoolean(strIgnoreFirst);
                _tag = machine.GetTag(strTag);
                _lastTag = (Tag) _tag.Clone();
                _lastTag.TagValue = 0;
            }
            else
            {
                _tag = machine.GetTag(strTag);
                _lastTag = (Tag) _tag.Clone();
                //_lastTag.TagValue = 0;
            }

            return true;
        }
    }
}