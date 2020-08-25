// ==================================================
// 文件名：Storage.cs
// 创建时间：2020/01/02 18:11
// ==================================================
// 最后修改于：2020/05/21 18:11
// 修改人：jians
// ==================================================

using Newtonsoft.Json;
using NLog;
using ProcessControlService.Contracts;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Storage.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Storage
{
    /// <summary>
    ///     存储基本类
    ///     Created by Dongmin 20180218
    /// </summary>
    public abstract class Storage : XMLConfig, IResource, IResourceExportService, IActionContainer
    {
        public virtual string StorageName { get; set; }

        protected Storage(string Name)
        {
            StorageName = Name;
            Log = LogManager.GetLogger($"Storage/{Name}");
            Log.SetProperty("filename", Name);
        }


        #region Private Propertys
        protected Logger Log { get; set; }

        protected string DbName { get; set; }

        protected IFreeSql Fsql { get; set; }

        protected XmlNode ConfigXmlNode { get; set; }
        #endregion

        /// <summary>
        /// 进入规则
        /// </summary>
        protected IEntryRule EntryRule { get; set; }

        /// <summary>
        /// 进入规则
        /// </summary>
        protected IExitRule ExitRule { get; set; }

        public abstract int Count { get; } //存货计数

        public virtual int Size { get; set; }

        public string CurrentEntryRule => EntryRule?.GetType().Name;
        public string CurrentExitRule => ExitRule?.GetType().Name;

        #region Public Methods
        /// <summary>
        /// 修改入库规则
        /// </summary>
        /// <param name="entryRule"></param>
        public void ChangeEntryRule(IEntryRule entryRule)
        {
            EntryRule = entryRule;
        }

        /// <summary>
        /// 修改出库规则
        /// </summary>
        /// <param name="exitRule"></param>
        public void ChangeExitRule(IExitRule exitRule)
        {
            ExitRule = exitRule;
        }

        public Coordinate GetNextEntryPosition<T>(T entryItem)
        {
            return EntryRule.GetNextEntryPosition(entryItem);
        }

        public Coordinate GetNextExitPosition()
        {
            return ExitRule.GetNextExitPosition();
        }


        public virtual void Clear()
        {
            
        }

        public virtual bool ContainsItem(string Id)
        { return false; }

        #endregion

        #region "Resource definition"

        public object ResourceLocker { get; } = new object();

        public virtual string ResourceName => StorageName;

        public virtual string ResourceType => "Storage";

        public virtual IResource GetResourceObject()
        {
            return this;
        }

        public virtual bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level1Item = (XmlElement)node;
                if (level1Item.HasAttribute("Size"))
                {
                    var strSize = level1Item.GetAttribute("Size");
                    Size = Convert.ToInt16(strSize);
                }
                foreach (XmlElement item in node.ChildNodes)
                {
                    if (item.Name == "Actions")
                    {
                        LoadActionsFromXml(item);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"加载Storage:{ResourceName}出错：{ex.Message}");
                return false;
            }
        }

        protected virtual void LoadActionsFromXml(XmlElement Actions)
        {
            //foreach (XmlElement actionEle in Actions.ChildNodes)
            //{
            //    var name = actionEle.GetAttribute("Name");
            //    var actionType = actionEle.GetAttribute("Type");

            //    var action = (StorageAction<Storage>)ActionsManagement.CreateAction(actionType, name);
            //    action.OwnerStorage = this;

            //    if (action.LoadFromConfig(actionEle))
            //    {
            //        try
            //        {
            //            AddAction(action);
            //        }
            //        catch (Exception ex)
            //        {
            //            Log.Error($"加载Storage{ResourceName} 的Action:{name}出错:{ex}");
            //        }
            //    }
            //    else
            //    {
            //        Log.Error($"加载Storage{ResourceName} 的Action:{name}出错");
            //    }
            //}
        }

        public virtual void FreeResource()
        {
            //StopWork();
        }

        public IResourceExportService GetExportService()
        {
            return this;
        }

        #endregion

        #region "Resource Service"

        [StdContract]
        public List<ResourceServiceModel> GetExportServices()
        {
            var svcs = new List<ResourceServiceModel>();
            foreach (var med in GetType().GetMethods())
            {
                var att = med.GetCustomAttribute(typeof(StdContractAttribute), false);
                if (att is StdContractAttribute)
                {
                    var paras = new List<ServiceParameterModel>();
                    foreach (var para in med.GetParameters())
                    {
                        paras.Add(new ServiceParameterModel()
                        {
                            Name = para.Name,
                            Type = para.ParameterType.ToString(),
                            Value = para.DefaultValue?.ToString()
                        });
                    }
                    svcs.Add(new ResourceServiceModel()
                    {
                        Name = med.Name,
                        Parameters = paras
                    });
                }
            }
            return svcs;
        }

        public string CallExportService(string serviceName, string strParameter = null)
        {
            try
            {
                var paras = JsonConvert.DeserializeObject<List<ServiceParameterModel>>(strParameter) ?? new List<ServiceParameterModel>();

                var services = GetType().GetMethod(serviceName);
                if (services == null)
                {
                    return "";
                }

                var objects = new List<object>();
                var ps = services.GetParameters();

                foreach (var pa in ps)
                {
                    objects.AddRange(from item in paras
                                     where pa.Name == item.Name
                                     select Convert.ChangeType(item.Value, pa.ParameterType));
                }

                var res = services.Invoke(this, objects.ToArray());

                return res == null ? "" : res.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// 获取Storage 所有信息
        /// </summary>
        /// <returns></returns>
        [StdContract]
        public abstract string GetStorageStatus();

        #endregion

        #region "ActionContainer"

        private readonly ActionCollection _actions = new ActionCollection();

        public void AddAction(BaseAction action)
        {
            _actions.AddAction(action);
        }

        public BaseAction GetAction(string name)
        {
            return _actions.GetAction(name);
        }

        public RedundancyMode CurrentRedundancyMode { get; } = RedundancyMode.Unknown;

        public virtual void ExecuteAction(string name)
        {
            var action = GetAction(name);
            action.Execute();
        }

        public string[] ListActionNames()
        {
            return _actions.ListActionNames();
        }

        #endregion

        /// <summary>
        /// 仿真速度
        /// </summary>
        public int SimulateJPH { get; set; } = 60;

    }
}