using System;
using System.Collections.Generic;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Common;
using log4net;

namespace ProcessControlService.ResourceLibrary.Action
{
    public class ActionsManagement
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ActionsManagement));

        private static readonly Dictionary<string, Type> ActionTypeCollection = new Dictionary<string, Type>();

        public static void AddActionType(string actionType, Type type)
        {
            ActionTypeCollection.Add(actionType, type);
        }

        public static void GetAllActionType(Type[] actions)
        {
            
        }

        public static void Init()
        {  }

        public static IActionContainer GetContainer(string containerName)
        {
            try
            {
              
                var resource = ResourceManager.GetResource(containerName);

                if (resource is IActionContainer container)
                {
                    return container;
                }

                return null;
            }
            catch (Exception)
            {
                return null;   
            }
        }

        // 创建通用Action容器
        public static void BuildCommonActionContainer()
        {
            // 创建通用Action容器
            var common = new CommonResource("Common");
            ResourceManager.AddResource(common);

            // 创建产品Action容器
            
            //_common.AddAction(new WaitAction("WaitAction"));
            //_common.AddAction(new ShowStatusAction("ShowStatusAction"));

            ////20180428Robin增加
            //_common.AddAction(new CallProcessAction("CallProcessAction"));

            ////////////////////////////////////////////
            // 加载外包里面的Action
            //BaseAction _action1 = CreateAction("WaitAction2", "WaitAction2");
            //_action1.ActionContainer = _common;
            //_common.AddAction(_action1);
            ////////////////////////////////////////////

        }

        public static BaseAction CreateAction(string ActionType, string actionName)
        {
            try
            {
                //string AppPath = Assembly.GetExecutingAssembly().GetName().Name;
                //string FullActionType = AppPath + ".Action." + ActionType;

                var actionType = ActionTypeCollection[ActionType];
                var obj = Activator.CreateInstance(actionType, actionName);

                var action = (BaseAction)obj;
                //machineAction.OwnerMachine = Owner;
                return action;

            }
            catch (Exception ex)
            {
                Log.Error($"创建Action：{actionName}失败{ex.Message}.");
                return null;
            }
        }

    }
}
