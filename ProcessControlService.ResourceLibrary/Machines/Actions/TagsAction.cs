// ==================================================
// 文件名：TagsAction.cs
// 创建时间：2019/11/13 12:37
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/31 12:37
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Xml;
using log4net;
using ProcessControlService.ResourceLibrary.Action;

//using ProcessControlService.Models;

// 修改记录 
// 20170528 David 增加了带条件执行ActionItem的功能
namespace ProcessControlService.ResourceLibrary.Machines.Actions
{
    public class TagsAction : MachineAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TagsAction));


        public TagsAction(string actionName)
            : base(actionName)
        {
        }

        #region "Parameter"

        //protected List<TagActionItem> _actionItems = new List<TagActionItem>();

        private readonly List<ReadTagActionItem> _readTags = new List<ReadTagActionItem>();
        private readonly List<WriteTagActionItem> _writeTags = new List<WriteTagActionItem>();
        private readonly List<CheckTagActionItem> _checkTags = new List<CheckTagActionItem>();

        // 增加条件执行功能 20170528
        //public readonly ParameterManager ConditionParameters = new ParameterManager();

        private bool? _actionSuccess; //sunjian 2019-11-26
        private bool _first = true; //Robin0521改

        private CheckResultType _checkResultType = CheckResultType.AllTag; // add by David 20170824

        protected override bool CreateParameters()
        {
            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new TagsAction(Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone()
            };

            basAction.InitializeNewAction(_readTags, _writeTags, _checkTags, _checkResultType);

            return basAction;
        }

        private void InitializeNewAction(IEnumerable<ReadTagActionItem> readTags,
            IEnumerable<WriteTagActionItem> writeTags, IEnumerable<CheckTagActionItem> checkTags,
            CheckResultType checkResultType)
        {
            foreach (var readTagActionItem in readTags)
            {
                _readTags.Add(readTagActionItem);
            }

            foreach (var writeTagActionItem in writeTags)
            {
                _writeTags.Add(writeTagActionItem);
            }
            foreach (var checkTagActionItem in checkTags)
            {
                _checkTags.Add(checkTagActionItem);
            }

            _checkResultType = checkResultType;
        }

        public override void Dispose()
        {
            _readTags.Clear();
            _writeTags.Clear();
            _checkTags.Clear();
            base.Dispose();
        }

        #endregion

        #region "继承接口"

        //<Action Type="TagsAction" Name="Action1" CheckResultType="AllTag">
        //    <ActionItem Type="WriteTag" Tag="Tag1" ConstValue="true" />
        //    <ActionItem Type="CheckTag" Tag="Tag2" ConstValue="true" />
        //    <ActionItem Type="CheckTag" Tag="Tag3" ConstValue="true" /> 
        //</Action>
        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                if (!base.LoadFromConfig(node))
                    return false;

                // 增加返回值检查类型功能 Dongmin 20170804
                var level0Item = (XmlElement) node;
                if (level0Item.HasAttribute("CheckResultType"))
                {
                    var strCheckResultType = level0Item.GetAttribute("CheckResultType");
                    if (strCheckResultType.ToLower() == "anytag") _checkResultType = CheckResultType.AnyTag;
                }

                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "Parameter", "StepAction"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    if (string.Equals(level1Item.Name, "ActionItem", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // 创建ActionItem
                        var actionItemType = level1Item.GetAttribute("Type");

                        if (string.Equals(actionItemType, "ReadTag", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var actionItem = new ReadTagActionItem();
                            if (!actionItem.LoadFromConfig(level1Item, this))
                                return false;
                            if (actionItem.Tag.AccessType == TagAccessType.Write)
                            {
                                Log.Error($"装载机器TagsAction:{Name},Machine:[{OwnerMachine.ResourceName}]装载Tag:{actionItem.Tag.TagName},Tag访问类型为[只写],不能用作ReadTag。");
                            }
                            _readTags.Add(actionItem);
                        }
                        else if (string.Equals(actionItemType, "WriteTag", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var actionItem = new WriteTagActionItem();
                            if (!actionItem.LoadFromConfig(level1Item, this))
                                return false;
                            _writeTags.Add(actionItem);
                        }
                        else if (string.Equals(actionItemType, "CheckTag", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var actionItem = new CheckTagActionItem();
                            if (!actionItem.LoadFromConfig(level1Item, this))
                                return false;
                            _checkTags.Add(actionItem);
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

        public override void Execute()
        {
            /*if (mode == RedundancyMode.Slave || mode == RedundancyMode.Unknown)
            {
                LOG.Info($"进入TagAction,名称：{Name},冗余模式为{mode}，不执行该操作。");
                return;
            }*/

            _actionSuccess = null; //sunjian 2019-11-26

            if (_first) //Robin0521改
            {
                Log.Debug($"执行机器：{OwnerMachine.ResourceName} 动作：{Name}.");
                _first = false;
            }

            // 读Tags
            foreach (var tg in _readTags)
            {
                if (!tg.ExecuteConditionCheck()) //  added by dongmin 20170528
                    continue;

                if (!tg.Read(ActionOutParameterManager)) // added by dongmin 20170517
                    _actionSuccess = false;
            }

            // 写Tags
            foreach (var tg in _writeTags)
            {
                if (!tg.ExecuteConditionCheck()) //  added by dongmin 20170528
                    continue;
                if (!tg.Write(ActionInParameterManager)) // added by dongmin 20170517
                {
                    _actionSuccess = false;
                }
            }

            JudgeIsActionSuccessFul();
        }

        private void JudgeIsActionSuccessFul()
        {
            if (_actionSuccess == null) //sunjian 2019-11-26  _actionSuccess 为null,之前过程没有出错
            {
                if (_checkResultType == CheckResultType.AllTag)
                {
                    // 所有检测点都通过 Add by dongmin 20170517
                    foreach (var tg in _checkTags)
                    {
                        if (!tg.ExecuteConditionCheck()) //  added by dongmin 20170528
                            continue;

                        if (!tg.Check(ActionInParameterManager)) _actionSuccess = false;
                    }

                    if (_actionSuccess != false)
                        _actionSuccess = true;
                }
                else
                {
                    // 单个检测点通过即可 20170804 Dongmin
                    foreach (var tg in _checkTags)
                    {
                        if (!tg.ExecuteConditionCheck())
                            continue;

                        if (tg.Check(ActionInParameterManager)) _actionSuccess = true;
                    }

                    if (_actionSuccess != true) _actionSuccess = false;
                }
            }
            else
            {
                _actionSuccess = false;
            }
        }

        public override bool IsSuccessful()
        {
            return _actionSuccess == true;
        }

        public override bool IsFinished()
        {
            return _actionSuccess != null;
        }


        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public enum CheckResultType
    {
        AnyTag = 0,
        AllTag = 1
    }
}