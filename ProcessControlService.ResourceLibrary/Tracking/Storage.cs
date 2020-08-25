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
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    /// 存储基本类
    /// Created by Dongmin 20180218
    /// </summary>
    public abstract class Storage : IResource,IResourceExportService, IActionContainer,ILocation
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(Storage));

        protected string _storageName;

        protected Int32 _size; // 容量
        public Int32 Size => _size;

        public bool IsFull => Count >= _size; //是否已满

        abstract public Int32 Count { get; } //存货计数

        public Storage(string Name)
        {
            _storageName = Name;

            CreateActions();

        }

        #region "Resource definition"

        public virtual string ResourceName
        {
            get
            {
                return _storageName;
            }
        }

        public virtual string ResourceType
        {
            get
            {
                return "Storage";
            }
        }

        public virtual IResource GetResourceObject()
        {
            return this;
        }

        public virtual bool LoadFromConfig(XmlNode node)
        {
            throw new Exception("非法调用Storage：LoadFromConfig");
        }

        public virtual void FreeResource()
        {
            StopWork();
        }

        public void StartWork()
        {
            LOG.Info(string.Format("设备{0}监控启动.", ResourceName));

        }

        private RedundancyMode _mode = RedundancyMode.Unknown;

        public RedundancyMode Mode
        {
            get { return _mode; }
        }

     

        public void RedundancyModeChange(RedundancyMode Mode)
        {
            _mode = Mode;
        }

        public void StopWork()
        {
            LOG.Info(string.Format("设备{0}监控停止.", ResourceName));
          

        }

        public IResourceExportService GetExportService()
        {
            return this;
        }

        #endregion

        #region "Resource Service"
        public List<string> GetExportServiceNames()
        {
            List<string> ServiceNames = new List<string>();

            ServiceNames.Add("GetStorageStatus");
            return ServiceNames;
        }

        
        public string CallExportService(string ServiceName)
        {
            if (ServiceName == "GetStatus")
            {
                return GetStatus();
            }
            else
                throw new Exception(string.Format("所调用接口{0}不存在.", ServiceName));
        }

        public virtual string GetStatus()
        {
            StorageStatusModel statusModel = new StorageStatusModel(Count,Size);
            DataContractJsonSerializer json = new DataContractJsonSerializer(statusModel.GetType());
            string szJson = "";
            using (MemoryStream stream = new MemoryStream())
            {
                json.WriteObject(stream, statusModel);
                szJson = Encoding.UTF8.GetString(stream.ToArray());
            }
            return szJson;
        }

        #endregion

        #region "ActionContainer"

        protected ActionCollection _actions = new ActionCollection();

        public void AddAction(BaseAction action)
        {
            _actions.AddAction(action);
        }

        public BaseAction GetAction(string name)
        {
            return (StorageAction)_actions.GetAction(name);
        }

        public virtual void ExecuteAction(string name)
        {
            StorageAction _action = (StorageAction)GetAction(name);
            _action.Execute(_mode);
        }

        public string[] ListActionNames()
        {
            return _actions.ListActionNames();
        }

        protected virtual void CreateActions()
        {
            // add actions
            _actions.AddAction(new EntryStorageAction(this, "EntryStorageAction"));
            _actions.AddAction(new GetStorageJsonAction(this, "GetStorageJsonAction"));
            _actions.AddAction(new ExitStorageAction(this, "ExitStorageAction"));


        }


        #endregion

        #region "Location"
        public string LocationID
        {
            get => _storageName;
            set => _storageName=value;
        }

        virtual public bool AcceptTrackUnit(TrackingUnit2 Unit)
        {//如果非满就允许
            return !IsFull;
        }

        public TrackingUnit2 QueryUnit() //查看
        {
            throw new NotImplementedException();
        }


        public TrackingUnit2 GetUnit()
        {
            return Exit(); 
        }

        public void RemoveUnit(TrackingUnit2 unit)//删除
        {
            throw new NotImplementedException();
        }

        public void PutUnit(TrackingUnit2 unit)
        {
            Entry(unit);
        }

        public Int32 GetUnitCount()
        {
            return Count;
        }

        virtual public bool CouldCreatePlace(TrackingUnit2 Unit)
        {
            throw new NotImplementedException();
        }

        virtual public bool HasOutput()
        {
            return Count>0;
        }

        virtual public bool Occupied { get; set; } //是否已经有处理进程


        #endregion

        public abstract void Entry(TrackingUnit2 Item);

        public abstract TrackingUnit2 Exit();

       
    }

    [DataContract]
    class StorageStatusModel
    {
        [DataMember]
        Int32 Count = 0; //仓库货物数量

        [DataMember]
        Int32 Size = 0; //仓库货物容量

        public StorageStatusModel(Int32 Count, Int32 Size)
        {
            this.Count = Count;
            this.Size = Size;
        }
      
    }

}
