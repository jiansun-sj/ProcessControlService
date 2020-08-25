// ==================================================
// 文件名：MachineAction.cs
// 创建时间：2020/01/02 15:09
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/01/19 15:09
// 修改人：jians
// ==================================================

using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Machines.Actions
{
    public abstract class MachineAction : BaseAction
    {
        protected MachineAction(string name) : base(name)
        {
        }

        public Machine OwnerMachine
        {
            get => (Machine) ActionContainer;
            set => ActionContainer = value;
        }
    }
}