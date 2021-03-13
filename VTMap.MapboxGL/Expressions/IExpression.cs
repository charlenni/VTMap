using VTMap.Core.Primitives;

namespace VTMap.MapboxGL.Expressions
{
    public interface IExpression
    {
        object Evaluate(EvaluationContext ctx);

        object PossibleOutputs();
    }
}
