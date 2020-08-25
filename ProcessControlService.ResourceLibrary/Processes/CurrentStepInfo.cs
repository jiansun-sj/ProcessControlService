// ==================================================
// 文件名：CurrentStepInfo.cs
// 创建时间：2020/06/11 13:53
// ==================================================
// 最后修改于：2020/06/11 13:53
// 修改人：jians
// ==================================================

namespace ProcessControlService.ResourceLibrary.Processes
{
    public class CurrentStepInfo
    {
        public short Id { get; set; }

        public string Name { get; set; }

        public string Container { get; set; }
    }
}