using System.Collections.Generic;

namespace SpatialIndexStructures.Spatial
{
    public interface ISpatialIndex
    {
        void Insert(ISpatialData geometry);
        bool Delete(ISpatialData geometry);
        IEnumerable<ISpatialData> Search(ISpatialData geometry);
        IEnumerable<ISpatialData> SearchRange(IArea area);
    }
}
