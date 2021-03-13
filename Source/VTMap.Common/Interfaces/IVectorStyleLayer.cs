using System.Collections.Generic;
using VTMap.Core.Enums;
using VTMap.Core.Filter;

namespace VTMap.Common.Interfaces
{
    public interface IVectorStyleLayer
    {
        /// <summary>
        /// Minimal zoom from which this style layer is used
        /// </summary>
        int MinZoom { get; }

        /// <summary>
        /// Maximal zoom up to which this style layer is used
        /// </summary>
        int MaxZoom { get; }

        /// <summary>
        /// Name of source layer this style layer belongs to 
        /// </summary>
        string SourceLayer { get; }

        /// <summary>
        /// Type of this style layer
        /// </summary>
        StyleType Type { get; }

        /// <summary>
        /// Is this style layer visible
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Filter used for this style layer
        /// </summary>
        IFilter Filter { get; }

        /// <summary>
        /// Paint to use to draw the features
        /// </summary>
        IEnumerable<IVectorPaint> Paints { get; }

        /// <summary>
        /// Symbol styler to use for creating symbols
        /// </summary>
        IVectorSymbolStyler SymbolStyler { get; }
    }
}
