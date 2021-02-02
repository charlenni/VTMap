namespace VTMap.Core.Events
{
    public interface IUpdatedEventHandler
    {
        void OnUpdated(object sender, UpdateEventArgs args);
    }
}
