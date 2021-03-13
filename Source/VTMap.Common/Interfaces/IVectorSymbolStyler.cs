using SkiaSharp;
using VTMap.Common.Primitives;
using VTMap.Core;
using VTMap.Core.Primitives;

namespace VTMap.Common.Interfaces
{
    public interface IVectorSymbolStyler
    {
        bool HasIcon { get; }

        bool HasText { get; }

        Symbol CreateIconSymbol(SKPoint point, TagsCollection tags, EvaluationContext context);

        Symbol CreateTextSymbol(SKPoint point, TagsCollection tags, EvaluationContext context);

        Symbol CreateIconTextSymbol(SKPoint point, TagsCollection tags, EvaluationContext context);

        Symbol CreatePathSymbols(VectorElement element, EvaluationContext context);
    }
}
