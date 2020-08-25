// ==================================================
// 文件名：SingleTagOnCondition.cs
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
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    public class SingleTagOnCondition : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SingleTagOnCondition));
        private bool _first = true;
        private bool _ignoreFirst;

        private Tag _lastTag;
        private Tag _tag;

        public SingleTagOnCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        //<Condition Type = "SingleTagOnCondition" Name="扫码枪开始扫码信号触发" Container="SoueastMachine" Tag="ScannerInpos_RTT2"/>

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

            if (conditionTag.TagType != "bool")
            {
                Log.Error($"触发条件Tag:{conditionTag.TagName}不是bool量，出错！");
                return false;
            }

            if (level1Item.HasAttribute("IgnoreFirst")) // 忽略第一次变化 - David 20170716
            {
                var strIgnoreFirst = level1Item.GetAttribute("IgnoreFirst");

                _ignoreFirst = Convert.ToBoolean(strIgnoreFirst);
                _tag = machine.GetTag(strTag);
                _lastTag = (Tag) _tag.Clone();
                _lastTag.TagValue = false;
            }
            else
            {
                _tag = machine.GetTag(strTag);
                _lastTag = (Tag) _tag.Clone();
                _lastTag.TagValue = false;
            }

            return true;
        }

        // check if Tag value change
        public override bool CheckReady()
        {
            //_tag.Read();

            return ValueChangedToOn();
        }

        private bool ValueChangedToOn()
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

                if (_tag.TagValue == null) return false;
                if (Tag.ValueEqual(_tag, _lastTag)) return false;
                if ((bool) _lastTag.TagValue == false)
                {
                    _lastTag.TagValue = _tag.TagValue;
                    return true;
                }

                _lastTag.TagValue = _tag.TagValue;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"检查SingleTagOnCondition时出错：{ex.Message}.");
                return false;
            }
        }
    }
}