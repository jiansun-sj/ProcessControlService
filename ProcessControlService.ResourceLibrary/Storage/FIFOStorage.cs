using System;
using System.Collections.Generic;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.ResourceLibrary.Storage
{
    /// <summary>
    /// 简单存储基本类
    /// Created by Dongmin 20180604
    /// </summary>
    public class FifoStorage : Storage
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(FifoStorage));

        public FifoStorage(string name) : base(name)
        {
          
        }

        private readonly Queue<TrackingUnit2> _queue = new Queue<TrackingUnit2>();

        #region Storage

        public override int Count => _queue.Count;

        #endregion

        #region "Resource definition"

        public override string ResourceName => StorageName;

        public override string ResourceType => "FIFOStorage";

        public override IResource GetResourceObject()
        {
            return this;
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Comment) return true;

                var level1Item = (XmlElement)node;

            }
            catch (Exception ex)
            {
                Log.Error($"加载FIFOStorage{ResourceName}出错：{ex.Message}");
                return false;
            }

            return true;
        }

        #endregion

        protected override void Entry(TrackingUnit2 item)
        {
            if(Count< Size)
            {
                _queue.Enqueue(item);
            }
        }

        protected override TrackingUnit2 Exit()
        {
            return Count>0 ? _queue.Dequeue() : null;
        }
    }
}
