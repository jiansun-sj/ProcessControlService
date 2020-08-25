// ==================================================
// 文件名：UnitContainer.cs
// 创建时间：2020/01/02 13:56
// ==================================================
// 最后修改于：2020/05/21 13:56
// 修改人：jians
// ==================================================

using System.Xml;
using log4net;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    ///     容器对象（例：托盘）
    ///     Created By David Dong at 20180530
    /// </summary>
    public abstract class UnitContainer : ITrackUnit
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UnitContainer));

        public string Type => nameof(UnitContainer);
        
        public string Name { get; }
        
        public bool LoadFromConfig(XmlNode node)
        {
            throw new System.NotImplementedException();
        }

        public abstract short UnitCount { get; }
        
        protected abstract short ContainerSize { get; }

        public bool IsFull => UnitCount >= ContainerSize; //是否已满
        
        public bool IsEmpty => UnitCount == 0; //是否为空

        public abstract bool AcceptUnit(ITrackUnit unit);

        public abstract bool HasUnit(ITrackUnit unit);

        public abstract bool PutIn(ITrackUnit unit);

        public abstract ITrackUnit GetCandidateUnit();

        public abstract ITrackUnit TakeOut();

        public abstract void TakeOut(ITrackUnit unit);
        
        public object Clone()
        {
            throw new System.NotImplementedException();
        }

        public ILocation PreviousLocation { get; set; }
        
        public ILocation CurrentLocation { get; set; }
        public string Id { get; set; }
        public string ToJson()
        {
            throw new System.NotImplementedException();
        }
    }
}