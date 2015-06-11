using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SpatialIndexStructures.RTree;
using Point = System.Windows.Point;
using RTreePoint = SpatialIndexStructures.Spatial.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace RTreeVisualization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ApplicationController _appControler;
        private bool _isChanged;
        private int _zIndex;
        private readonly RangeSearchInfo _rangeSearchInfo;

        public MainWindow()
        {
            InitializeComponent();
            _appControler = new ApplicationController(RTreeCanvas.Width, RTreeCanvas.Height);
            _isChanged = false;
            _zIndex = 0;
            _rangeSearchInfo = new RangeSearchInfo();
        }

        private void ResetMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _appControler.Reset();
            GeometriesCounTextBox.Text = "0";
            RTreeCanvas.Children.Clear();
            SetPaintMenuItemEnabling(false);
            _zIndex = 0;
        }

        private void GenerateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FindNewBears(1);
            RemoveRangeSearchRectangleFromCanvas();
        }

        private void GenerateCountItem_OnClick(object sender, RoutedEventArgs e)
        {
            FindNewBears(20);
            RemoveRangeSearchRectangleFromCanvas();
        }

        private void RTreeCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return;
            var currentPoint = e.GetPosition(RTreeCanvas);
            _appControler.InsertBear(currentPoint);
            PaintFoundBear(currentPoint);
            GeometriesCounTextBox.Text = _appControler.GetBearsCount().ToString(ContentStringFormat);
            SetPaintMenuItemEnabling(true);
            RemoveRangeSearchRectangleFromCanvas();
        }

        private void CurrentPaintMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveRangeSearchRectangleFromCanvas();
            if (_isChanged)
            {
                _zIndex = 0;
                _appControler.RecordPreviousState(RTreeCanvas);
            }

            RTreeCanvas.Children.Clear();
            var nodes = _appControler.GetRegions();
            foreach (var node in nodes)
            {
                ShowRegionsOnMap(node);
                if (!node.IsLeaf()) continue;
                foreach (var bear in node.Entries)
                {
                    ShowBearOnMap(bear);
                }
            }
            _isChanged = false;
        }

        private void PreviousPaintMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (_appControler.PreviousPaint == null) return;
            PaintCollectionFromHistory();
        }

        private void RTreeCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released || _rangeSearchInfo.Rect == null)
                return;

            var endPoint = e.GetPosition(RTreeCanvas);

            var mostLeft = Math.Min(endPoint.X, _rangeSearchInfo.BeginPoint.X);
            var mostTop = Math.Min(endPoint.Y, _rangeSearchInfo.BeginPoint.Y);

            var width = Math.Max(endPoint.X, _rangeSearchInfo.BeginPoint.X) - mostLeft;
            var height = Math.Max(endPoint.Y, _rangeSearchInfo.BeginPoint.Y) - mostTop;

            _rangeSearchInfo.Rect.Width = width;
            _rangeSearchInfo.Rect.Height = height;

            ShiftOnCanvas(_rangeSearchInfo.Rect, new Point(mostLeft, mostTop));
        }

        private void FindNewBears(int count)
        {
            var newBears = _appControler.FindNewBears(count);
            foreach (var bear in newBears)
            {
                PaintFoundBear(new Point(bear.X, bear.Y));
            }
            GeometriesCounTextBox.Text = _appControler.GetBearsCount().ToString(ContentStringFormat);
            SetPaintMenuItemEnabling(true);
        }

        private void PaintFoundBear(Point point)
        {
            var newPointStyle = FindResource("NewPointStyle") as Style;
            var circle = ShapesController.CreateCircle(newPointStyle);
            circle.ToolTip = CreatePointToolTip(point);
            ShiftOnCanvas(circle, point);
            RTreeCanvas.Children.Insert(0, circle);
        }

        private void ShiftOnCanvas(UIElement element, Point startPoint)
        {
            Canvas.SetTop(element, startPoint.Y);
            Canvas.SetLeft(element, startPoint.X);
            Panel.SetZIndex(element, _zIndex++);
        }

        private void SetPaintMenuItemEnabling(bool value)
        {
            CurrentPaintMenuItem.IsEnabled = value;
            PreviousPaintMenuItem.IsEnabled = value;
            _isChanged = value;
        }

        private void ShowRegionsOnMap(ControlNode node)
        {
            var geometry = node.Mbr;
            var beginPoint = new Point(geometry.BeginPoint.X, geometry.BeginPoint.Y);
            var endPoint = new Point(geometry.EndPoint.X, geometry.EndPoint.Y);
            var regionStyle = FindResource("RegionStyle") as Style;
            var rect = ShapesController.GetDashedRectangle(beginPoint, endPoint, regionStyle);
            rect.ToolTip = CreateControlNodeToolTip(node);
            ShiftOnCanvas(rect, ShapesController.MostTopLeftPoint(beginPoint, endPoint));
            RTreeCanvas.Children.Insert(0, rect);
        }


        private void ShowBearOnMap(Entry entity)
        {
            var geometry = entity.GetGeometry();
            if (geometry is RTreePoint)
            {
                var entityStyle = FindResource("EntityStyle") as Style;
                var circle = ShapesController.CreateCircle(entityStyle);
                var point = geometry as RTreePoint;
                circle.ToolTip = CreateEntryToolTip(entity);
                var circleCenter = new Point(point.X, point.Y);
                circleCenter.X = circleCenter.X - circle.Width/2;
                circleCenter.Y = circleCenter.Y - circle.Height/2;
                ShiftOnCanvas(circle, circleCenter);
                RTreeCanvas.Children.Insert(0, circle);
            }
        }

        private static object CreateControlNodeToolTip(ControlNode node)
        {
            var stackPanel = new StackPanel();
            var beginPoint = node.Mbr.BeginPoint;
            var endPoint = node.Mbr.EndPoint;
            var nodeTitleTextBlock = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                Text = node.IsLeaf() ? "Leaf" : "Control Node"
            };
            var nodeInfoTextBlock = new TextBlock
            {
                Text = (node.IsLeaf()
                    ? "Entries number: " + node.EntriesCount
                    : "Children number: " + node.ChildrenCount) + "\n" +
                       "MBR: [" + beginPoint.X + ", " + beginPoint.Y + "][" +
                       endPoint.X + ", " + endPoint.Y + "]"
            };
            stackPanel.Children.Add(nodeTitleTextBlock);
            stackPanel.Children.Add(nodeInfoTextBlock);
            return stackPanel;
        }

        private static object CreateEntryToolTip(Entry entry)
        {
            var stackPanel = new StackPanel();
            var entityTitleTextBlock = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                Text = "Entry"
            };
            var entityNameTextBlock = new TextBlock
            {
                Text = entry.GetGeometry().ToString()
            };
            stackPanel.Children.Add(entityTitleTextBlock);
            stackPanel.Children.Add(entityNameTextBlock);
            return stackPanel;
        }

        private static object CreatePointToolTip(Point point)
        {
            var stackPanel = new StackPanel();
            var wrapPanel = new WrapPanel();
            var pointTextBlock = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                Text = "Polar Bear"
            };

            wrapPanel.Children.Add(pointTextBlock);
            var entityNameTextBlock = new TextBlock
            {
                Text = "x = " + point.X + ", y = " + point.Y
            };
            stackPanel.Children.Add(wrapPanel);
            stackPanel.Children.Add(entityNameTextBlock);
            return stackPanel;
        }

        private void PaintCollectionFromHistory()
        {
            RTreeCanvas.Children.Clear();
            foreach (var element in _appControler.PreviousPaint)
            {
                RTreeCanvas.Children.Add(element);
            }
        }

        private void SaveCanvasMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog {FileName = "polarBears", DefaultExt = ".png", Filter = "PNG | *.png"};
            var result = dlg.ShowDialog();
            if (result != true) return;
            var uri = new Uri(dlg.FileName, UriKind.Absolute);
            _appControler.ExportToPng(uri, RTreeCanvas);
        }

        private void RTreeCanvas_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_rangeSearchInfo.Rect != null)
            {
                RTreeCanvas.Children.Remove(_rangeSearchInfo.Rect);
            }
            _rangeSearchInfo.BeginPoint = e.GetPosition(RTreeCanvas);

            _rangeSearchInfo.Rect = new Rectangle {Style = FindResource("RangeSearchRectangleStyle") as Style};
            ShiftOnCanvas(_rangeSearchInfo.Rect, _rangeSearchInfo.BeginPoint);
            RTreeCanvas.Children.Add(_rangeSearchInfo.Rect);
        }

        private void RemoveRangeSearchRectangleFromCanvas()
        {
            if (_rangeSearchInfo.Rect == null)
                return;
            RTreeCanvas.Children.Remove(_rangeSearchInfo.Rect);
            _rangeSearchInfo.Rect = null;
        }

        private void RangeSearchMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (_rangeSearchInfo.Rect == null) return;
            var entries = _appControler.FindEntriesInRange(_rangeSearchInfo);
            var title = "Range [" + _rangeSearchInfo.BeginPoint.X + ", " + _rangeSearchInfo.BeginPoint.Y + "]" +
                           "[" + (_rangeSearchInfo.BeginPoint.X + _rangeSearchInfo.Rect.Width) + ", " +
                           (_rangeSearchInfo.BeginPoint.Y + _rangeSearchInfo.Rect.Height) + "] contains:\n";
            var entriesString = entries.Aggregate(title, (current, spatialData) => current + ("Entry: " + spatialData.ToString() + "\n"));
            MessageBox.Show(entriesString);
        }

        private void ShowStructureMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var structureWindow = new StructureWindow();
            var rTreeView = structureWindow.RTreeView;
            if(_appControler.FillTreeViewByRTree(rTreeView))
                structureWindow.ShowDialog();
        }

        private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}
