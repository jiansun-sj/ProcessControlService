using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ResourceLibrary.Event
{
    /// <summary>
    /// BaseEvent类
    /// 描述机器触发事件
    /// Created by: DongMin 20180806
    /// </summary>
    public abstract class BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BaseEvent));

        public string Name { get; }

        public IResource OwnerResource; //宿主资源

        protected BaseEvent(string eventName)
        {
            Name = eventName;
        }

        public abstract bool IsTriggered(); // 返回事件是否被触发状态

        public abstract object GetResult(); // 返回事件的各种结果值

        public abstract void UpdateEvent(); // 被客户端调用，定义检查事件的代码

        public abstract bool LoadFromConfig(XmlNode node);

    }
}
