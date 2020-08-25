// ==================================================
// 文件名：LineQueue.cs
// 创建时间：2020/08/18 16:01
// ==================================================
// 最后修改于：2020/08/18 16:01
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.ResourceLibrary.Queues
{
    /// <summary>
    ///     线体队列，先进先出原则
    /// </summary>
    public class FIFOQueue<T> : ILocation where T : ITrackUnit
    {
        public int QueueSize = 500;

        private readonly ILog _log = LogManager.GetLogger(typeof(FIFOQueue<>));

        public readonly List<T> TrackUnits = new List<T>();

        public FIFOQueue(string queueName)
        {
            if (string.IsNullOrEmpty(queueName)) throw new ArgumentNullException(nameof(FIFOQueue<T>));

            QueueName = queueName;
        }

        public string QueueName { get; }

        public string LocationId { get=>QueueName; set{} }

        public bool AcceptTrackUnit(ITrackUnit unit)
        {
            return unit is T;
        }

        public bool CouldCreatePlace(ITrackUnit unit)
        {
            return false;
        }

        public bool HasOutput { get; set; } = false;

        public int UnitCount => TrackUnits.Count;

        public ITrackUnit QueryUnit(string trackUnitId)
        {
            return TrackUnits.FirstOrDefault(a => a.Id == trackUnitId);
        }

        public void RemoveUnit(ITrackUnit unit)
        {
            if (unit is T trackUnit && TrackUnits.Contains(trackUnit)) TrackUnits.Remove(trackUnit);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public ITrackUnit TakeUnit()
        {
            try
            {
                lock (LocationLocker)
                {
                    var firstOrDefault = TrackUnits.FirstOrDefault();

                    if (firstOrDefault != null) TrackUnits.Remove(firstOrDefault);

                    /*
                   AVICenter.FreeSql.Delete<Vehicle>().Where(a => a.QueueName == QueueName && a.Vin == vin)
                       .ExecuteAffrows();
                       */

                    return firstOrDefault;
                }
            }
            catch (Exception e)
            {
                _log.Error($"车体移出队列失败，异常为：{e}");
                return null;
            }
        }

        public bool PutUnit(ITrackUnit unit, bool addIntoDatabase)
        {
            try
            {
                PutUnit(unit);
                
                // if (addIntoDatabase) AVICenter.FreeSql.Insert(vehicle).ExecuteAffrows();

                return true;
            }
            catch (Exception e)
            {
                _log.Error($"车体队列进队列失败，异常为：{e}");
                return false;
            }
        }
        
        public bool PutUnit(ITrackUnit unit)
        {
            try
            {
                lock (LocationLocker)
                {
                    if (TrackUnits.Any(a => a.Id == unit.Id)) return false;

                    if (TrackUnits.Count >= QueueSize)
                        throw new ArgumentOutOfRangeException($"超出车体队列上限值，车体队列名：[{QueueName}]");

                    TrackUnits.Add((T) unit);

                    unit.CurrentLocation = this;

                    return true;
                }
            }
            catch (Exception e)
            {
                _log.Error($"车体队列进队列失败，异常为：{e}");
                return false;
            }
        }

        public object LocationLocker { get; set; } = new object();

        public bool HasQueueMember(string id)
        {
            return TrackUnits.Any(a => a.Id == id);
        }

        public List<T> GetTrackUnits()
        {
            return TrackUnits;
        }

        public void Clear()
        {
            TrackUnits.Clear();
        }
    }
}