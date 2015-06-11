namespace SpatialIndexStructures.Spatial
{
    public class Point : ISpatialData
    {
        internal double x;
        internal double y;



        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(Point source) : this(source.x, source.y)
        {
        }

        public double X
        {
            get { return x; }
        }

        public double Y
        {
            get { return y; }
        }

        public double GetArea()
        {
            return 0;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(this, this);
        }

        public double Distance(ISpatialData geometry)
        {
            return ToRectangle().Distance(geometry);
        }

        public bool Equals(ISpatialData other)
        {
            if (!(other is Point)) return false;
            var otherPoint = other as Point;
            return otherPoint.x.Equals(x) && otherPoint.y.Equals(y);
        }

        public override string ToString()
        {
            return "x = " + x + "; y = " + y;
        }
    }
}