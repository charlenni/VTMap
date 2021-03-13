using System;
using VTMap.Core.Enums;
using VTMap.Core.Primitives;

namespace VTMap.Core.Events
{
    public class PanEventArgs : EventArgs
    {
        public Point PreviousPoint { get; }

        public Point NewPoint { get; }

        public TouchActionType TouchActionType { get; }

        public bool Handled { get; set; } = false;

        public PanEventArgs(Point previousPoint, Point newPoint, TouchActionType touchActionType)
        {
            PreviousPoint = previousPoint;
            NewPoint = newPoint;
            TouchActionType = touchActionType;
        }
    }
}
