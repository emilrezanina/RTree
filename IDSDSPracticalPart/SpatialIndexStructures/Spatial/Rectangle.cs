using System;
using System.Collections.Generic;
using System.Linq;

namespace SpatialIndexStructures.Spatial
{
    public class Rectangle : ISpatialData, IArea
    {
        internal readonly Point BeginPoint;
        internal readonly Point EndPoint;
       
        public Rectangle(Point beginPoint, Point endPoint)
        {
            BeginPoint = beginPoint;
            EndPoint = endPoint;
        }

        public Rectangle(double x1, double y1, double x2, double y2)
            : this(new Point(x1, y1), new Point(x2, y2))
        {

        }
        
        public Rectangle(Rectangle source1, Rectangle source2)
        {
            var coordinatesX = new List<double>()
            {
                source1.BeginPoint.x,
                source1.EndPoint.x,
                source2.BeginPoint.x,
                source2.EndPoint.x,
            };
            var coordinatesY = new List<double>()
            {
                source1.BeginPoint.y,
                source1.EndPoint.y,
                source2.BeginPoint.y,
                source2.EndPoint.y,
            };
            BeginPoint = new Point(coordinatesX.Min(), coordinatesY.Min());
            EndPoint = new Point(coordinatesX.Max(), coordinatesY.Max());
        }

        public Rectangle(Rectangle source)
        {
            BeginPoint = new Point(source.BeginPoint);
            EndPoint = new Point(source.EndPoint);
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
            return (EndPoint.x - BeginPoint.x) * (EndPoint.y - BeginPoint.y);
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

            distanceSquared += DimensionDistanceSquared(rect.BeginPoint.x, rect.EndPoint.x, BeginPoint.x, EndPoint.x);
            distanceSquared += DimensionDistanceSquared(rect.BeginPoint.y, rect.EndPoint.y, BeginPoint.y, EndPoint.y);
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
            var enlargedArea = ((Math.Max(EndPoint.x, Math.Max(rect.BeginPoint.x, rect.EndPoint.x))
                - Math.Min(BeginPoint.x, Math.Min(rect.BeginPoint.x, rect.EndPoint.x))) *
                (Math.Max(EndPoint.y, Math.Max(rect.BeginPoint.y, rect.EndPoint.y))
                - Math.Min(BeginPoint.y, Math.Min(rect.BeginPoint.y, rect.EndPoint.y))));

            return enlargedArea - GetArea();
        }

        public void Extension(Point point)
        {
            BeginPoint.x = Math.Min(BeginPoint.x, point.x);
            BeginPoint.y = Math.Min(BeginPoint.y, point.y);
            EndPoint.x = Math.Max(EndPoint.x, point.x);
            EndPoint.y = Math.Max(EndPoint.y, point.y);
        }

        public void Extension(Rectangle rect)
        {
            Extension(rect.BeginPoint);
            Extension(rect.EndPoint);
        }

        public bool Equals(ISpatialData other)
        {
            if (!(other is Rectangle)) return false;
            var otherPoint = other as Rectangle;
            return (BeginPoint.Equals(otherPoint.BeginPoint) 
                    && EndPoint.Equals(otherPoint.EndPoint));
        }

        public override string ToString()
        {
            return BeginPoint.ToString() + "; " + EndPoint.ToString();
        }

        public bool Overlap(Rectangle rect)
        {
            return Distance(rect).Equals(0);
        }

    }
}
