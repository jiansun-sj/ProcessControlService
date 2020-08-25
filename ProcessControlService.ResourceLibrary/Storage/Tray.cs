// ==================================================
// 文件名：Tray.cs
// 创建时间：2020/05/22 9:01
// ==================================================
// 最后修改于：2020/05/22 9:01
// 修改人：jians
// ==================================================

using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.ResourceLibrary.Storage
{
    /// <summary>
    /// 托盘，单个或一组产品放置在托盘上。
    /// </summary>
    public class Tray: TrackingUnit2
    {
        /// <summary>
        /// 托盘上的产品集合
        /// </summary>
        public ProductCollection ProductCollection { get; set; }=new ProductCollection();

        /// <summary>
        /// 托盘产品容量，默认为单个产品
        /// </summary>
        public int Capacity { get; set; } = 1;
        
    }
}