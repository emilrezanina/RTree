using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SpatialIndexStructures.Spatial;

namespace SpatialIndexStructures.RTree
{
    public class RTree : ISpatialIndex
    {
        private readonly int _maxChildrenOfNode;
        private readonly int _minChildrenOfNode;
        private ControlNode _root;
        private const int LeafLevel = 0;

        public RTree(int maxChildrenOfNode)
        {
            _maxChildrenOfNode = maxChildrenOfNode;
            _minChildrenOfNode = maxChildrenOfNode / 2;
            _root = new ControlNode(LeafLevel, _maxChildrenOfNode);
        }

        public void Insert(ISpatialData geometry)
        {
            var entry = new Entry(geometry, _maxChildrenOfNode);
            ControlNode secondNode = null;
            var actualNode = ChooseLeaf(entry);
            if (!actualNode.IsFull())
            {
                actualNode.AddEntry(entry);
            }
            else
            {
                secondNode = new ControlNode(actualNode.Level, _maxChildrenOfNode);
                if (actualNode.Parent == null)
                {
                    var parentLevel = actualNode.Level + 1;
                    var parent = new ControlNode(parentLevel, _maxChildrenOfNode);
                    parent.Add(actualNode);
                    actualNode.Parent = parent;
                    parent.CreateMbr();
                    _root = parent;
                }
                secondNode.Parent = actualNode.Parent;
                actualNode.Parent.Add(secondNode);
                actualNode.AddEntry(entry);
                Split(actualNode, secondNode);
            }
            AdjustTree(actualNode, secondNode);
        }

        public void Insert(IEnumerable<ISpatialData> geometries)
        {
            foreach (var spatialData in geometries)
            {
                Insert(spatialData);
            }
        }

        private ControlNode ChooseLeaf(Entry entry)
        {
            var actualNode = _root;
            while (!actualNode.IsLeaf())
            {
                var smallestExtension = new KeyValuePair<double, ControlNode>(double.MaxValue, null);
                for (var index = 0; index < actualNode.ChildrenCount; index++)
                {
                    var actualChild = actualNode.GetChild(index);
                    var nodeExtension = actualChild.Mbr.Distance(entry.Geometry.ToRectangle());
                    if (nodeExtension < smallestExtension.Key
                        || (nodeExtension.Equals(smallestExtension.Key) && !actualChild.HasGreaterMbr(smallestExtension.Value)))
                        smallestExtension = new KeyValuePair<double, ControlNode>(nodeExtension, actualChild);
                }
                actualNode = smallestExtension.Value;
            }
            return actualNode;
        }

        private void Split(ControlNode dividedNode, ControlNode nnNode)
        {
            if (dividedNode.IsLeaf())
                Split(dividedNode.Entries, nnNode.Entries);
            else
            {
                Split(dividedNode.Children, nnNode.Children);
                SetParentOnChildren(dividedNode);
                SetParentOnChildren(nnNode);
            }
        }

        private static void SetParentOnChildren(ControlNode dividedNode)
        {
            foreach (var controlNode in dividedNode.Children)
            {
                controlNode.Parent = dividedNode;
            }
        }

        private void Split<T>(ICollection<T> dividedNodeData, ICollection<T> nnNodeData) where T : INode
        {
            var nonReorganizedData = new List<T>(dividedNodeData);
            dividedNodeData.Clear();
            PickSeed(nonReorganizedData, dividedNodeData, nnNodeData);
            var dividedNodeDataMbr = new Rectangle(dividedNodeData.First().GetGeometry().ToRectangle());
            var nnNodeDataMbr = new Rectangle(nnNodeData.First().GetGeometry().ToRectangle());
            var dividedNodeDataMbrPair = new KeyValuePair<ICollection<T>, Rectangle>(dividedNodeData, dividedNodeDataMbr);
            var nnNodeDataMbrPair = new KeyValuePair<ICollection<T>, Rectangle>(nnNodeData, nnNodeDataMbr);
            while (nonReorganizedData.Count != 0)
            {
                if (TryInsertRecords(nonReorganizedData, dividedNodeData)
                    || TryInsertRecords(nonReorganizedData, nnNodeData))
                    break;
                PickNext(nonReorganizedData, dividedNodeDataMbrPair, nnNodeDataMbrPair);
            }
        }

        private static void PickSeed<T>(IList<T> source, ICollection<T> firstNodeData,
            ICollection<T> secondNodeData) where T : INode
        {
            var maxDifferenceTuple = new Tuple<double, int, int>(double.MinValue, 0, 0);
            for (var index1 = 0; index1 < source.Count; index1++)
                for (var index2 = 0; index2 < source.Count; index2++)
                    if (index1 != index2)
                    {
                        var geometry1 = source[index1].GetGeometry();
                        var geometry2 = source[index2].GetGeometry();
                        var pairRect = new Rectangle(geometry1.ToRectangle(), geometry2.ToRectangle());
                        var difference = pairRect.GetArea() - geometry1.GetArea() - geometry2.GetArea();
                        if (maxDifferenceTuple.Item1 < difference)
                        {
                            maxDifferenceTuple = new Tuple<double, int, int>(difference, index1, index2);
                        }
                    }
            firstNodeData.Add(source[maxDifferenceTuple.Item2]);
            secondNodeData.Add((source[maxDifferenceTuple.Item3]));
            source.RemoveAt(Math.Max(maxDifferenceTuple.Item2, maxDifferenceTuple.Item3));
            source.RemoveAt(Math.Min(maxDifferenceTuple.Item2, maxDifferenceTuple.Item3));
        }

        private bool TryInsertRecords<T>(ICollection<T> children, ICollection<T> nodeData)
            where T : INode
        {
            if (children.Count != (_minChildrenOfNode - nodeData.Count)) return false;
            foreach (var leaf in children)
            {
                nodeData.Add(leaf);
            }
            children.Clear();
            return true;
        }

        private static void PickNext<T>(IList<T> children, KeyValuePair<ICollection<T>,
            Rectangle> firstDataMbrPair, KeyValuePair<ICollection<T>, Rectangle> secondDataMbrPair) where T : INode
        {
            var dataMbrPairs = new List<KeyValuePair<ICollection<T>, Rectangle>> { firstDataMbrPair, secondDataMbrPair };
            var maxDifferenceTuple = new Tuple<KeyValuePair<ICollection<T>, Rectangle>, int, double, double>
                (firstDataMbrPair, 0, double.MaxValue, double.MaxValue);
            for (var index = 0; index < children.Count; index++)
            {
                foreach (var pair in dataMbrPairs)
                {
                    var differenceDistance = pair.Value.Distance(children[index].GetGeometry().ToRectangle());
                    var differenceExtension = pair.Value.GetPossibleExtension(children[index].GetGeometry().ToRectangle());
                    //pokud je distance stejna, tak se jeste porovna i rozsireni MBR
                    //pokud i v MBR je shoda, provede se porovnani poctu prvku v kolekcich
                    if (maxDifferenceTuple.Item3 > differenceDistance ||
                        (maxDifferenceTuple.Item3.Equals(differenceDistance) &&
                            (maxDifferenceTuple.Item4 > differenceExtension ||
                            (maxDifferenceTuple.Item4.Equals(differenceExtension) &&
                                maxDifferenceTuple.Item1.Key.Count > pair.Key.Count))))
                    {
                        maxDifferenceTuple = new Tuple<KeyValuePair<ICollection<T>, Rectangle>, int, double, double>
                        (pair, index, differenceDistance, differenceExtension);
                    }
                }
            }
            var chooseNode = children[maxDifferenceTuple.Item2];
            maxDifferenceTuple.Item1.Key.Add(chooseNode);
            maxDifferenceTuple.Item1.Value.Extension(chooseNode.GetGeometry().ToRectangle());
            children.RemoveAt(maxDifferenceTuple.Item2);
        }

        private void AdjustTree(ControlNode actualNode, ControlNode nnNode)
        {
            actualNode.CreateMbr();
            if (nnNode != null) nnNode.CreateMbr();

            var lastChanged = true;
            var parent = actualNode.Parent;
            while (parent != null && lastChanged)
            {
                lastChanged = false;

                if (parent.IsOverflow())
                {
                    var nnParent = new ControlNode(parent.Level, _maxChildrenOfNode);
                    if (parent.Parent == null)
                    {
                        var parentLevel = parent.Level + 1;
                        var parentParent = new ControlNode(parentLevel, _maxChildrenOfNode);
                        parentParent.Add(parent);
                        parent.Parent = parentParent;
                        _root = parentParent;
                    }
                    parent.Parent.Add(nnParent);
                    nnParent.Parent = parent.Parent;
                    Split(parent, nnParent);
                    parent.CreateMbr();
                    nnParent.CreateMbr();
                    parent.Parent.CreateMbr();
                    lastChanged = true;
                }
                else
                {
                    var changedMbr = nnNode == null ? new Rectangle(actualNode.Mbr) :
                        new Rectangle(actualNode.Mbr, nnNode.Mbr);
                    if (parent.Mbr.GetPossibleExtension(changedMbr) > 0)
                    {
                        parent.MbrExtension(changedMbr);
                        lastChanged = true;
                    }
                }

                parent = parent.Parent;
            }
        }

        public bool Delete(ISpatialData geometry)
        {
            var entry = new Entry(geometry, _maxChildrenOfNode);
            var leafWithDeletingGeometry = FindLeaf(entry);
            if (leafWithDeletingGeometry == null) return false;
            leafWithDeletingGeometry.RemoveEntry(entry);
            Condense(leafWithDeletingGeometry);
            return true;
        }

        private ControlNode FindLeaf(Entry entry)
        {
            var leafWithDeletingGeometry = ChooseLeaf(entry);
            return leafWithDeletingGeometry.Contain(entry) ? leafWithDeletingGeometry : null;
        }

        private void Condense(ControlNode leafWithDeletingGeometry)
        {
            var eliminatedNodes = new List<ControlNode>();
            var actualNode = leafWithDeletingGeometry;
            while (actualNode != _root)
            {
                var parent = actualNode.Parent;
                if (actualNode.IsUnderflow())
                {
                    parent.Children.Remove(actualNode);
                    eliminatedNodes.Add(actualNode);
                }
                else
                {
                    actualNode.CreateMbr();
                }
                actualNode = parent;
            }
            var reinsertData = GetSpatialData(eliminatedNodes);
            Insert(reinsertData);
        }

        private static IEnumerable<ISpatialData> GetSpatialData(List<ControlNode> sourceData)
        {
            IList<ControlNode> usedNodes = new List<ControlNode>();
            ICollection<ISpatialData> entities = new Collection<ISpatialData>();
            while (sourceData.Count != 0)
            {
                var controlNode = sourceData.First();
                if (!usedNodes.Contains(controlNode))
                {
                    if (!controlNode.IsLeaf())
                        sourceData.AddRange(controlNode.Children);
                    else
                    {
                        foreach (var entry in controlNode.Entries)
                            entities.Add(entry.Geometry);
                    }
                }
                sourceData.Remove(controlNode);
                usedNodes.Add(controlNode);
            }
            return entities;
        }

        public IEnumerable<ISpatialData> Search(ISpatialData geometry)
        {
            return SearchRange(geometry.ToRectangle());
        }

        public IEnumerable<ISpatialData> SearchRange(IArea area)
        {
            ICollection<ISpatialData> data = new Collection<ISpatialData>();
            Search(_root, area, data);
            return data;
        }

        private static void Search(ControlNode node, IArea area, ICollection<ISpatialData> foundData)
        {
            if (!node.IsLeaf())
            {
                foreach (var child in node.Children.Where(child => area.Overlap(child.Mbr)))
                {
                    Search(child, area, foundData);
                }
            }
            else
            {
                foreach (var entry in node.Entries.Where(entry => area.Overlap(new Rectangle(entry.Geometry.ToRectangle()))))
                {
                    foundData.Add(entry.Geometry);
                }
            }
        }
    }
}
