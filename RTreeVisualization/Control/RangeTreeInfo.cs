using System.Windows;
using System.Windows.Shapes;

namespace RTreeVisualization
{
    public class RangeSearchInfo
    {
        private Point _beginPoint;

        public Point BeginPoint
        {
            set { _beginPoint = value; }
            get { return _beginPoint; }
        }

        public Rectangle Rect { get; set; }
    };
}
