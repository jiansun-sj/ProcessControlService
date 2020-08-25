using System;
using System.Collections.Generic;
using log4net;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ResourceLibrary.Event
{
    public class EventsManagement
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventsManagement));

        private static readonly Dictionary<string, Type> EventTypeCollection = new Dictionary<string, Type>();

        public static void AddEventType(string eventType, Type type)
        {
            EventTypeCollection.Add(eventType, type);
        }

        public static void GetAllEventType(Type[] events)
        {

        }

        private static readonly List<IEventContainer> EventContainerCollection = new List<IEventContainer>();

        public static List<IEventContainer> GetAllEventContainer()
        {
            return EventContainerCollection;
        }

        public static void AddEventContainer(IEventContainer container)
        {
            EventContainerCollection.Add(container);
        }

        public static void Init()
        { }

        public static IEventContainer GetContainer(string containerName)
        {
            try
            {

                var resource = ResourceManager.GetResource(containerName);

                if (resource is IEventContainer container)
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

        public static BaseEvent CreateEvent(string eventTypeName, string eventName)
        {
            try
            {
                //string AppPath = Assembly.GetExecutingAssembly().GetName().Name;
                //string FullEventType = AppPath + ".Event." + EventType;

                var eventType = EventTypeCollection[eventTypeName];
                var obj = Activator.CreateInstance(eventType, eventName);

                var _Event = (BaseEvent)obj;
                //machineEvent.OwnerMachine = Owner;

                return _Event;
            }
            catch (Exception ex)
            {
                Log.Error($"创建Event：{eventName}失败{ex.Message}.");
                return null;
            }
        }

    }
}
