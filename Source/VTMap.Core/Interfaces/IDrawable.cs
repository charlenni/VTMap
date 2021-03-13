using VTMap.Core.Primitives;

namespace VTMap.Core.Interfaces
{
    /// <summary>
    /// Interface for all vector drawables
    /// </summary>
    public interface IDrawable
    {
        EvaluationContext Context { get; }
    }
}
