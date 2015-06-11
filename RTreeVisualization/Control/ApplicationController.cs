using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SpatialIndexStructures.RTree;
using SpatialIndexStructures.Spatial;
using Icone = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using RTreePoint = SpatialIndexStructures.Spatial.Point;
using Size = System.Windows.Size;

namespace RTreeVisualization
{
    internal class ApplicationController
    {
        private RTree _rTree;
        private readonly BearGenerator _bearGenerator;
        private int _geometriesCount;

        public ApplicationController(double maxWidth, double maxHeight)
        {
            _rTree = new RTree(20);
            _bearGenerator = new BearGenerator(maxWidth, maxHeight, 10);
            _geometriesCount = 0;
        }

        public UIElement[] PreviousPaint { get; set; }


        public void Reset()
        {
            _rTree = new RTree(4);
            _geometriesCount = 0;
        }

        public RTreePoint[] FindNewBears(int count)
        {
            var bears = new RTreePoint[count];
            for (var i = 0; i < count; i++)
            {
                var bear = _bearGenerator.GenerateBearPosition();
                Insert(bear);
                bears[i] = bear;
            }
            return bears;
        }

        public void InsertBear(Point currentPoint)
        {
            Insert(new RTreePoint(currentPoint.X, currentPoint.Y));
        }

        private void Insert(ISpatialData point)
        {
            if (point == null) throw new ArgumentNullException("point");
            _rTree.Insert(point);
            _geometriesCount++;
        }

        public IEnumerable<ControlNode> GetRegions()
        {
            return _rTree.GetAllControlNodes();
        }

        public int GetBearsCount()
        {
            return _geometriesCount;
        }

        public void RecordPreviousState(Canvas rTreeCanvas)
        {
            var collection = new UIElement[rTreeCanvas.Children.Count];
            rTreeCanvas.Children.CopyTo(collection, 0);
            PreviousPaint = collection;
        }

        public void ExportToPng(Uri path, Canvas surface)
        {
            if (path == null) return;

            // Save current canvas transform
            var transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            var size = new Size(surface.Width, surface.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            var renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (var outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                // Use png encoder for our data
                var encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }

        public IEnumerable<ISpatialData> FindEntriesInRange(RangeSearchInfo rangeSearchInfo)
        {
            var beginRTreePoint = new RTreePoint(rangeSearchInfo.BeginPoint.X, rangeSearchInfo.BeginPoint.Y);
            var endRTreePoint = new RTreePoint(rangeSearchInfo.BeginPoint.X + rangeSearchInfo.Rect.Width,
                rangeSearchInfo.BeginPoint.Y + rangeSearchInfo.Rect.Height);
            var area = new Rectangle(beginRTreePoint, endRTreePoint);

            return _rTree.SearchRange(area);
        }

        public bool FillTreeViewByRTree(TreeView rTreeView)
        {
            var root = _rTree.GetRoot();
            if (root.IsEmpty()) return false;
            var rootViewItem = new TreeViewItem
            {
                Header = "Root; Level = " + root.Level + "; MBR: [" + root.Mbr.BeginPoint.X + ", " +
                         root.Mbr.BeginPoint.Y + "][" +
                         root.Mbr.EndPoint.X + ", " + root.Mbr.EndPoint.Y + "]"
            };
            rTreeView.Items.Add(rootViewItem);
            AddChildrenToTreeView(rootViewItem, root);
            return true;
        }

        private static void AddChildrenToTreeView(ItemsControl parentViewItem, ControlNode node)
        {
            TreeViewItem nodeViewItem;
            if (node.IsLeaf())
            {
                foreach (var entry in node.Entries)
                {
                    nodeViewItem = new TreeViewItem
                    {
                        Header = "Entry; [" + entry.GetGeometry() + ", " +
                                 entry.GetGeometry() + "]",
                    };
                    parentViewItem.Items.Add(nodeViewItem);
                }
            }
            else
            {
                foreach (var controlNode in node.Children)
                {
                    nodeViewItem = new TreeViewItem
                    {
                        Header = "Control Node; Level = " + controlNode.Level + "; MBR: [" + controlNode.Mbr.BeginPoint.X + ", " +
                                 controlNode.Mbr.BeginPoint.Y + "][" +
                                 controlNode.Mbr.EndPoint.X + ", " + controlNode.Mbr.EndPoint.Y + "]"
                    };
                    parentViewItem.Items.Add(nodeViewItem);
                    AddChildrenToTreeView(nodeViewItem, controlNode);
                }
            }
        }
    }
}
