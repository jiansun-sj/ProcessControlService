// ==================================================
// 文件名：MultiTagsChangeCondition.cs
// 创建时间：2020/06/04 10:51
// ==================================================
// 最后修改于：2020/06/04 10:51
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using log4net;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    public class MultiTagsChangeCondition : MultiTagsCondition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MultiTagsChangeCondition));

        public MultiTagsChangeCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        public MultiTagsChangeCondition()
        {
        }

        public override bool CheckReady()
        {
            return OnTagValueChanged();
        }

        private bool OnTagValueChanged()
        {
            try
            {
                foreach (var monitorTagPair in MonitorTagChangeDic)
                {
                    if (!TagValueIsNotNullAndHasChanged(monitorTagPair, out var monitorTag, out var currentTagValue)) continue;

                    // 设置输出参数
                    OutMachineName.SetValue(monitorTag.TagOwner);
                    OutTagName.SetValue(monitorTag.Tag.TagName);

                    return currentTagValue > 0;
                }

                return false;
            }
            catch (Exception)
            {
                Log.Error("");
                return false;
            }
        }

        /// <summary>
        /// UnitTest Purpose
        /// </summary>
        /// <param name="monitorTagPair"></param>
        /// <param name="monitorTag"></param>
        /// <param name="currentTagValue"></param>
        /// <returns></returns>
        public bool TagValueIsNotNullAndHasChanged(KeyValuePair<TagModel, Tag> monitorTagPair, out TagModel monitorTag, out int currentTagValue)
        {
            monitorTag = monitorTagPair.Key;

            currentTagValue=0;
            
            if (!monitorTag.ShouldMonitor)
                return false;

            var lastTag = monitorTagPair.Value;

            if (monitorTag.Tag.TagValue == null || lastTag.TagValue == null)
                return false;

            currentTagValue = Convert.ToInt32(monitorTag.Tag.TagValue);
            var lastTagValue = Convert.ToInt32(lastTag.TagValue);
            
            if (currentTagValue == lastTagValue) return false;

            lastTag.TagValue = currentTagValue;
            
            //忽略程序启动时
            return lastTagValue != 0;
        }
    }
}