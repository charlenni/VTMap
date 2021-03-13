using SkiaSharp.Views.Forms;
using VTMap.Core.Enums;
using VTMap.Core.Events;
using VTMap.Core.Primitives;

namespace VTMap.View.Forms.Extensions
{
    public static class TouchEventArgsExtension
    {
        public static TouchEventArgs ToTouchEventArgs(this SKTouchEventArgs value)
        {
            var id = value.Id;
            var actionType = (TouchActionType)value.ActionType;
            var mouseButton = (MouseButton)value.MouseButton;
            var deviceType = (TouchDeviceType)value.DeviceType;
            var location = new Point(value.Location.X, value.Location.Y);
            var inContact = value.InContact;
            var wheelDelta = value.WheelDelta;
            var pressure = value.Pressure;

            return new TouchEventArgs(id, actionType, mouseButton, deviceType, location, inContact, wheelDelta, pressure);
        }

    }
}
