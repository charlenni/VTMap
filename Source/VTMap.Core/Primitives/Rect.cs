namespace VTMap.Core.Primitives
{
    public struct Rect
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public float Width => Right - Left;

        public float Height => Bottom - Top;

        public Rect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
