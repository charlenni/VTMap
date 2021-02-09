using System;

namespace VTMap.Core.Events
{
    public interface ITouchEventHandler
    {
        /// <summary>
        /// Time in milliseconds until a second touch event is registered as double tap
        /// </summary>
        TimeSpan DoubleTapDelay { get; set; }

        /// <summary>
        /// Time in milliseconds until a touch event is resitered as long press
        /// </summary>
        TimeSpan LongPressDelay { get; set; }

        /// <summary>
        /// MaxMoveDistance is the number of pixel that distinguish between tap and pan
        /// </summary>
        int MaxMoveDistance { get; set; }

        event EventHandler<TouchEventArgs> TouchDown;
        event EventHandler<TouchEventArgs> TouchUp;
        event EventHandler<TouchEventArgs> TouchMove;
        event EventHandler<TapEventArgs> Tapping;
        event EventHandler<TapEventArgs> SingleTapped;
        event EventHandler<TapEventArgs> DoubleTapped;
        event EventHandler<TapEventArgs> LongPressing;
        event EventHandler<TapEventArgs> LongPressed;
        event EventHandler<PanEventArgs> Panning;
        event EventHandler<PanEventArgs> Panned;
        event EventHandler<PinchEventArgs> Pinching;
        event EventHandler<PinchEventArgs> Pinched;
        event EventHandler<SwipeEventArgs> Swiped;
        event EventHandler<WheelChangedEventArgs> WheelChanged;

        /// <summary>
        /// Handle any touch events that is raised
        /// </summary>
        /// <param name="e">Arguments for this touch event</param>
        void Handle(TouchEventArgs e);
    }
}
