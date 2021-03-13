using System.Collections.Generic;
using VTMap.Common.Interfaces;
using VTMap.Core.Enums;
using VTMap.Core.Filter;

namespace VTMap.MapboxGL
{
    public class MGLStyleLayer : IVectorStyleLayer
    {
        public string Id { get; internal set; }

        public int MinZoom { get; internal set; }

        public int MaxZoom { get; internal set; }

        public string SourceLayer { get; internal set; }

        public StyleType Type { get; internal set; }

        public IFilter Filter { get; internal set; }

        public IEnumerable<IVectorPaint> Paints { get; internal set; } = new List<MGLPaint>();

        public IVectorSymbolStyler SymbolStyler { get; internal set; } = MGLSymbolStyler.Default;

        public bool IsVisible { get; internal set; } = true;

        public MGLStyleLayer()
        {
        }
    }
}
