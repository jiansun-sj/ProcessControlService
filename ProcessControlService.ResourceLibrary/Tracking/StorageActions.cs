using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceFactory;


namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    /// Storage Action基础类
    /// Created By David Dong at 20170407
    /// </summary>
    public abstract class StorageAction : BaseAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(StorageAction));

        public Storage _ownerStorage;

        public Storage OwnerStorage
        {
            get { return _ownerStorage; }
        }

        public StorageAction(Storage Storage, string name) : base(name)
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
    /// 进存储操作Action
    /// Created by David Dong 20180218
    /// </summary>
    public class EntryStorageAction : StorageAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(EntryStorageAction));

        

        public EntryStorageAction(Storage Storage,string Name) : base(Storage,Name)
        {
            //_ownerStorage = 
        }

        #region "Core functions"

        public override void Execute(RedundancyMode Mode)
        {
            if(InParameters["EntryItem"].HasValue)
            {
                TrackingUnit2 item = (TrackingUnit2)InParameters["EntryItem"].GetValue();
                if (item != null)
                {
                    if (item.ID != null && item.ID != string.Empty && item.ID != "")
                    {
                        _ownerStorage.Entry(item);
                    }                    
                }
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
            InParameters.Add(new Parameter("EntryItem", "Object", "Tracking.TrackingUnit"));

            return true;
        }

    }

    public class ExitStorageAction : StorageAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ExitStorageAction));



        public ExitStorageAction(Storage Storage, string Name) : base(Storage, Name)
        {
            //_ownerStorage = 
        }

        #region "Core functions"

        public override void Execute(RedundancyMode Mode)
        {
            
            TrackingUnit2 _vehicle =  _ownerStorage.Exit();

            OutParameters["ExitItem"].SetValue(_vehicle);
            //if (InParameters["EntryItem"].HasValue)
            //{
            //    TrackingUnit item = (TrackingUnit)InParameters["EntryItem"].GetValue();
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
            InParameters.Add(new Parameter("EntryItem", "Object", "Tracking.TrackingUnit"));
            OutParameters.Add(new Parameter("ExitItem", "Object", "Tracking.TrackingUnit"));
            return true;
        }

    }

    /// <summary>
    /// 以JSON格式获得
    /// </summary>
    public class GetStorageJsonAction : StorageAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(GetStorageJsonAction));

        public GetStorageJsonAction(Storage Storage, string Name) : base(Storage, Name)
        {
        }

        #region "Core functions"

        public override void Execute(RedundancyMode Mode)
        {
            //string jsonStroage = _ownerStorage.ToJson();
            //OutParameters["StorageJson"].SetValue(jsonStroage);
            throw new Exception("没有ToJson方法");
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
            OutParameters.Add(new Parameter("StorageJson", "String"));

            return true;
        }
    }


    public class GetFeatureStorageAction : StorageAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(GetFeatureStorageAction));



        public GetFeatureStorageAction(Storage Storage, string Name) : base(Storage, Name)
        {
            //_ownerStorage = 
        }

        #region "Core functions"

        public override void Execute(RedundancyMode Mode)
        {

            TrackingUnit2 _vehicle = _ownerStorage.Exit();

            OutParameters["ExitItem"].SetValue(_vehicle);
            //if (InParameters["EntryItem"].HasValue)
            //{
            //    TrackingUnit item = (TrackingUnit)InParameters["EntryItem"].GetValue();
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
            InParameters.Add(new Parameter("EntryItem", "Object", "Tracking.TrackingUnit"));
            OutParameters.Add(new Parameter("ExitItem", "Object", "Tracking.TrackingUnit"));
            return true;
        }

    }


}
