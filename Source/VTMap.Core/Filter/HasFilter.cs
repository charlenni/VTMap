using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class HasFilter : Filter
    {
        public string Key { get; }

        public HasFilter(string key)
        {
            Key = key;
        }

        public override bool Evaluate(IVectorElement feature)
        {
            return feature != null && feature.Tags.ContainsKey(Key);
        }
    }
}
