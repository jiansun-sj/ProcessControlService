// ==================================================
// 文件名：MultiTagsCondition.cs
// 创建时间：2020/06/16 10:47
// ==================================================
// 最后修改于：2020/07/21 10:47
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    /// <summary>
    ///     sunjian 2020/3/12 extracted
    /// </summary>
    public partial class MultiTagsCondition : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MultiTagsCondition));

        public readonly List<Tag> LastTags = new List<Tag>();

        public readonly Dictionary<TagModel, Tag> MonitorTagChangeDic = new Dictionary<TagModel, Tag>();

        public readonly List<TagModel> MonitorTags = new List<TagModel>();

        public IBasicParameter OutMachineName;
        public IBasicParameter OutTagName;
        
        protected MultiTagsCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        protected MultiTagsCondition()
        {
        }

        public override bool LoadFromConfig(XmlElement node)
        {
            try
            {
                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "TrigTags"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    if (string.Equals(level1Item.Name, "TrigTags", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (XmlNode level2Node in level1Node)
                        {
                            if (level2Node.NodeType == XmlNodeType.Comment)
                                continue;

                            var level2Item = (XmlElement) level2Node;
                            if (string.Equals(level2Item.Name, "TrigTag", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // level2 --  "TrigTag"
                                var strMachine = level2Item.GetAttribute("Machine");
                                var strTag = level2Item.GetAttribute("Tag");

                                var machine = (Machine) ResourceManager.GetResource(strMachine);
                                var tag = machine.GetTag(strTag);

                                if (tag != null)
                                {
                                    /*if (tag.TagType != "bool")
                                        throw new Exception($"装载MultiTagsOnCondition时Tag{strTag}不是BOOL量");*/

                                    MonitorTags.Add(new TagModel {Tag = tag});

                                    var lastTag = (Tag) tag.Clone();
                                    LastTags.Add(lastTag);

                                    MonitorTagChangeDic.Add(new TagModel {Tag = tag}, lastTag);
                                }
                                else
                                {
                                    throw new Exception($"装载MultiTagsOnCondition时未找到Tag:{strTag}");
                                }
                            }
                        }
                    }
                    else if (string.Equals(level1Item.Name, "OutParameter", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var strMachineName = level1Item.GetAttribute("MachineName");
                        var strTagName = level1Item.GetAttribute("TagName");

                        OutMachineName = Owner.ProcessParameterManager.GetBasicParam(strMachineName);
                        OutTagName = Owner.ProcessParameterManager.GetBasicParam(strTagName);
                        if (OutMachineName == null || OutTagName == null)
                            throw new Exception($"未能在过程参数里找到机器名{strMachineName}或标签名{strTagName}。");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"装载MultiTagsCondition时出错：{ex.Message}.");
                return false;
            }
        }

        public override bool CheckReady()
        {
            return false;
        }

        public string AlterTrigTagMonitorStatus(string machineName, string tagName, bool shouldMonitor)
        {
            var tagModel = MonitorTags.FirstOrDefault(a => a.TagOwner == machineName && a.Tag.TagName == tagName);

            if (tagModel == null)
                return $"未查询到需要屏蔽监控的TrigTag，MachineName:[{machineName}],TagName:[{tagName}]";

            tagModel.ShouldMonitor = shouldMonitor;

            return $"成功修改Machine[{machineName}]的Tag[{tagName}]的监控状态，监控状态为[{tagModel.ShouldMonitor}]";
        }
    }
}