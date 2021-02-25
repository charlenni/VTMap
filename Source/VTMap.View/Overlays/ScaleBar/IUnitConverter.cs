//
// Found at https://github.com/mapsforge/vtm/blob/master/vtm/src/org/oscim/scalebar/DistanceUnitAdapter.java
//

using System.Collections.Generic;

namespace VTMap.View.Overlays.ScaleBar
{
    public interface IUnitConverter
    {
        double MeterRatio { get; }

        IEnumerable<int> ScaleBarValues { get; }

        string GetScaleText(int mapScaleValue);
    }
}