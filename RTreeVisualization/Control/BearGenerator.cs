using System;
using RTreePoint = SpatialIndexStructures.Spatial.Point;

namespace RTreeVisualization
{
    class BearGenerator
    {
        private readonly Random _random;
        private readonly double _regionWidth;
        private readonly double _regionHeight;
        private readonly double _boundWidth;

        public BearGenerator(double regionWidht, double regionHeight, double boundWidth)
        {
            _regionWidth = regionWidht;
            _regionHeight = regionHeight;
            _boundWidth = boundWidth;
            _random = new Random(unchecked((int)DateTime.Now.Ticks));
        }

        public RTreePoint GenerateBearPosition()
        {
            var point = new RTreePoint(_boundWidth + _random.Next() % (_regionWidth - 2 * _boundWidth),
                _boundWidth + _random.Next() % (_regionHeight - 2 * _boundWidth));
            return point;
        }
    }
}
