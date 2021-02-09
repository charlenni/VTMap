namespace VTMap.Core.Events
{
    public class ViewportChangedEventArgs
    {
        public string Name { get; }

        public ViewportChangedEventArgs(string name)
        {
            Name = name;
        }
    }
}
