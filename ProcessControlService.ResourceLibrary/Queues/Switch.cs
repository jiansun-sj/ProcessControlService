// ==================================================
// 文件名：Switch.cs
// 创建时间：2020/08/18 14:13
// ==================================================
// 最后修改于：2020/08/18 14:13
// 修改人：jians
// ==================================================

using System.Collections.Generic;

namespace ProcessControlService.ResourceLibrary.Queues
{
    /// <summary>
    ///     队列分叉站点路由
    /// </summary>
    public class Switch
    {
        public Dictionary<short, Route> Routes = new Dictionary<short, Route>();

        public Switch(string switchName)
        {
            Name = switchName;
        }

        public string Name { get; set;}

        public Route this[short index] => Routes.ContainsKey(index) ? Routes[index] : null;

        public void AddRoute(short id, Route route)
        {
            Routes.Add(id, route);
        }
    }
}