using ProcessControlService.ResourceLibrary.Action;
using System;

namespace ProcessControlService.ResourceLibrary.Storage
{
    /// <summary>
    /// Storage Action基础类
    /// Created By David Dong at 20170407
    /// </summary>
    public abstract class StorageAction<T> : BaseAction where T : Storage
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(StorageAction<T>));

        public T OwnerStorage { get; set; }

        protected StorageAction(T storage, string name) : base(name)
        {
            OwnerStorage = storage;
        }

        protected StorageAction(string name) : base(name)
        {

        }

        public override BaseAction Clone()
        {
            var action = (StorageAction<T>)base.Clone();
            action.OwnerStorage = OwnerStorage;
            return action;
        }
    }

    /// <summary>
    /// 进存储操作Action
    /// Created by David Dong 20180218
    /// </summary>
    public class EntryStorageAction : StorageAction<Storage>
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(EntryStorageAction));


        public EntryStorageAction(Storage storage, string name) : base(storage, name)
        {
            CreateParameters();
        }

        #region "Core functions"

        public override void Execute()
        {
            /*
               if(ActionInParameterManager["EntryItem"].HasValue)
               {
                   var item = (TrackingUnit2)ActionInParameterManager["EntryItem"].GetValue();
                   if (item != null)
                   {
                       if (!string.IsNullOrEmpty(item.ID) && item.ID != "")
                       {
                           OwnerStorage.Entry(item);
                       }                    
                   }
               }
            */
        }


        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        private new bool CreateParameters()    //必须使用New以区分BaseAction里的
        {
            //  参数1：跟踪项目ID号
            /*
                        try
                        {
                            ActionInParameterManager.AddBasicParameter(new BasicParameter("EntryItem", "Object", OwnerStorage.ItemType));
                        }
                        catch (Message)
                        {
                            // ignored
                        }
            */

            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new EntryStorageAction(OwnerStorage, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };

            return basAction;

        }
    }

    /// <summary>
    /// 出存储操作Action
    /// Created by David Dong 20180218
    /// </summary>
    public class ExitStorageAction : StorageAction<Storage>
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ExitStorageAction));


        public ExitStorageAction(Storage storage, string name) : base(storage, name)
        {

            CreateParameters();
        }

        #region "Core functions"

        public override void Execute()
        {

            /*var vehicle =  OwnerStorage.Exit();

            ActionOutParameterManager["ExitItem"].SetValue(vehicle);*/
            //if (InParameterBinds["EntryItem"].HasValue)
            //{
            //    TrackingUnit item = (TrackingUnit)InParameterBinds["EntryItem"].GetValue();
            //    if (item != null)
            //    {
            //        _ownerStorage.Entry(item);
            //    }
            //}

        }


        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        private new bool CreateParameters()
        {
            try
            {
                /*
                                ActionOutParameterManager.Add(new Parameter("ExitItem", "Object", OwnerStorage.ItemType));
                */
            }
            catch (Exception)
            {
                // ignored
            }

            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new ExitStorageAction(OwnerStorage, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };

            return basAction;
        }
    }

    /// <summary>
    /// 以JSON格式获得状态
    /// </summary>
    public class GetStorageStatusAction : StorageAction<Storage>
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GetStorageStatusAction));

        public GetStorageStatusAction(Storage storage, string name) : base(storage, name)
        {
        }

        #region "Core functions"

        public override void Execute()
        {
            try
            {
                /*var status = OwnerStorage.GetStatus();
                ActionOutParameterManager["StorageStatus"].SetValue(status);*/
            }
            catch (Exception ex)
            {
                Log.Error($"执行StorageAction:出错：{ex.Message}");
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数1：跟踪项目ID号
            /*
                        ActionOutParameterManager.Add(new Parameter("StorageStatus", "String"));
            */

            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new GetStorageStatusAction(OwnerStorage, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };

            return basAction;
        }
    }

    /// <summary>
    /// ？？？
    /// </summary>
    public class GetFeatureStorageAction : StorageAction<Storage>
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GetFeatureStorageAction));



        public GetFeatureStorageAction(Storage storage, string name) : base(storage, name)
        {
            //_ownerStorage = 
        }

        #region "Core functions"

        public override void Execute()
        {

            /*var vehicle = OwnerStorage.Exit();

            ActionOutParameterManager["ExitItem"].SetValue(vehicle);*/
            //if (InParameterBinds["EntryItem"].HasValue)
            //{
            //    TrackingUnit item = (TrackingUnit)InParameterBinds["EntryItem"].GetValue();
            //    if (item != null)
            //    {
            //        _ownerStorage.Entry(item);
            //    }
            //}

        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数1：跟踪项目ID号
            /*ActionInParameterManager.Add(new Parameter("EntryItem", "Object", "Tracking.TrackingUnit"));
            ActionOutParameterManager.Add(new Parameter("ExitItem", "Object", "Tracking.TrackingUnit"));*/
            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new GetFeatureStorageAction(OwnerStorage, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };

            return basAction;
        }
    }

    /// <summary>
    /// 清除存储区Action
    /// David 20180812
    /// </summary>
    public class ClearStorageAction : StorageAction<Storage>
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ClearStorageAction));

        public ClearStorageAction(Storage storage, string name) : base(storage, name)
        {
        }

        #region "Core functions"

        public override void Execute()
        {
            OwnerStorage.Clear();
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数1：跟踪项目ID号

            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new ClearStorageAction(OwnerStorage, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };

            return basAction;
        }
    }

    /// <summary>
    /// 查询存储区位置
    /// David 20180812
    /// </summary>
    public class QueryStorageAction : StorageAction<Storage>
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(QueryStorageAction));

        public QueryStorageAction(Storage storage, string name) : base(storage, name)
        {
        }

        #region "Core functions"

        public override void Execute()
        {

            /*var pos = (short)ActionInParameterManager["Position"].GetValue();

            var itemId = OwnerStorage.GetPositionItemId(new StoragePosition(pos));

            ActionOutParameterManager["ItemID"].SetValue(itemId);*/


        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数1：跟踪项目ID号
            /*            ActionInParameterManager.Add(new Parameter("Position", "Int16"));
                        ActionOutParameterManager.Add(new Parameter("ItemID", "String"));*/
            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new QueryStorageAction(OwnerStorage, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };

            return basAction;
        }
    }
}
