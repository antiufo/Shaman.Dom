using Shaman.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Shaman.Dom
{
#if SALTARELLE
    public class HtmlNodeCollection
#else
    public struct HtmlNodeCollection
#endif
        : IList<HtmlNode>, ICollection<HtmlNode>, IEnumerable<HtmlNode>, IEnumerable
	{
#if SALTARELLE
        public class Enumerator
#else
        public struct Enumerator
#endif
            : IEnumerator<HtmlNode>, IDisposable, IEnumerator
		{
			internal HtmlNode parent;
			internal int index;
			public HtmlNode Current
			{
				get
				{
					return this.parent._childNodesArray[this.index];
				}
			}
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
			public void Dispose()
			{
			}
			public bool MoveNext()
			{
				this.index++;
				return this.index < this.parent._childNodesCount;
			}
			public void Reset()
			{
				throw new NotSupportedException();
			}
		}
		private readonly HtmlNode _parentnode;
		internal HtmlNode[] _items
		{
			get
			{
				return this._parentnode._childNodesArray;
			}
		}
		internal int _count
		{
			get
			{
				return this._parentnode._childNodesCount;
			}
		}
		public int this[HtmlNode node]
		{
			get
			{
				int nodeIndex = this.GetNodeIndex(node);
				if (nodeIndex == -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				return nodeIndex;
			}
		}
		public HtmlNode this[string nodeName]
		{
			get
			{
				HtmlNode[] items = this._items;
				if (items == null)
				{
					return null;
				}
				int count = this._count;
				nodeName = nodeName.ToLowerFast();
				for (int i = 0; i < count; i++)
				{
					if (items[i].TagName.Equals(nodeName))
					{
						return items[i];
					}
				}
				return null;
			}
		}
		public int Count
		{
			get
			{
				return this._parentnode._childNodesCount;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		public HtmlNode this[int index]
		{
			get
			{
				return this._items[index];
			}
			set
			{
				this._items[index] = value;
			}
		}
		internal HtmlNodeCollection(HtmlNode parentnode)
		{
			this._parentnode = parentnode;
		}
		public void Add(HtmlNode node)
		{
			this._parentnode.EnsureCapacity(this._parentnode._childNodesCount + 1);
			this._items[this._parentnode._childNodesCount] = node;
			this._parentnode._childNodesCount++;
		}
		public void Clear()
		{
			if (this._items == null)
			{
				return;
			}
			for (int i = 0; i < this._parentnode._childNodesCount; i++)
			{
				HtmlNode htmlNode = this._parentnode._childNodesArray[i];
				htmlNode.ParentNode = null;
				this._parentnode._childNodesArray[i] = null;
			}
			this._parentnode._childNodesCount = 0;
		}
		public bool Contains(HtmlNode item)
		{
			HtmlNode[] items = this._items;
			return items != null && HtmlNodeCollection.IndexOf(items, item, this._parentnode._childNodesCount) != -1;
		}
		public void CopyTo(HtmlNode[] array, int arrayIndex)
		{
			HtmlNode[] items = this._items;
			if (items == null)
			{
				return;
			}
			int count = this._count;
			for (int i = 0; i < count; i++)
			{
				array[arrayIndex] = items[i];
				arrayIndex++;
			}
		}
		public int IndexOf(HtmlNode item)
		{
			HtmlNode[] items = this._items;
			if (items == null)
			{
				return -1;
			}
			return HtmlNodeCollection.IndexOf(items, item, this._parentnode._childNodesCount);
		}
		public void Insert(int index, HtmlNode node)
		{
			this._parentnode.EnsureCapacity(this._parentnode._childNodesCount + 1);
			for (int i = this._parentnode._childNodesCount - 1; i >= index; i--)
			{
				this._items[i]._index = i + 1;
				this._items[i + 1] = this._items[i];
			}
			this._items[index] = node;
			node._index = index;
			this._parentnode._childNodesCount++;
			node._parentnode = this._parentnode;
		}
		public bool Remove(HtmlNode item)
		{
			HtmlNode[] items = this._items;
			if (items == null)
			{
				return false;
			}
			int num = HtmlNodeCollection.IndexOf(items, item, this._parentnode._childNodesCount);
			if (num == -1)
			{
				return false;
			}
			this.RemoveAt(num);
			return true;
		}
		private static int IndexOf(HtmlNode[] _items, HtmlNode item, int p)
		{
			for (int i = 0; i < p; i++)
			{
				if (_items[i] == item)
				{
					return i;
				}
			}
			return -1;
		}
		public void RemoveAt(int index)
		{
			HtmlNode[] items = this._items;
			if (items == null)
			{
				throw new ArgumentOutOfRangeException();
			}
			HtmlNode htmlNode = items[index];
			this._parentnode._childNodesCount--;
			for (int i = index; i < this._parentnode._childNodesCount; i++)
			{
				items[i] = items[i + 1];
				items[i]._index = i;
			}
			items[this._parentnode._childNodesCount] = null;
			htmlNode._parentnode = null;
		}
		public static HtmlNode FindFirst(HtmlNodeCollection items, string name)
		{
			foreach (HtmlNode current in items)
			{
				if (current.TagName.ToLowerFast().Contains(name))
				{
					HtmlNode result = current;
					return result;
				}
				if (current.HasChildNodes)
				{
					HtmlNode htmlNode = HtmlNodeCollection.FindFirst(current.ChildNodes, name);
					if (htmlNode != null)
					{
						HtmlNode result = htmlNode;
						return result;
					}
				}
			}
			return null;
		}
		public void Append(HtmlNode node)
		{
			int count = this._count;
			this._parentnode.EnsureCapacity(count + 1);
			HtmlNode[] items = this._items;
			items[count] = node;
			node._index = count;
			this._parentnode._childNodesCount++;
			node._parentnode = this._parentnode;
		}
		public HtmlNode FindFirst(string name)
		{
			return HtmlNodeCollection.FindFirst(this, name);
		}
		public int GetNodeIndex(HtmlNode node)
		{
			HtmlNode[] items = this._items;
			if (items == null)
			{
				return -1;
			}
			int count = this._count;
			for (int i = 0; i < count; i++)
			{
				if (node == items[i])
				{
					return i;
				}
			}
			return -1;
		}
		public void Prepend(HtmlNode node)
		{
			this.Insert(0, node);
		}
		public bool Remove(int index)
		{
			this.RemoveAt(index);
			return true;
		}
		public void Replace(int index, HtmlNode node)
		{
			HtmlNode[] items = this._items;
			if (items == null)
			{
				throw new ArgumentOutOfRangeException();
			}
			HtmlNode htmlNode = items[index];
			items[index] = node;
			node._index = index;
			node._parentnode = this._parentnode;
			htmlNode._parentnode = null;
		}
		public HtmlNodeCollection.Enumerator GetEnumerator()
		{
			return new HtmlNodeCollection.Enumerator
			{
				parent = this._parentnode,
				index = -1
			};
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		IEnumerator<HtmlNode> IEnumerable<HtmlNode>.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
