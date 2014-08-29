using System;
using SpatialIndexStructures.Spatial;

namespace SpatialIndexStructures.RTree
{
    class Entry : Node, IEquatable<Entry>
    {
        private readonly ISpatialData _geometry;
        public Entry(ISpatialData geometry, int maxCapacity) : base(maxCapacity)
        {
            _geometry = geometry;
        }

        public override ISpatialData GetGeometry()
        {
            return _geometry;
        }

        public bool Equals(Entry other)
        {
            return _geometry.Equals(other._geometry);
        }

        public override string ToString()
        {
            return _geometry.ToString();
        }
    }
}
