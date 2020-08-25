using System;
using System.Reflection;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Event;
using ProcessControlService.ResourceLibrary.Machines.DataSources;

namespace ProcessControlService.ResourceLibrary
{
    // 主资源包描述文件
    public class PackageProfile : BaseProfile
    {
        public PackageProfile(Assembly assembly) : base(assembly)
        { // 初始化包
        }

        //public override List<string> GetResourceNames()
        //{ // 获得所有资源名
        //    return (Enum.GetNames(typeof(ResourceType))).ToList();
        //}

        protected override void InitResources()
        {
            ActionsManagement.Init();

            base.InitResources();
        }

        public override string GetPackageName()
        {
            return "ProcessControlService.ResourceLibrary";
        }

        public override void LoadAdditionalResource()
        {
            LoadActions();

            LoadEvents(); // Dongmin 20180807

            base.LoadAdditionalResource();

        }

        #region Actions
        private void LoadActions()
        {

            // 收集本身包里的Actions
            CollectActionsInSelfPackage();

            // 收集其它包里的Actions
            foreach (BaseProfile profile in OtherPackages)
            {
                CollectActionsInOtherPackage(profile);
            }

            // 创建通用Action的容器
            ActionsManagement.BuildCommonActionContainer();
        }



        /// ///////////////////////////////////////
        /// 以下为本Package自己的任务
        private void CollectActionsInOtherPackage(BaseProfile profile)
        {
            // 整合别的包里面的Action
            foreach (Type type in profile.GetAllTypes())
            {
                if (type.IsSubclassOf(typeof(BaseAction)))
                {
                    ActionsManagement.AddActionType(type.Name, type);
                }
                else if (type.IsSubclassOf(typeof(DataSource)))
                {
                    DataSourceManagement.AddDataSourceType(type.Name, type);
                }
            }
        }

        private void CollectActionsInSelfPackage()
        {
            // 整合自己包里面的Action
            foreach (Type type in GetAllTypes())
            {
                if (type.IsSubclassOf(typeof(BaseAction)))
                {
                    ActionsManagement.AddActionType(type.Name, type);
                }
                else if (type.IsSubclassOf(typeof(DataSource)))
                {
                    DataSourceManagement.AddDataSourceType(type.Name, type);
                }
            }
        }
        #endregion

        #region Events
        private void LoadEvents()
        {

            // 收集本身包里的Actions
            CollectEventsInSelfPackage();

            // 收集其它包里的Actions
            foreach (BaseProfile profile in OtherPackages)
            {
                CollectEventsInOtherPackage(profile);
            }

        }

        /// ///////////////////////////////////////
        /// 以下为本Package自己的任务
        private void CollectEventsInOtherPackage(BaseProfile profile)
        {
            // 整合别的包里面的Action
            foreach (Type type in profile.GetAllTypes())
            {
                if (type.IsSubclassOf(typeof(BaseEvent)))
                {
                    EventsManagement.AddEventType(type.Name, type);
                }
            }
        }

        private void CollectEventsInSelfPackage()
        {
            // 整合自己包里面的Action
            foreach (Type type in GetAllTypes())
            {
                if (type.IsSubclassOf(typeof(BaseEvent)))
                {
                    EventsManagement.AddEventType(type.Name, type);
                }
            }
        }
        #endregion
    }

    public enum ResourceType
    {
        CommonResource = 0,
        Process = 1,
        Machine = 2,
        Queue = 3,
        Storage = 4,
        Product = 5,
        Order = 6
    }
}
