namespace VTMap.Core.Events
{
    public class TapEventArgs
    {
        public long Id { get; }
        public Point Location { get; }

        public TapEventArgs(long id, Point point)
        {
            Id = id;
            Location = point;
        }
    }
}
