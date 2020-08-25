using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Tracking;
using ProcessControlService.ResourceFactory;


namespace ProcessControlService.ResourceLibrary.Storages
{
    /// <summary>
    /// Storage Action基础类
    /// Created By David Dong at 20170407
    /// </summary>
    public abstract class StorageGroupActions : BaseAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(StorageGroupActions));

        public StorageGroup _ownerStorage;

        public StorageGroup OwnerStorage
        {
            get { return _ownerStorage; }
        }

        public StorageGroupActions(StorageGroup Storage, string name) : base(name)
        {
            _ownerStorage = Storage;
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

    }

    /// <summary>
    /// 指定ID进存储操作Action
    /// Created by David Dong 20180817
    /// </summary>
    public class EntryStorageGroupAction : StorageGroupActions
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(EntryStorageGroupAction));

        public EntryStorageGroupAction(StorageGroup Storage,string Name) : base(Storage,Name)
        {

            CreateParameters();
        }

        #region "Core functions"

        public override void Execute(RedundancyMode Mode)
        {
            try
            {
                Int16 subGroupID = (Int16)InParameters["SubGroupID"].GetValue();
                TrackingUnit2 item = (TrackingUnit2)InParameters["EntryItem"].GetValue();
               
                _ownerStorage.EntrySubGroup(subGroupID,item);
             
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("执行EntryStorageGroupAction 出错{0}", ex));
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

        private new bool CreateParameters()    //必须使用New以区分BaseAction里的
        {
            //  参数1：跟踪项目ID号
            try
            {
                InParameters.Add(new Parameter("SubGroupID", "Int16"));
                InParameters.Add(new Parameter("EntryItem", "Object", OwnerStorage.ItemType));
                return true;

            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("创建EntrySubStorageAction 出错{0}", ex));
                return false;
            }
        }

    }

    /// <summary>
    /// 指定ID出存储操作Action
    /// Created by David Dong 20180817
    /// </summary>
    public class ExitStorageGroupAction : StorageGroupActions
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ExitStorageGroupAction));

        public ExitStorageGroupAction(StorageGroup Storage, string Name) : base(Storage, Name)
        {

            CreateParameters();
        }

        #region "Core functions"

        public override void Execute(RedundancyMode Mode)
        {
            try
            {
                Int16 subGroupID = (Int16)InParameters["SubGroupID"].GetValue();

                TrackingUnit2 item = _ownerStorage.ExitSubGroup(subGroupID);

                OutParameters["ExitItem"].SetValue(item);

            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("执行ExitSubStorageAction 出错{0}", ex));
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

        private new bool CreateParameters()    //必须使用New以区分BaseAction里的
        {
            //  参数1：跟踪项目ID号
            try
            {
                InParameters.Add(new Parameter("SubGroupID", "Int16"));
                OutParameters.Add(new Parameter("ExitItem", "Object", OwnerStorage.ItemType));

                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("创建ExitStorageGroupAction参数 出错{0}", ex));
                return false;
            }
        }

    }

}
