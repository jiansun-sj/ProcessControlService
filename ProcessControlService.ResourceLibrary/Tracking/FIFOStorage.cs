using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Machines.DataSources;
using ProcessControlService.ResourceLibrary.Action;
using log4net;
using Newtonsoft.Json;
using System.IO;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    /// 简单存储基本类
    /// Created by Dongmin 20180604
    /// </summary>
    public class FIFOStorage : Storage
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(FIFOStorage));

        public FIFOStorage(string Name) : base(Name)
        {
          
        }

        private Queue<TrackingUnit2> _queue = new Queue<TrackingUnit2>();

        #region Storage

        public override Int32 Count => (Int32)_queue.Count;

        #endregion

        #region "Resource definition"

        public override string ResourceName
        {
            get
            {
                return _storageName;
            }
        }

        public override string ResourceType
        {
            get
            {
                return "FIFOStorage";
            }
        }

        public override IResource GetResourceObject()
        {
            return this;
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Comment) return true;

                XmlElement level1_item = (XmlElement)node;

                string strSize = level1_item.GetAttribute("Size");

                _size = Convert.ToInt16(strSize);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("加载FIFOStorage{0}出错：{1}", ResourceName, ex.Message));
                return false;
            }

            return true;
        }

        #endregion

        override public void Entry(TrackingUnit2 Item)
        {
            if(Count<_size)
            {
                _queue.Enqueue(Item);
            }
        }

        public override TrackingUnit2 Exit()
        {
            if(Count>0)
            {
                return _queue.Dequeue();
            }
            return null;
        }
    }
}
