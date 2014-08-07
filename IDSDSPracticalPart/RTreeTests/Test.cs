using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using SpatialIndexStructures.RTree;
using SpatialIndexStructures.Spatial;
using Xunit;
using Xunit.Extensions;

namespace RTreeTests
{
    public class TestClass
    {

        public static IEnumerable<object[]> PointsAreInRectangleData
        {
            get
            {
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(3, 3)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(4, 3)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(1, 3)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(1, 1)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(2, 5)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(5, 5)
                };
            }
        }

        [Theory]
        [PropertyData("PointsAreInRectangleData")]
        private void PointsAreInRectangle(ISpatialData rect, ISpatialData point)
        {
            Assert.Equal(0, rect.Distance(point));
        }

        public static IEnumerable<object[]> PointsArentInRectangleData
        {
            get
            {
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(0, 0)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(6, 6)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(2, 6)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(0, 2)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(6, 3)
                };
                yield return new object[]
                {
                    new Rectangle(new Point(1, 1), new Point(5, 5)), new Point(0, 5)
                };
            }
        }

        [Theory]
        [PropertyData("PointsArentInRectangleData")]
        private void PointsArentInRectangle(ISpatialData rect, ISpatialData point)
        {
            Assert.True(rect.Distance(point) > 0);
        }

        [Fact]
        private void ClosesPointFromRectangle()
        {
            var rect = new Rectangle(new Point(1, 1), new Point(5, 5));
            var dist1 = rect.Distance(new Point(7, 1));
            var dist2 = rect.Distance(new Point(0, 4));
            var dist3 = rect.Distance(new Point(1, 8));
            Assert.True(dist2.CompareTo(dist1) == -1 && dist2.CompareTo(dist3) == -1);
        }

        [Theory]
        [InlineData(0, 1, 0, 2)]
        [InlineData(0, 1, 5, 0)]
        [InlineData(1, 7, 7, 1)]
        private void TwoClosesPointsFromRectangle(double x1, double y1, double x2, double y2)
        {
            var rect = new Rectangle(new Point(1, 1), new Point(5, 5));
            var dist1 = rect.Distance(new Point(x1, y1));
            var dist2 = rect.Distance(new Point(x2, y2));
            Assert.True(dist1.Equals(dist2));
        }


        private static IEnumerable<ISpatialData> spatialData = new ISpatialData[]
        {
            new Point(3, 6),
            new Point(3, 3),
            new Point(6, 6),
            new Point(4, 4)
        };

        [Fact]
        private void AllInseredDataAreInRange()
        {
            var rTree = new RTree(3);
            rTree.Insert(spatialData);
            var result = rTree.SearchRange(new Rectangle(new Point(3, 3), new Point(6, 6)));
            Assert.Equal(spatialData.Count(), result.Count());
        }

        [Fact]
        private void AllInseredDataArentInRange()
        {
            var rTree = new RTree(3);
            rTree.Insert(spatialData);
            var result = rTree.SearchRange(new Rectangle(new Point(0, 3), new Point(2, 6)));
            Assert.Equal(0, result.Count());
        }

        [Fact]
        private void InsertRectangle()
        {
            var rTree = new RTree(3);
            var rect = new Rectangle(2, 2, 3, 3);
            rTree.Insert(rect);
            var result = rTree.Search(rect);
            Assert.Equal(rect, result.First());
        }

        [Fact]
        private void RectangleIsntInRange()
        {
            var rTree = new RTree(3);
            var rect = new Rectangle(3, 3, 8, 8);
            var result = rTree.SearchRange(new Rectangle(3, 3, 6, 6));
            Assert.Equal(0, result.Count());
        }

        [Fact]
        private void RectangleIsInSameRectangleRange()
        {
            var rTree = new RTree(3);
            var rect = new Rectangle(3, 3, 8, 8);
            rTree.Insert(rect);
            var result = rTree.SearchRange(rect);
            Assert.Equal(1, result.Count());
        }

        [Fact]
        private void RectangleIsInRange()
        {
            var rTree = new RTree(3);
            var rect = new Rectangle(3, 3, 8, 8);
            rTree.Insert(rect);
            var result = rTree.SearchRange(new Rectangle(0, 0, 9, 9));
            Assert.Equal(1, result.Count());
        }

        [Fact]
        private void SearchAllDataWithSameCoordinates()
        {
            var rTree = new RTree(3);
            var searchingPoint = new Point(1, 1);
            rTree.Insert(searchingPoint);
            rTree.Insert(searchingPoint);
            rTree.Insert(new Point(4, 1));
            rTree.Insert(searchingPoint);
            rTree.Insert(searchingPoint);
            rTree.Insert(searchingPoint);
            Assert.Equal(5, rTree.Search(searchingPoint).Count());
        }

        public static IEnumerable<Object[]> DeletedObjects
        {
            get
            {
                yield return new object[] { new Point(3, 3) };
                yield return new object[] { new Rectangle(3, 3, 4, 4) };
            }
        }

        [Theory]
        [PropertyData("DeletedObjects")]
        private void SuccessfulDeleteNodeReturnTrue(ISpatialData deletedObject)
        {
            var rTree = new RTree(3);
            var random = new Random();
            rTree.Insert(deletedObject);
            for (var index = 0; index < 10; index++)
            {
                rTree.Insert(new Point(random.Next()% 100, random.Next() % 100));
            }
            Assert.True(rTree.Delete(deletedObject));
        }

        [Theory]
        [PropertyData("DeletedObjects")]
        private void UnsuccessfulDeleteNodeReturnFalse(ISpatialData deletedObject)
        {
            var rTree = new RTree(3);
            var random = new Random();

            for (var index = 0; index < 10; index++)
            {
                rTree.Insert(new Point(random.Next() % 100, random.Next() % 100));
            }
            Assert.False(rTree.Delete(deletedObject));
        }

        [Theory]
        [PropertyData("DeletedObjects")]
        private void SuccessfulDeleteNodeHasLessObjectsCount(ISpatialData deletedObject)
        {
            var rTree = new RTree(3);
            var random = new Random();
            rTree.Insert(deletedObject);
            for (var index = 0; index < 10; index++)
            {
                rTree.Insert(new Point(random.Next() % 100, random.Next() % 100));
            }
            var countBefore = rTree.SearchRange(new Rectangle(0, 0, 100, 100)).Count();
            rTree.Delete(deletedObject);
            var countAfter = rTree.SearchRange(new Rectangle(0, 0, 100, 100)).Count();
            Assert.Equal(countBefore - 1, countAfter);
        }

        [Theory]
        [PropertyData("DeletedObjects")]
        private void UnsuccessfulDeleteNodeHasSameObjectsCount(ISpatialData deletedObject)
        {
            var rTree = new RTree(3);
            var random = new Random();
            for (var index = 0; index < 10; index++)
            {
                rTree.Insert(new Point(random.Next() % 100, random.Next() % 100));
            }
            var countBefore = rTree.SearchRange(new Rectangle(0, 0, 100, 100)).Count();
            rTree.Delete(deletedObject);
            var countAfter = rTree.SearchRange(new Rectangle(0, 0, 100, 100)).Count();
            Assert.Equal(countAfter, countBefore);
        }

        private static double NextDouble(Random random, int decimals)
        {
            const int max = 20;
            const int min = 10;
            return Math.Round(min - random.NextDouble()*(max - min), decimals);
        }

        [Fact]
        private void DeletingWithDoubleNumbers()
        {
            var rTree = new RTree(3);
            var random = new Random();
            var deletedObject = new Point(NextDouble(random, 2), NextDouble(random, 2));
            rTree.Insert(deletedObject);
            for (var index = 0; index < 10; index++)
            {
                rTree.Insert(new Point(NextDouble(random, 2), NextDouble(random, 2)));
            }
            Assert.True(rTree.Delete(deletedObject));
        }
    }
}