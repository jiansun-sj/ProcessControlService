using System;
using System.ServiceModel;
using ProcessControlService.Contracts;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
           ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class QueueService : IQueue
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MachineService));

      //  private ProcessFactory pc_controller;

        public QueueService()
        {
            //pc_controller = pc;

            AddClientEventHandler(ClientEventType.HeartBeat, HeartBeat);
            AddClientEventHandler(ClientEventType.MissHeartBeat, HeartbeatTimeout);

        }

        #region "心跳"
        private HeartBeatManager _hbManager = new HeartBeatManager(); //心跳管理对象

        /// <summary>
        /// 定阅事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="handler">事件处理方法</param>
        public void AddClientEventHandler(ClientEventType type, EventHandler<ClientEventArg> handler)
        {
            switch (type)
            {

                case ClientEventType.MissHeartBeat:
                    {
                        _hbManager.AddMissingHBHandler(handler);
                        break;
                    }
                case ClientEventType.HeartBeat:
                    {
                        _hbManager.AddHBHandler(handler);
                        break;
                    }

            }
        }

        /// <summary>
        /// 心跳处理
        /// </summary>
        public void HeartBeat(object sender, ClientEventArg arg)
        {
            LOG.Error(string.Format("客户端心跳处理"));
        }

        /// <summary>
        /// 客户端连接超时处理 callback
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        public void HeartbeatTimeout(object sender, ClientEventArg arg)
        {
            string ClientID = arg.ClientID;
            LOG.Error(string.Format("客户端:{0}连接超时", ClientID));

        }

        #endregion

        #region "接口实现"

        public void ConnectQueueHost(string ClientID)
        {
            LOG.Debug(string.Format("客户端{0}连接上线.", ClientID));

            string ClientHostName = OperationContext.Current.Channel.RemoteAddress.ToString();

            _hbManager.AddClient(ClientID);
        }

        public void DisconnectQueueHost(string ClientID)
        {
            LOG.Debug(string.Format("客户端{0}连接下线.", ClientID));

            _hbManager.RemoveClient(ClientID);
        }

        public void HeartBeat(string ClientID)
        {
            //LOG.Debug(string.Format("客户端{0}发来心跳信号.", ClientID));

            _hbManager.HeartBeat(ClientID);
        }

        public string GetQueueItemType(string QueueName)
        {
            try
            {
                TrackQueue queue = (TrackQueue)ResourceManager.GetResource(QueueName);

                //return queue.ItemType;
                return null; // DongMin改，为了编译通过
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("QueueService获取队列{0}项目类型出错：{1}", QueueName,ex.Message));
                return null;
            }
        }

        public string GetQueueItemCounts(string QueueName)
        {
            try
            {
                TrackQueue queue = (TrackQueue)ResourceManager.GetResource(QueueName);

                return queue.Counts.ToString();
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("QueueService获取队列{0}数量出错：{1}", QueueName, ex.Message));
                return null;
            }
        }

        public string[] GetQueueItems(string QueueName)
        {
            try
            {
                TrackQueue queue = (TrackQueue)ResourceManager.GetResource(QueueName);

                return queue.ListItemID();
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("QueueService获取队列{0}项目ID出错：{1}", QueueName, ex.Message));
                return null;
            }
        }

        public void EntryQueue(string QueueName, Int16 index, string ItemID)
        {
            try
            {
                TrackQueue queue = (TrackQueue)ResourceManager.GetResource(QueueName);

                queue.InsertByIndex(index, ItemID);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("QueueService增加队列出错：{0},位置{1},字段{2}", QueueName, index, ItemID, ex.Message));
            }
        }


        public void RemoveQueue(string QueueName, Int16 index)
        {
            try
            {
                TrackQueue queue = (TrackQueue)ResourceManager.GetResource(QueueName);

                queue.TakeOutAt(index);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("QueueService移出队列出错：{0},位置{1}", QueueName, index, ex.Message));
            }
        }

        public void ClearQueue(string QueueName)
        {
            try
            {
                TrackQueue queue = (TrackQueue)ResourceManager.GetResource(QueueName);

                queue.Clear();
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("QueueService清空队列出错：{0},位置{1}", QueueName, ex.Message));
            }
        }


        //void EntryQueue(string QueueName, string ItemID)
        //{ 
            
        //}

        //string ExitQueue(string QueueName)
        //{ }

        //public string[] GetBatteryStatusByTrayID(string TrayID)
        //{
        //    try
        //    {
        //        //TrackQueue queue = (TrackQueue)ResourceManager.GetResource(QueueName);
        //        //return queue.ListItemID();
        //        //TrackQueue _proxy= new TrackQueue("");
        //        ////string[] _batteryStatus=new string[48];
        //        //return _proxy.GetBatteryInfoByTrayID(TrayID);

        //        Tray tray = new Tray();
        //        return tray.GetBatteryIDs(TrayID);

        //    }

        //    catch (Exception ex)
        //    {
        //        LOG.Error(string.Format("分选机获取电池信息出错：{0}",  ex.Message));
        //        return null;
        //    }
        //}

        //public string[] GetBIMBatteryByTrayID(string TrayID)
        //{
        //    try
        //    {
        //        Tray tray = new Tray();
        //        return tray.GetBIMBatteryIDs(TrayID);
        //    }

        //    catch (Exception ex)
        //    {
        //        LOG.Error(string.Format("分选机获取电池信息出错：{0}",  ex.Message));
        //        return null;
        //    }
        //}

        public string[] GetBIMTrayIDs(string queueName)
        {
            TrackQueue queue = (TrackQueue)ResourceManager.GetResource(queueName);
            return queue.ListItemID();

        }

        public string[] GetBatteryStatusByTrayID(string TrayID)
        {
            throw new NotImplementedException();
        }

        public string[] GetBIMBatteryByTrayID(string TrayID)
        {
            throw new NotImplementedException();
        }

        #endregion

    }




}
