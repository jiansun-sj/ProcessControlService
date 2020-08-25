// ==================================================
// 文件名：MultiTagsOffCondition.cs
// 创建时间：2020/06/16 13:44
// ==================================================
// 最后修改于：2020/08/10 13:44
// 修改人：jians
// ==================================================

using System;
using log4net;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    public class MultiTagsOffCondition : MultiTagsCondition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MultiTagsOffCondition));

        public MultiTagsOffCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        // check if Tag value change
        public override bool CheckReady()
        {
            return OneTagValueChangedToOff();
        }

        private bool OneTagValueChangedToOff()
        {
            try
            {
                for (var i = 0; i < LastTags.Count; i++)
                {
                    var monitorTag = MonitorTags[i];

                    if (!monitorTag.ShouldMonitor)
                        continue;

                    var lastTag = LastTags[i];

                    if (Tag.ValueEqual(monitorTag.Tag, lastTag)) continue;
                    if (!(bool) monitorTag.Tag.TagValue)
                    {
                        //查找到变化
                        lastTag.TagValue = monitorTag.Tag.TagValue;

                        // 设置输出参数
                        OutMachineName.SetValue(monitorTag.TagOwner);
                        OutTagName.SetValue(monitorTag.Tag.TagName);

                        return true;
                    }

                    lastTag.TagValue = monitorTag.Tag.TagValue;
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error($"检测下降沿触发条件失败，异常为:[{e.Message}]");
                return false;
            }
        }
    }
}