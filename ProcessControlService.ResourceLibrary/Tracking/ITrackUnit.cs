// ==================================================
// 文件名：TrackingUnit2.cs
// 创建时间：2020/01/02 13:56
// ==================================================
// 最后修改于：2020/05/21 13:56
// 修改人：jians
// ==================================================

using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    ///     用于追踪的基本单元，可用于存储或队列
    ///     TrackUnit抽象为接口，并继承自ICustomizedType
    /// </summary>
    public interface ITrackUnit : ICustomType
    {
        ILocation CurrentLocation { get; set; } //可跟踪对象的位置

        string Id { get; set; }
    }
}