using VTMap.Common.Primitives;
using VTMap.Core.Enums;

namespace VTMap.Common.Interfaces
{
    public interface ITileDataSink
    {
        void Process(VectorElement element);

        /// <summary>
        /// Notify loader that tile loading is completed.
        /// </summary>
        /// <param name="result"></param>
        void Completed(QueryResult result);
    }
}
