using VTMap.Core.Enums;
using VTMap.Core.Primitives;

namespace VTMap.Core.Events
{
    public class WheelChangedEventArgs
    {
        public long Id { get; }
        public Point Location { get; private set; }
        public int Delta { get; }
        public MouseButton MouseButton { get; }

        public WheelChangedEventArgs(long id, Point location, int delta, MouseButton button)
        {
            Id = id;
            Location = location;
            Delta = delta;
            MouseButton = button;
        }
    }
}
