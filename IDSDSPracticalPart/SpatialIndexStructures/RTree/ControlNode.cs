using System.Collections.Generic;
using System.Linq;
using SpatialIndexStructures.Spatial;

namespace SpatialIndexStructures.RTree
{
    class ControlNode : Node
    {
        public List<Entry> Entries { get; set; }
        public List<ControlNode> Children { get; set; }
        public int Level { get; set; }
        public Rectangle Mbr { get; private set; }
        public ControlNode Parent { get; set; }
        public ControlNode(int level, int maxCapacity) : base(maxCapacity)
        {
            Level = level;
            Entries = new List<Entry>();
            Children = new List<ControlNode>();
            Mbr = null;
            Parent = null;
        }

        public bool IsLeaf()
        {
            return Level == 0;
        }

        public int ChildrenCount { get { return Children.Count; } }
        public int EntriesCount { get { return Entries.Count;  } }
        public ControlNode GetChild(int index)
        {
            return Children[index];
        }

        public bool IsFull()
        {
            return MaxCapacity == (IsLeaf() ? Entries.Count : Children.Count);
        }

        public bool IsOverflow()
        {
            return MaxCapacity < (IsLeaf() ? EntriesCount : ChildrenCount);
        }
        public bool IsUnderflow()
        {
            return MinCapacity > (IsLeaf() ? EntriesCount : ChildrenCount);
        }
        public void AddEntry(Entry entry)
        {
            Entries.Add(entry);
        }

        public void Add(ControlNode node)
        {
            Children.Add(node);
        }

        public override ISpatialData GetGeometry()
        {
            return Mbr;
        }

        public void Clear()
        {
            Entries.Clear();
            Children.Clear();
        }

        public bool HasGreaterMbr(ControlNode second)
        {
            return Mbr.CompareTo(second.Mbr) > 0;
        }

        public void MbrExtension(Rectangle geometry)
        {
            Mbr.Extension(geometry);
        }

        public void CreateMbr()
        {
            Mbr = null;
            ICollection<INode> collection = IsLeaf() ? Entries.ToList<INode>() : Children.ToList<INode>();
            foreach (var node in collection)
            {
                if(Mbr == null) Mbr = new Rectangle(node.GetGeometry().ToRectangle(), node.GetGeometry().ToRectangle());
                    Mbr.Extension(node.GetGeometry().ToRectangle());    
            }
        }

        public override string ToString()
        {
            return Mbr.ToString();
        }

        public bool Contain(Entry searchedEntry)
        {
            return Entries.Any(entry => entry.GetGeometry().Equals(searchedEntry.GetGeometry()));
        }

        public void RemoveEntry(Entry entry)
        {
            Entries.Remove(entry);
        }

    }
}
