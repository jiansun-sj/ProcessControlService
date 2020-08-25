using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.ResourceLibrary.Storage
{
    /// <summary>
    /// 队列，一般先进先出，也允许插入或从中间移出。
    /// </summary>
    public class FreeQueue : Storage
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(FreeQueue));

        public FreeQueue(string name) : base(name)
        {

        }

        private readonly List<TrackingUnit2> _queue = new List<TrackingUnit2>();

        #region Storage

        public override int Count => _queue.Count;

        public override void PutIn(StoragePosition pos, TrackingUnit2 item)
        {
            if (pos.DimensionCount == 1)
            {
                int posX = pos.GetDiemensionValue(StoragePositionDimension.X);

                Entry(posX, item);
            }
            else
            {
                throw new Exception(StorageName + "位置坐标维度错误");
            }
        }

        public void Entry(int pos, TrackingUnit2 item)
        {
            if (Count < Size)
            {
                _queue.Insert(pos, item);

            }
            else
            {
                throw new Exception(StorageName + "进队列出错,数量已满");
            }
        }

        //进出队列说明
        public override TrackingUnit2 TakeOut(StoragePosition Pos)
        {
            if (Pos.DimensionCount == 1)
            {
                int PosX = Pos.GetDiemensionValue(StoragePositionDimension.X);

                return Exit(PosX);
            }

            throw new Exception(StorageName + "位置坐标维度错误");
        }


        //进出队列说明
        //Add(Item)是在结尾处增加新元素
        //RemoveAt(Count-1)是删除结尾处的元素
        //Insert(Couunt,Item)是在结尾处增加新元素
        public TrackingUnit2 Exit(int index)
        {
            if (Count > 0)
            {
                var temp = _queue[index];
                _queue.RemoveAt(index);

                return temp;
            }

            throw new Exception(StorageName + "出队列出错,数量为0");
        }

        public TrackingUnit2 GetItem(int index)
        {
            try
            {
               return _queue[index];
            }
            catch (Exception ex)
            {
                Log.Error($"从FreeQueue：{ResourceName}获取数据失败{ex}");
                return null;
            }
        }


        protected override string GetStatus()
        {
            var statusModel = _queue.Select(item => item.Id).ToList();
            var json = new DataContractJsonSerializer(statusModel.GetType());
            var szJson = "";
            using (var stream = new MemoryStream())
            {
                json.WriteObject(stream, statusModel);
                szJson = Encoding.UTF8.GetString(stream.ToArray());
            }
            return szJson;
        }

        #endregion

        #region "Resource definition"

        public override string ResourceName => StorageName;

        public override string ResourceType => "Queue";

        public override IResource GetResourceObject()
        {
            return this;
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level1Item = (XmlElement)node;
                //if (level1_item.HasAttribute("Size"))
                //{
                //    string strSize = level1_item.GetAttribute("Size");
                //    _size = Convert.ToInt16(strSize);
                //}
                //else
                //{
                //    _size = 0;
                //    throw new Message("XML没有Size属性");
                //}
             
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"加载Queue:{ResourceName}出错：{ex.Message}");
                return false;
            }
        }

        #endregion
  
    }
}
