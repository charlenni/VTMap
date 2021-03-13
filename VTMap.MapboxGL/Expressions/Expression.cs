using VTMap.Core.Primitives;

namespace VTMap.MapboxGL.Expressions
{
    public class Expression : IExpression
    {
        public virtual object Evaluate(EvaluationContext ctx)
        {
            return null;
        }

        public virtual object PossibleOutputs()
        {
            return null;
        }
    }
}
