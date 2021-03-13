using System;
using VTMap.Core.Enums;
using VTMap.Core.Primitives;

namespace VTMap.Core.Events
{
    public class PinchEventArgs : EventArgs
    {
        public Point PreviousPoint { get; }

        public Point NewPoint { get; }

        public Point PivotPoint { get; }

        public Point MidPoint { get; }

        public Point Translation { get; }

        public float Rotation { get; }

        public float Scale { get; }

        public TouchActionType TouchActionType { get; }

        public bool Handled { get; set; } = false;

        public PinchEventArgs(Point previousPoint, Point newPoint, Point pivotPoint, Point midPoint, Point trans, float rot, float scale, TouchActionType touchActionType)
        {
            PreviousPoint = previousPoint;
            NewPoint = newPoint;
            PivotPoint = pivotPoint;
            MidPoint = midPoint;
            Translation = trans;
            Rotation = rot;
            Scale = scale;
            TouchActionType = touchActionType;
        }
    }
}
