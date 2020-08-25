// ==================================================
// 文件名：SimpleUnitContainer.cs
// 创建时间：2020/01/02 13:55
// ==================================================
// 最后修改于：2020/05/21 13:55
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    ///     先进先出容器对象
    ///     Created By David Dong at 20180530
    /// </summary>
    public class SimpleUnitContainer : UnitContainer
    {
        private readonly Queue<ITrackUnit> _units = new Queue<ITrackUnit>(); // 产品集合对象

        public override short UnitCount => (short) _units.Count;

        protected override short ContainerSize => short.MaxValue;

        public override bool PutIn(ITrackUnit Unit)
        {
            //if (UnitCount < ContainerSize)
            if (!IsFull)
            {
                _units.Enqueue(Unit);
                return true;
            }

            return false;
        }

        public override ITrackUnit TakeOut()
        {
            if (!IsEmpty) return _units.Dequeue();
            return null;
        }

        public override bool AcceptUnit(ITrackUnit Unit)
        {
            throw new NotImplementedException();
        }

        public override bool HasUnit(ITrackUnit Unit)
        {
            throw new NotImplementedException();
        }

        public override void TakeOut(ITrackUnit Unit)
        {
            throw new NotImplementedException();
        }

        public override ITrackUnit GetCandidateUnit()
        {
            throw new NotImplementedException();
        }
    }
}