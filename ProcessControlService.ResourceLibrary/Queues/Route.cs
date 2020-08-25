// ==================================================
// 文件名：Route.cs
// 创建时间：2020/08/18 14:14
// ==================================================
// 最后修改于：2020/08/18 14:14
// 修改人：jians
// ==================================================

namespace ProcessControlService.ResourceLibrary.Queues
{
    /// <summary>
    ///     路由
    /// </summary>
    public class Route
    {
        /// <summary>
        ///     路由Id
        /// </summary>
        public short Id { get; set; }

        /// <summary>
        ///     路由对应队列名
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        ///     是否为工艺终点，车辆到达路由终点时，清除所有队列中相应车辆的信息。
        /// </summary>
        public bool IsTerminal { get; set; }
    }
}