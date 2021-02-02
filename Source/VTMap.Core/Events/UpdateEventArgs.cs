using VTMap.Core.Enums;
using VTMap.Core.Map;

namespace VTMap.Core.Events
{
    public class UpdateEventArgs
    {
        public EventType EventType { get; }
        public Camera Position { get; }

        public UpdateEventArgs(EventType eventType, Camera position)
        {
            EventType = eventType;
            Position = position;
        }
    }
}
