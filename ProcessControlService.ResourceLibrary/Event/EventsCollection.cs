using System.Collections.Generic;
using System.Linq;

namespace ProcessControlService.ResourceLibrary.Event
{
    public class EventCollection
    {
        private readonly Dictionary<string, BaseEvent> _events = new Dictionary<string, BaseEvent>();

        public void AddEvent(BaseEvent Event)
        {
            _events.Add(Event.Name, Event);
        }

        public BaseEvent GetEvent(string name)
        {
            return _events[name];
        }

        public string[] ListEventNames()
        {
            return _events.Keys.ToArray();
        }

    }
}
