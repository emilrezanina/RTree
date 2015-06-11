using SpatialIndexStructures.Spatial;

namespace SpatialIndexStructures.RTree
{
    interface INode
    {
        ISpatialData GetGeometry();
    }
}
