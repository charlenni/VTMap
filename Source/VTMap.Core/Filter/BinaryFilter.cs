﻿using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public abstract class BinaryFilter : Filter
    {
        public string Key { get; }
        public object Value { get; }

        public BinaryFilter()
        {
        }

        public BinaryFilter(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public abstract override bool Evaluate(IVectorElement feature);
    }
}
