using SkiaSharp;
using VTMap.Core.Primitives;

namespace VTMap.Common.Interfaces
{
    public interface IVectorPaint
    {
        /// <summary>
        /// Creates a SKPaint to 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        SKPaint CreatePaint(EvaluationContext context);
    }
}
