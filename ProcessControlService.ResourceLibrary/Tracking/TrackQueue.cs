using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    /// 修改：夏   2017-3-21 11:15:31
    /// 修改内容：增加batIndex指针
    /// </summary>
    public class TrackQueue : ParameterCollection, IResource
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(TrackQueue));

        private short _length = 0;
        //private List<TrackItem> _items = new List<TrackItem>();//将_items里的东西的实例的数据取出来，用流的方式序列化，存成文件放到内存外；每次关机重开机的时候再读取内存外的文件，然后反序列化
        private List<string> _itemIDs = new List<string>(); //存放ItemID列表
        public int Counts
        {
            get { return _itemIDs.Count; }
        }

        //private int* takeIndex; 

        /// <summary>
        /// 指针，定位下次要判读电池的队列下标
        /// </summary>
        
        //public int* batIndex;

        private string _queueName;

        public string ResourceType
        {
            get
            {
                return "Queue";
            }
        }

        public TrackQueue(string name)
        {
            this._queueName = name;

            CreateActions();
        }

        #region "Resource definition"
        public string ClassName
        {
            get { return "Queue"; }
        }

        public string ResourceName
        {
            get { return _queueName; }
        }
       
        public IResource GetResourceObject()
        {
            return this;
        }
        
        /// <summary>
        /// Created by Dongmin at 20170311
        /// 装载队列XML
        /// Example：<Resource Type="Queue" Name="BatteryQueue" Item="Battery" Length="10" Enable="True"/>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool LoadFromConfig(XmlNode node)
        {
            try
            {
                XmlElement level0_item = (XmlElement)node;
               
                //string strItem = level0_item.GetAttribute("Item");
                string strLength = level0_item.GetAttribute("Length");

                _length = Convert.ToInt16(strLength);

                try
                {
                    LoadQueue();
                }
                catch { }
                return true;
            }
            catch (Exception ex)
            {
                LOG.Error("信息出错："+ex);
                return false;
            }
        }

        // add by David 20170917
        public void FreeResource()
        {
            // nothing to do now
        }

        public IResourceExportService GetExportService()
        {
            return null;
        }

        #endregion

        public string[] ListItemID()
        {
            List<string> itemIDs = new List<string>();

            foreach (string itemID in _itemIDs)
            {
                itemIDs.Add(itemID);
            }

            return itemIDs.ToArray();
        }

        ///// <summary>
        ///// 进入队列
        ///// </summary>
        ///// <param name="item"></param>
        //public void Entry(TrackItem item)
        //{
        //    _itemIDs.Insert(0, item.ID);
        //}

        /// <summary>
        /// 进入队列  
        /// xia  如果队列已满，着先移除一个元素在入队 2017年7月4日10:00:13 
        /// </summary>
        /// <param name="item"></param>
        public void Entry(string ItemID)
        {
            if (_itemIDs.Contains(ItemID))
            {
                LOG.DebugFormat("队列{0}中已存在此元素：{1}，不能入队", ResourceName, ItemID);
                return;
            }
            while (Counts >=_length)
            {
                _itemIDs.RemoveAt(Counts-1);
            }
            _itemIDs.Insert(0, ItemID);
            SaveQueue();
        }

        /// <summary>
        /// 移出队列
        /// </summary>
        /// <returns></returns>
        //public TrackItem TakeOut()
        //{
        //    string strItemID = TakeOut();

        //    TrackItem Item = new TrackItem(strItemID);
        //}

        public string TakeOut()
        {
            try
            {
                //*batIndex = *batIndex - 1;
                int i = Counts;
                string temp = _itemIDs[i - 1];
                if (i>=1)
                {
                    _itemIDs.RemoveAt(i - 1);
                } 
                

                return temp;
            }
            catch (Exception ex)
            {
                LOG.Error("取出队列项目异常"+ex.Message );
                return "";
            }
        }

        //public void Entry(string TrackItemID)
        //{ }

        /// <summary>
        /// 从队列中部拿走
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
         public string TakeOutAt(Int32 index)
        {
            try
            {
                //*batIndex = *batIndex-1;
                string temp = _itemIDs[index];
                _itemIDs.RemoveAt(index);
                SaveQueue();
                return temp;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("取出队列项目异常", ex.Message));
            }
        }

        /// <summary>
        /// 查询队列中某元素个数
        /// </summary>
        /// <returns></returns>
        public int QueryCountByItemID(string ItemID)
        {
            try
            {
                int c = _itemIDs.Count(p => p == ItemID);
                return c;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("查询队列中某元素个数", ex.Message));
            }
        }


        /// <summary>
        /// 从队列中部插入
        /// </summary>Robin  20170619
        /// <param name="index"></param>
        /// <returns></returns>
         public bool InsertByIndex(Int16 index,string ItemID)
        {
            try
            {
                _itemIDs.Insert(index, ItemID);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("插入队列项目异常{0}:",ex.Message));
            }
        }

        /// <summary>
        /// 获取队列中指针所指的项目
        /// </summary>
        /// 
        /// <returns></returns>
        public string GetAt(Int16 Index)
        {
           
            try
            {
                 //判断是否满足
                if (Counts > Index)
                {
                    if( _itemIDs[Index]=="")
                        return _itemIDs[0];
                    else
                        return _itemIDs[Index];
                }
                else
                    return _itemIDs[0];
            }
            catch (Exception)
            {
                
                LOG.Error(string.Format("获取队列{0}值出错,Index={1}", _queueName, Index));
                return "";
            }
            
        }

        /// <summary>
        /// 创建：夏 2017年7月4日10:08:13
        /// 修改：
        /// 作用：清空队列
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            try
            {
                _itemIDs.Clear();
            }
            catch (Exception)
            {
                LOG.Error(string.Format("清空队列{0}值出错", _queueName));
            }
            
        }

        #region "Actions"

        private void CreateActions()
        {
            QueueAction action1 = new EnqueueAction(this);
            _actions.AddAction(action1);

            QueueAction action2 = new DequeueAction(this);
            _actions.AddAction(action2);

            QueueAction action3 = new QueryqueueAction(this);
            _actions.AddAction(action3);

            QueueAction action4 = new QueryHeadAction(this);
            _actions.AddAction(action4);

            QueueAction action5 = new CheckHeadAction(this);
            _actions.AddAction(action5);

            QueueAction action6 = new QuerySecondAction(this);
            _actions.AddAction(action6);

            QueueAction action7 = new GetQueueLengthAction(this);//Add by   ZSY 2017年6月3日
            _actions.AddAction(action7);

            //同时多个进队列
            QueueAction action8 = new MultiEnqueueAction(this);//Add by   ZSY 2017年6月10日
            _actions.AddAction(action8);

            //同时多个出队列
            QueueAction action9 = new MultiDequeueAction(this);//Add by   ZSY 2017年6月10日
            _actions.AddAction(action9);
            QueueAction action10 = new QueryQueueOutAction(this);//夏 2017年7月3日14:25:03 拿出队列中某个位置item，二楼入盘机使用
            _actions.AddAction(action10);

            QueueAction action11 = new ClearQueueAction(this);//夏2017年7月4日10:15:17 清空某个队列
            _actions.AddAction(action11);

        }

        private ActionCollection _actions = new ActionCollection();

        //public abstract void CreateActions();

        public QueueAction GetAction(string name)
        {
            return (QueueAction)_actions.GetAction(name);
        }

        public virtual void ExecuteAction(string name)
        {
            QueueAction _action = GetAction(name);
            _action.Execute();
        }

        public string[] ListActionNames()
        {
            return _actions.ListActionNames();
        }
        #endregion


        public void SaveQueue()
        {
            lock (this)
            {
                FileStream fs = new FileStream(@".\On-Site Data\" + this.ResourceName + ".queue", FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, this._itemIDs);
                fs.Close();
            }
        }

        public void LoadQueue()
        {
            lock (this)
            {
                FileStream readstream = new FileStream(@".\On-Site Data\" + this.ResourceName + ".queue", FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryFormatter formatter = new BinaryFormatter();
                this._itemIDs = (List<string>)formatter.Deserialize(readstream);
                readstream.Close();
            }
        }

        public void StartWork()
        {
            throw new NotImplementedException();
        }
    }

}
