using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Reflection;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    /// 队列Action基础类
    /// Created By David Dong at 20170407
    /// </summary>
    public abstract class QueueAction : BaseAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(QueueAction));

        protected TrackQueue _ownerQueue;
        public TrackQueue OwnerQueue
        {
            get { return _ownerQueue; }
        }

        public QueueAction(TrackQueue Queue,string name):base(name)
        {
            _ownerQueue = Queue;
            
        }


        #region "Core functions"

        //public abstract void Execute();

        //public abstract bool IsSuccessful();

        //public abstract object GetResult();

        public override bool LoadFromConfig(XmlNode node)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region "Parameters"  
        //注释于Robin20180203
        //public readonly ParameterCollection InParameters = new ParameterCollection();
        //public readonly ParameterCollection OutParameters = new ParameterCollection();

        //protected abstract void CreateParameters();

        //public Parameter GetInParameter(string ParameterName)
        //{
        //    return InParameters[ParameterName];
        //}

        //public Parameter GetOutParameter(string ParameterName)
        //{
        //    return OutParameters[ParameterName];
        //}

        #endregion

    }

    /// <summary>
    /// 进队列操作Action
    /// Created by David Dong 20170408
    /// </summary>
    public class EnqueueAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(EnqueueAction));

        //string _itemID;

        public EnqueueAction(TrackQueue Queue) : base(Queue, "EnqueueAction")
        {
           // _name = "EnqueueAction";
            CreateParameters();
        }


        #region "Core functions"

        public override void Execute()
        {

            _ownerQueue.Entry((InParameters["ItemID"].GetValue().ToString()));
            //_ownerQueue.SaveQueue();
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return null;
        }

        #endregion

        protected override bool CreateParameters()   //void改成bool   20180203Robin
        {
            //  参数1：队列名称
            //Parameter Par1 = new Parameter("QueueName", "String");
            //InParameters.Add(Par1);


            //  参数2：跟踪项目ID号
            Parameter Par2 = new Parameter("ItemID", "string");
            InParameters.Add(Par2);

            return true;
        }

    }

    /// <summary>
    ///  MultiEnqueueAction
    ///  同时10个进队列
    /// </summary>
    public class MultiEnqueueAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(MultiEnqueueAction));

        //string _itemID;

        public MultiEnqueueAction(TrackQueue Queue)
            : base(Queue, "MultiEnqueueAction")
        {
            //_name = "MultiEnqueueAction";
            CreateParameters();
        }

        private List<string> parameters = new List<string>();

        #region "Core functions"
        public object obj = new object();

        public override void Execute()
        {
            lock (obj)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    _ownerQueue.Entry((string)(InParameters[parameters[i]].GetValue()));
                }
                _ownerQueue.SaveQueue();
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return null;
        }

        #endregion

        protected override bool CreateParameters()
        {
            for (int i = 0; i < 10; i++)
            {
                Parameter Par = new Parameter("ItemID"+(i+1).ToString(), "String");
                parameters.Add("ItemID"+(i+1).ToString());
                InParameters.Add(Par);
            }
            return true;

        }

    }

    /// <summary>
    /// 移出队列操作Action
    /// Created by David Dong 20170408
    /// </summary>
    public class DequeueAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(DequeueAction));

        string strItemID;

        public DequeueAction(TrackQueue Queue)
            : base(Queue, "DequeueAction")
        {
           // _name = "DequeueAction";
            CreateParameters();
        }


        #region "Core functions"

        public override void Execute()
        {
            try
            {
                strItemID = _ownerQueue.TakeOut();
                OutParameters["ItemID"].SetValue(Int32.Parse(strItemID));
                _ownerQueue.SaveQueue();
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("执行移出队列{0}操作出错：{1}", _ownerQueue.ResourceName, ex.Message));
            }

        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return strItemID;
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数2：跟踪项目ID号
            Parameter Par2 = new Parameter("ItemID", "Int32");
            OutParameters.Add(Par2);
            return true;
        }

    }


    /// <summary>
    /// 同时10个出队列
    /// </summary>
    public class MultiDequeueAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(MultiDequeueAction));

        //string _itemID;

        public MultiDequeueAction(TrackQueue Queue)
            : base(Queue, "MultiDequeueAction")
        {
           // _name = "MultiDequeueAction";
            CreateParameters();
        }

        private List<string> parameters = new List<string>();

        public object obj = new object();
        #region "Core functions"

        public override void Execute()
        {
            lock (obj)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    string strItemID = _ownerQueue.TakeOut();
                    OutParameters[parameters[i]].SetValue(strItemID);
                }
                _ownerQueue.SaveQueue();
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            //return _itemID;
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            for (int i = 0; i < 10; i++)
            {
                Parameter Par = new Parameter("ItemID"+(i+1).ToString(), "String");
                parameters.Add("ItemID" + (i + 1).ToString());
                OutParameters.Add(Par);
            }
            return true;
        }

    }


    /// <summary>
    /// 查询队列操作Action
    /// Created by David Dong 20170408
    /// </summary>
    public class QueryqueueAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(QueryqueueAction));

        string _itemID;

        public QueryqueueAction(TrackQueue Queue)
            : base(Queue, "QueryqueueAction")
        {
           // _name = "QueryqueueAction";
            CreateParameters();
        }


        #region "Core functions"

        public override void Execute()
        {

            Int16 _index = Convert.ToInt16(InParameters["Index"].GetValue());
            _itemID = _ownerQueue.GetAt(_index);

            OutParameters["ItemID"].SetValue(_itemID);

        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return _itemID;
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数1：查询索引
            Parameter Par1 = new Parameter("Index", "Int16");
            InParameters.Add(Par1);

            //  参数2：查询结果
            Parameter Par2 = new Parameter("ItemID", "String");
            OutParameters.Add(Par2);
            return true;
        }
    }

    /// <summary>
    /// 拿出队列中某个位置item
    /// 2017年7月3日14:20:28  夏 
    /// </summary>
    public class QueryQueueOutAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(QueryQueueOutAction));

        string _itemID;

        public QueryQueueOutAction(TrackQueue Queue)
            : base(Queue, "QueryQueueOutAction")
        {
           // _name = "QueryQueueOutAction";
            CreateParameters();
        }


        #region "Core functions"

        public override void Execute()
        {
            object str = InParameters["Index"].GetValue();

            Int32 _index = Convert.ToInt32((InParameters["Index"].GetValue()));
            if (_index > _ownerQueue.Counts)
            {
                _index = 0;
            }
            _itemID = _ownerQueue.TakeOutAt(_index);

            OutParameters["ItemID"].SetValue(_itemID);

        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return _itemID;
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数1：查询索引
            Parameter Par1 = new Parameter("Index", "Int32");
            InParameters.Add(Par1);

            //  参数2：查询结果
            Parameter Par2 = new Parameter("ItemID", "String");
            OutParameters.Add(Par2);
            return true;
        }

    }
    /// <summary>
    /// 查询队列操作Action
    /// Created by xia 2017-4-11 19:51:30
    /// </summary>
    public class QueryHeadAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(QueryHeadAction));

        string _itemID;

        public QueryHeadAction(TrackQueue Queue)
            : base(Queue, "QueryHeadAction")
        {
           // _name = "QueryHeadAction";
            CreateParameters();
        }


        private bool _actionResult;

        #region "Core functions"

        public override void Execute()
        {
            if (_ownerQueue.Counts>0)
            {
                Int16 _top = (Int16)(_ownerQueue.Counts - 1);
                _itemID = _ownerQueue.GetAt(_top);
                OutParameters["ItemID"].SetValue(_itemID);
                _actionResult = true;
            } 
            else 
            {
                OutParameters["ItemID"].SetValue("默认");
                _actionResult = true;
                //_actionResult = false;
            }

        }

        public override bool IsSuccessful()
        {

            return _actionResult;

        }

        public override object GetResult()
        {
            return _itemID;
        }

        #endregion

        protected override bool CreateParameters()
        {
            ////  参数1：查询索引
            //Parameter Par1 = new Parameter("Index", "Int16");
            //InParameters.Add(Par1);

            //  参数2：查询结果
            Parameter Par2 = new Parameter("ItemID", "String");
            OutParameters.Add(Par2);
            return true;
        }
    }

    public class QuerySecondAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(QuerySecondAction));

        string _itemID;

        public QuerySecondAction(TrackQueue Queue)
            : base(Queue,"QuerySecondAction")
        {
          //  _name = "QuerySecondAction";
            CreateParameters();
        }


        #region "Core functions"

        public override void Execute()
        {

            //Int16 _index = (Int16)InParameters["Index"].GetValue();
            if (_ownerQueue.Counts == 2)
            {
                
                _itemID = _ownerQueue.GetAt(0);
            }
            else if (_ownerQueue.Counts == 1)
            {
                _itemID = _ownerQueue.GetAt(0);
            }
            else
            {
                _itemID = _ownerQueue.GetAt(1);
            }
    
            OutParameters["ItemID"].SetValue(_itemID);

        }


        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return _itemID;
        }

        #endregion

        protected override bool CreateParameters()
        {
            ////  参数1：查询索引
            //Parameter Par1 = new Parameter("Index", "Int16");
            //InParameters.Add(Par1);

            //  参数2：查询结果
            Parameter Par2 = new Parameter("ItemID", "String");
            OutParameters.Add(Par2);
            return true;
        }

    }

    /// <summary>
    /// 检查队列头操作Action
    /// Created by Robin 20170411
    /// </summary>
    public class CheckHeadAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(QueryHeadAction));

        string _itemID;
        public CheckHeadAction(TrackQueue Queue)
            : base(Queue,"CheckHeadAction")
        {
           // _name = "CheckHeadAction";
            CreateParameters();
        }


        #region "Core functions"

        public override void Execute()
        {
            int _top = _ownerQueue.Counts;
            if (_top > 0)
            {
                _itemID = _ownerQueue.GetAt((Int16)(_top - 1));
                if ((Int32)InParameters["ItemID"].GetValue() == Int32.Parse(_itemID))
                {
                    OutParameters["Result"].SetValue(true);
                }
                else
                {
                    OutParameters["Result"].SetValue(false);
                }
            }
            else
            {
                OutParameters["Result"].SetValue(false);
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }
        public override object GetResult()
        {
            return null;
        }
        #endregion

        protected override bool CreateParameters()
        {
            Parameter Par1 = new Parameter("ItemID", "Int32");
            InParameters.Add(Par1);

            Parameter Par2 = new Parameter("Result", "bool");
            OutParameters.Add(Par2);
            return true;
        }

    }


    /// <summary>
    /// 查询队列长度Action
    /// Created by ZSY 2017年6月2日23:07:43
    /// </summary>
    public class GetQueueLengthAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(GetQueueLengthAction));


        public GetQueueLengthAction(TrackQueue Queue)
            : base(Queue,"GetQueueLengthAction")
        {
           //_name = "GetQueueLengthAction";
            CreateParameters();
        }


        #region "Core functions"

        public override void Execute()
        {
            try
            {
                string queueName=InParameters["QueueName"].GetValueInString();
                TrackQueue queue = (TrackQueue)ResourceManager.GetResource(queueName);
                int queueLength = queue.Counts;
                OutParameters["QueueLength"].SetValue(queueLength);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return null;
        }

        #endregion

        protected override bool CreateParameters()
        {
            Parameter Par1 = new Parameter("QueueName","String");
            InParameters.Add(Par1);
            Parameter Par2 = new Parameter("QueueLength", "int32");
            OutParameters.Add(Par2);

            return true;
        }

    }
    /// <summary>
    /// 清空队列
    /// Created by 夏 2017年7月4日10:04:32
    /// </summary>
    public class ClearQueueAction : QueueAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ClearQueueAction));


        public ClearQueueAction(TrackQueue Queue)
            : base(Queue,"ClearQueueAction")
        {
          //  _name = "ClearQueueAction";
            
        }


        #region "Core functions"

        public override void Execute()
        {
            try
            {
                _ownerQueue.Clear();
            }
            catch (Exception es)
            {
                LOG.Error(string.Format("清空队列{0}出错",_ownerQueue.ResourceName,es.Message));
            }

        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            return null;
        }

        #endregion

        protected override bool CreateParameters()
        {
            return true;
        }

    }
}
