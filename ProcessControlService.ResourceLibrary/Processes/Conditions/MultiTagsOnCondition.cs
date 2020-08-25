// ==================================================
// 文件名：MultiTagsOnCondition.cs
// 创建时间：2020/06/16 13:45
// ==================================================
// 最后修改于：2020/08/10 13:45
// 修改人：jians
// ==================================================

using System;
using log4net;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    /// <summary>
    ///     多个Tag变为ON触发条件
    /// </summary>
    public class MultiTagsOnCondition : MultiTagsCondition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MultiTagsOnCondition));

        public MultiTagsOnCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        public override bool CheckReady()
        {
            return OneTagValueChangedToOn();
        }

        private bool OneTagValueChangedToOn()
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

                    if ((bool) monitorTag.Tag.TagValue)
                    {
                        Log.Info(
                            $"{lastTag.Owner.ResourceName}的触发信号发生变化，变化前值为：{lastTag.TagValue},变化后值为：{monitorTag.Tag.TagValue}.");

                        //查找到变化
                        lastTag.TagValue = monitorTag.Tag.TagValue;

                        // 设置输出参数
                        OutMachineName.SetValue(monitorTag.TagOwner);
                        OutTagName.SetValue(monitorTag.Tag.TagName);

                        return true;
                    }

                    Log.Info(
                        $"{lastTag.Owner.ResourceName}的触发信号发生变化，变化前值为：{lastTag.TagValue},变化后值为：{monitorTag.Tag.TagValue}.");

                    lastTag.TagValue = monitorTag.Tag.TagValue;
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                return false;
            }
        }
    }
}