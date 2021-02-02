using System;
using VTMap.Core.Enums;
using VTMap.Core.Primitives;

namespace VTMap.Core.Events
{
    public class SwipeEventArgs : EventArgs
    {
        public Point LastPoint { get; }

        public Vector TranslationVelocity { get; }

        public float RotationVelocity { get; }

        public float ScaleVelocity { get; }

        public TouchActionType TouchActionType { get; }

        public bool Handled { get; set; } = false;

        public SwipeEventArgs(Point lastPoint, Vector transVelocity, float rotVelocity, float scaleVelocity, TouchActionType touchActionType)
        {
            LastPoint = lastPoint;
            TranslationVelocity = transVelocity;
            RotationVelocity = rotVelocity;
            ScaleVelocity = scaleVelocity;
            TouchActionType = touchActionType;
        }
    }
}
