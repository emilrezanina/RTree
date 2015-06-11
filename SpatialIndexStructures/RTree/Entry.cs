using System;
using SpatialIndexStructures.Spatial;

namespace SpatialIndexStructures.RTree
{
    public class Entry : Node, IEquatable<Entry>
    {
        public ISpatialData Geometry { get; set; }
        public Entry(ISpatialData geometry, int maxCapacity) : base(maxCapacity)
        {
            Geometry = geometry;
        }

        public override ISpatialData GetGeometry()
        {
            return Geometry;
        }

        public bool Equals(Entry other)
        {
            return Geometry.Equals(other.Geometry);
        }

        public override string ToString()
        {
            return Geometry.ToString();
        }
    }
}
