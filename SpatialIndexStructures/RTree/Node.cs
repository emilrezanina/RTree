using System.Collections.Generic;
using SpatialIndexStructures.Spatial;

namespace SpatialIndexStructures.RTree
{
    public abstract class Node : INode
    {
        protected readonly int MaxCapacity;
        protected readonly int MinCapacity;

        protected Node(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            MinCapacity = maxCapacity/2;
        }
        
        public abstract ISpatialData GetGeometry();

        
    }
}
