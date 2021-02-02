namespace VTMap.Core.Enums
{
    public enum EventType
    {
        // Map position has changed
        PositionEvent,

        // Map was moved by user
        MoveEvent,

        // Map was scaled by user
        ScaleEvent,

        // Map was rotated by user
        RotateEvent,

        // Map was tilted by user
        TiltEvent,

        // Delivered on main-thread when updateMap() was called
        // and no CLEAR_EVENT or POSITION_EVENT was triggered
        UpdateEvent,

        // Map state has changed in a way that all layers
        // should clear their state e.g. the theme or the TilesSource has changed.
        // TODO should have an event-source to only clear affected layers.
        ClearEvent,

        // Animation is started
        AnimationStartEvent,

        // Animation is ended
        AnimationEndEvent,
    }
}
