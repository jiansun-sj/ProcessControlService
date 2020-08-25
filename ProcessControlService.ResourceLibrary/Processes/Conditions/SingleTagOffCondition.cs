// ==================================================
// 文件名：SingleTagOffCondition.cs
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
    public class SingleTagOffCondition : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SingleTagOffCondition));
        private bool _first = true;

        private bool _ignoreFirst;
        private Tag _lastTag;
        private Tag _tag;


        public SingleTagOffCondition(string name, Process owner)
            : base(name, owner)
        {
            //_tag = tag;
            //_lastTag = (Tag)_tag.Clone();
            //_lastTag.TagValue = true;
        }


        public override bool LoadFromConfig(XmlElement level1_item)
        {
            var strName = level1_item.GetAttribute("Name");

            var strMachine = level1_item.GetAttribute("Container");
            var strTag = level1_item.GetAttribute("Tag");

            var _machine = (Machines.Machine) ResourceManager.GetResource(strMachine);

            if (_machine == null)
            {
                Log.Error(string.Format("机器{0}装载出错.", strMachine));
                return false; // add by dongmin 20170517
            }

            var _conditionTag = _machine.GetTag(strTag);
            if (_conditionTag == null)
            {
                Log.Error(string.Format("条件Tag：{0}装载出错.", strTag));
                return false; // add by dongmin 20170517
            }

            if (_conditionTag.TagType != "bool")
            {
                Log.Error(string.Format("触发条件Tag:{0}不是bool量，出错！", _conditionTag.TagName));
                return false;
            }

            if (level1_item.HasAttribute("IgnoreFirst")) // 忽略第一次变化 - David 20170716
            {
                var strIgnoreFirst = level1_item.GetAttribute("IgnoreFirst");

                _ignoreFirst = Convert.ToBoolean(strIgnoreFirst);
                _tag = _machine.GetTag(strTag);
                _lastTag = (Tag) _tag.Clone();
                _lastTag.TagValue = true;
            }
            else
            {
                _tag = _machine.GetTag(strTag);
                _lastTag = (Tag) _tag.Clone();
                _lastTag.TagValue = true;
            }

            return true;
        }

        // check if Tag value change
        public override bool CheckReady()
        {
            //_tag.Read();

            return ValueChangedToOff();
        }

        private bool ValueChangedToOff()
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

                if (_tag.TagValue != null) //Robin20170724
                    if (!Tag.ValueEqual(_tag, _lastTag))
                    {
                        if ((bool) _lastTag.TagValue)
                        {
                            _lastTag.TagValue = _tag.TagValue;
                            return true;
                        }

                        _lastTag.TagValue = _tag.TagValue;
                    }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("检查SingleTagOffCondition时出错：{0}.", ex.Message));
                return false;
            }
        }
    }
}