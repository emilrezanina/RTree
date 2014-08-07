using System;
using System.Linq;
using SpatialIndexStructures.RTree;
using SpatialIndexStructures.Spatial;

namespace RTreeSample
{
    
    class Program
    {
        private static double NextDouble(Random random)
        {
            return random.Next(0, 10);
        }

        static void Main(string[] args)
        {
            //Zadani1();
            //Zadani2();
            //Zadani3();
            Zadani4();
            ISpatialIndex rtree = new RTree(4);
            var random = new Random((int) DateTime.Now.Ticks);
            const int generatedNumberCount = 20;
            var deletedPoint = new Point(0, 0);
            for (var index = 0; index < generatedNumberCount; index++)
            {
                var point = new Point(NextDouble(random), NextDouble(random));
                if (index == 3) deletedPoint = point; 
                rtree.Insert(point);
            }
            rtree.Delete(deletedPoint);
        }

        private static void Zadani4()
        {
            var rTree = new RTree(3);
            var random = new Random();
            //ISpatialData deletedObject = new Point(3, 3);
            ISpatialData deletedObject = new Rectangle(3, 3, 4, 4);
            rTree.Insert(deletedObject);
            for (var index = 0; index < 10; index++)
            {
                rTree.Insert(new Point(random.Next() % 100, random.Next() % 100));
            }
            var countBefore = rTree.SearchRange(new Rectangle(0, 0, 100, 100)).Count();
            rTree.Delete(deletedObject);
            var countAfter = rTree.SearchRange(new Rectangle(0, 0, 100, 100)).Count();
        }

        private static void Zadani3()
        {
            var rTree = new RTree(3);
            var rect = new Rectangle(3, 3, 8, 8);
            var result = rTree.SearchRange(new Rectangle(0, 0, 9, 9));
        }

        private static void Zadani2()
        {
            ISpatialIndex rtree = new RTree(4);
            rtree.Insert(new Point(0, 6));
            rtree.Insert(new Point(0, 0));
            rtree.Insert(new Point(3, 4));
            rtree.Insert(new Point(1, 4));
            rtree.Insert(new Point(0, 4));

            rtree.Insert(new Point(5, 9));
            rtree.Insert(new Point(6, 9));
            rtree.Insert(new Point(6, 8));

            rtree.Insert(new Point(5, 7));
            rtree.Insert(new Point(6, 6));

            rtree.Insert(new Point(5, 2));
            rtree.Insert(new Point(7, 4));
            rtree.Insert(new Point(9, 2));
            rtree.Insert(new Point(9, 5));

            rtree.Insert(new Point(5, 0));
            rtree.Insert(new Point(7, 1));
            rtree.Insert(new Point(6, 1));

            rtree.Insert(new Point(4, 6));
            rtree.Insert(new Point(4, 2));
            rtree.Insert(new Point(4, 4));

            var foundNodes = rtree.Search(new Point(6, 9));
            foundNodes = rtree.Search(new Point(0, 1));
            foundNodes = rtree.SearchRange(new Rectangle(new Point(3, 3), new Point(6, 6)));
        }

        private static void Zadani1()
        {
            ISpatialIndex rtree = new RTree(4);
            rtree.Insert(new Point(4, 1));
            rtree.Insert(new Point(0, 4));
            rtree.Insert(new Point(2, 4));
            rtree.Insert(new Point(0, 4));
            rtree.Insert(new Point(4, 4));
            rtree.Insert(new Point(3, 0));
            rtree.Insert(new Point(2, 3));
            rtree.Insert(new Point(0, 4));
        }
    }
}
