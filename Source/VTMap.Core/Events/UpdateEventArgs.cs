using VTMap.Core.Enums;

namespace VTMap.Core.Events
{
    public class UpdateEventArgs
    {
        public EventType EventType { get; }

        public UpdateEventArgs(EventType eventType)
        {
            EventType = eventType;
        }
    }
}
