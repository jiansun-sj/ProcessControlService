namespace ProcessControlService.ResourceLibrary.Event
{
    // Event 的容器接口
    public interface IEventContainer
    {
        //string ContainerName
        //{ get; }

        void AddEvent(BaseEvent Event);

        BaseEvent GetEvent(string eventName);

        string[] ListEventNames();

    }

}
