using System;
using System.Collections.Generic;
using System.Linq;

namespace SpatialIndexStructures.Spatial
{
    public class Rectangle : ISpatialData, IArea
    {
        private readonly Point _beginPoint;
        private readonly Point _endPoint;

       
        public Rectangle(Point beginPoint, Point endPoint)
        {
            _beginPoint = beginPoint;
            _endPoint = endPoint;
        }

        public Rectangle(double x1, double y1, double x2, double y2)
            : this(new Point(x1, y1), new Point(x2, y2))
        {

        }
        
        public Rectangle(Rectangle source1, Rectangle source2)
        {
            var coordinatesX = new List<double>()
            {
                source1._beginPoint.X,
                source1._endPoint.X,
                source2._beginPoint.X,
                source2._endPoint.X,
            };
            var coordinatesY = new List<double>()
            {
                source1._beginPoint.y,
                source1._endPoint.y,
                source2._beginPoint.y,
                source2._endPoint.y,
            };
            _beginPoint = new Point(coordinatesX.Min(), coordinatesY.Min());
            _endPoint = new Point(coordinatesX.Max(), coordinatesY.Max());
        }

        public Rectangle(Rectangle source)
        {
            _beginPoint = new Point(source._beginPoint);
            _endPoint = new Point(source._endPoint);
        }

        public Point BeginPoint
        {
            get { return _beginPoint; }
        }

        public Point EndPoint
        {
            get { return _endPoint; }
        }

        public decimal CompareTo(Rectangle second)
        {
            var firstArea = GetArea();
            var secondArea = second.GetArea();
            if (firstArea > secondArea)
                return 1;
            if (firstArea < secondArea)
                return -1;
            return 0;
        }

        public double GetArea()
        {
            return (_endPoint.X - _beginPoint.X) * (_endPoint.y - _beginPoint.y);
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(this);
        }

        public double Distance(ISpatialData geometry)
        {
            return RectDistance(geometry.ToRectangle());
        }
       
        private double RectDistance(Rectangle rect)
        {
            var distanceSquared = 0.0;

            distanceSquared += DimensionDistanceSquared(rect._beginPoint.X, rect._endPoint.X, _beginPoint.X, _endPoint.X);
            distanceSquared += DimensionDistanceSquared(rect._beginPoint.y, rect._endPoint.y, _beginPoint.y, _endPoint.y);
            return (double)Math.Sqrt(distanceSquared);
        }
        
        private static double DimensionDistanceSquared(double minCoordinate, double maxCoordinate, 
            double minCoordinateRect, double maxCoordinateRect)
        {
            double changeDistanceSquared = 0;
            var greatestMin = Math.Max(minCoordinateRect, minCoordinate);
            var leastMax = Math.Min(maxCoordinateRect, maxCoordinate);

            if (greatestMin > leastMax)
            {
                changeDistanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
            }
            return changeDistanceSquared;
        }

        public double GetPossibleExtension(Rectangle rect)
        {
            var enlargedArea = ((Math.Max(_endPoint.X, Math.Max(rect._beginPoint.X, rect._endPoint.X))
                - Math.Min(_beginPoint.X, Math.Min(rect._beginPoint.X, rect._endPoint.X))) *
                (Math.Max(_endPoint.y, Math.Max(rect._beginPoint.y, rect._endPoint.y))
                - Math.Min(_beginPoint.y, Math.Min(rect._beginPoint.y, rect._endPoint.y))));

            return enlargedArea - GetArea();
        }

        public void Extension(Point point)
        {
            _beginPoint.x = Math.Min(_beginPoint.X, point.X);
            _beginPoint.y = Math.Min(_beginPoint.y, point.y);
            _endPoint.x = Math.Max(_endPoint.X, point.X);
            _endPoint.y = Math.Max(_endPoint.y, point.y);
        }

        public void Extension(Rectangle rect)
        {
            Extension(rect._beginPoint);
            Extension(rect._endPoint);
        }

        public bool Equals(ISpatialData other)
        {
            if (!(other is Rectangle)) return false;
            var otherPoint = other as Rectangle;
            return (_beginPoint.Equals(otherPoint._beginPoint) 
                    && _endPoint.Equals(otherPoint._endPoint));
        }

        public override string ToString()
        {
            return _beginPoint.ToString() + "; " + _endPoint.ToString();
        }

        public bool Overlap(Rectangle rect)
        {
            return Distance(rect).Equals(0);
        }

    }
}
