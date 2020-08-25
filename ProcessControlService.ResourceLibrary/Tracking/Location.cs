namespace ProcessControlService.ResourceLibrary.Tracking
{
    public interface ILocation
    {
        #region ILocation

        string LocationId { get; set; }

        bool AcceptTrackUnit(ITrackUnit unit); //该位置接受Unit

        bool CouldCreatePlace(ITrackUnit unit); //是否可以创建一个容纳Unit的位置

        bool HasOutput { get; set; } //是否有输出

        int UnitCount { get; }

        ITrackUnit QueryUnit(string trackUnitId); //根据关键字查询

        void RemoveUnit(ITrackUnit unit); //删除

        ITrackUnit TakeUnit(); //拿走
        
        bool PutUnit(ITrackUnit unit); //放进

        object LocationLocker { get; set; }//是否已经有处理进程

        #endregion
    }
}
