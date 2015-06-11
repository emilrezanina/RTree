using System;

namespace SpatialIndexStructures.Spatial
{
    public interface ISpatialData : IEquatable<ISpatialData>
    {
        double GetArea();
        Rectangle ToRectangle();
        double Distance(ISpatialData geometry);
    }
}
