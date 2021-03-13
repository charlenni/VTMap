using VTMap.Core.Primitives;

namespace VTMap.Core.Events
{
    public class TouchManipulationInfo
    {
        public Point PreviousPoint { set; get; }

        public Point NewPoint { set; get; }

        public bool IsMoving { get; set; } = false;
    }
}