using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RTreeVisualization
{
    public class ShapesController
    {
        public static Point MostTopLeftPoint(params Point[] points)
        {
            var point = new Point
            {
                X = Math.Min(points[0].X, points[1].X),
                Y = Math.Min(points[0].Y, points[1].Y),
            };
            return point;
        }

        public static Rectangle GetDashedRectangle(Point beginPoint, Point endPoint, Style style)
        {
            return new Rectangle
            {
                Width = Math.Abs(beginPoint.X - endPoint.X),
                Height = Math.Abs(beginPoint.Y - endPoint.Y),
                Style = style,
                StrokeDashArray = new DoubleCollection { 2 }
            };
        }

        public static Ellipse CreateCircle(Style style)
        {
            var circle = new Ellipse {Style = style};
            return circle;
        }
    }
}
