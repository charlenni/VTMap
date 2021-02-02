// Found at SkiaSharp Forms implementation
// See at https://github.com/mono/SkiaSharp

using System;
using VTMap.Core.Enums;

namespace VTMap.Core.Events
{
	public class TouchEventArgs : EventArgs
	{
		public TouchEventArgs(long id, TouchActionType type, Point location, bool inContact)
			: this(id, type, MouseButton.Left, TouchDeviceType.Touch, location, inContact, 0, 1)
		{
		}

		public TouchEventArgs(long id, TouchActionType type, MouseButton mouseButton, TouchDeviceType deviceType, Point location, bool inContact)
			: this(id, type, mouseButton, deviceType, location, inContact, 0, 1)
		{
		}

		public TouchEventArgs(long id, TouchActionType type, MouseButton mouseButton, TouchDeviceType deviceType, Point location, bool inContact, int wheelDelta)
			: this(id, type, mouseButton, deviceType, location, inContact, wheelDelta, 1)
		{
		}

		public TouchEventArgs(long id, TouchActionType type, MouseButton mouseButton, TouchDeviceType deviceType, Point location, bool inContact, int wheelDelta, float pressure)
		{
			Id = id;
			ActionType = type;
			DeviceType = deviceType;
			MouseButton = mouseButton;
			Location = location;
			InContact = inContact;
			WheelDelta = wheelDelta;
			Pressure = pressure;
		}

		public bool Handled { get; set; } = false;

		public long Id { get; private set; }

		public TouchActionType ActionType { get; private set; }

		public TouchDeviceType DeviceType { get; private set; }

		public MouseButton MouseButton { get; private set; }

		public Point Location { get; private set; }

		public bool InContact { get; private set; }

		public int WheelDelta { get; private set; }

		public float Pressure { get; private set; }

		public override string ToString()
		{
			return $"{{ActionType={ActionType}, DeviceType={DeviceType}, Handled={Handled}, Id={Id}, InContact={InContact}, Location={Location}, MouseButton={MouseButton}, WheelDelta={WheelDelta}, Pressure={Pressure}}}";
		}
	}
}