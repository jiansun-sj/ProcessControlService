// ==================================================
// 文件名：MultiTagsCondition.TagModel.cs
// 创建时间：2020/07/21 11:33
// ==================================================
// 最后修改于：2020/07/21 11:33
// 修改人：jians
// ==================================================

using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    public partial class MultiTagsCondition
    {
        public class TagModel
        {
            public Tag Tag { get; set; }

            //是否监控该Tag的变化
            public bool ShouldMonitor { get; set; } = true;

            public string TagOwner => Tag.Owner.ResourceName;
        }
    }
}