// ==================================================
// 文件名：ActionContainer.cs
// 创建时间：2020/01/02 10:35
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/03/09 10:35
// 修改人：jians
// ==================================================

namespace ProcessControlService.ResourceLibrary.Action
{
    // Action 的容器接口
    public interface IActionContainer
    {
        void AddAction(BaseAction action);

        BaseAction GetAction(string actionName);

        void ExecuteAction(string name);

        string[] ListActionNames();
    }
}