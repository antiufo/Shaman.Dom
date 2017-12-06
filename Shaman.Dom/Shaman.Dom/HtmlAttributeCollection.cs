using Shaman.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Shaman.Dom
{
#if SALTARELLE
    public class HtmlAttributeCollection
#else
    public struct HtmlAttributeCollection 
#endif
        : IEnumerable<HtmlAttribute>, IEnumerable
	{

#if SALTARELLE
        public class Enumerator
#else
        public struct Enumerator
#endif

         : IEnumerator<HtmlAttribute>, IDisposable, IEnumerator
		{
			internal HtmlNode parent;
			internal int index;
			public HtmlAttribute Current
			{
				get
				{
					return this.parent._attributeArray[this.index];
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
				return this.index < (int)this.parent._attributeCount;
			}
			public void Reset()
			{
				throw new NotSupportedException();
			}
		}
		private readonly HtmlNode _ownernode;

        public HtmlAttribute this[int index]
        {
            get
            {
                if (index >= this._ownernode._attributeCount) throw new IndexOutOfRangeException();
                return this._ownernode._attributeArray[index];
            }
        }



        public HtmlAttribute this[string name]
		{
			get
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				string b = name.ToLowerFast();
				HtmlAttribute[] attributeArray = this._ownernode._attributeArray;
				byte attributeCount = this._ownernode._attributeCount;
				for (int i = 0; i < (int)attributeCount; i++)
				{
					HtmlAttribute result = attributeArray[i];
					if (result.Name == b)
					{
						return result;
					}
				}
				return HtmlAttribute.None;
			}
			set
			{
				this.AddInternal(value);
			}
		}
		public int Count
		{
			get
			{
				return (int)this._ownernode._attributeCount;
			}
		}
		internal HtmlAttributeCollection(HtmlNode ownernode)
		{
			this._ownernode = ownernode;
		}
		public HtmlAttributeCollection.Enumerator GetEnumerator()
		{
			return new HtmlAttributeCollection.Enumerator
			{
				parent = this._ownernode,
				index = -1
			};
		}
		IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public void Add(HtmlAttribute newAttribute)
		{
			this.AddInternal(newAttribute);
		}
		public int AddInternal(HtmlAttribute newAttribute)
		{
			int num = this.GetAttributeIndex(newAttribute.Name);
			if (num != -1)
			{
				this._ownernode._attributeArray[num] = newAttribute;
				return num;
			}
			HtmlAttribute[] array = this._ownernode._attributeArray;
			byte attributeCount = this._ownernode._attributeCount;
			if (array == null)
			{
				array = new HtmlAttribute[4];
				this._ownernode._attributeArray = array;
			}
			else
			{
				if ((int)attributeCount == array.Length)
				{
					HtmlAttribute[] array2 = new HtmlAttribute[array.Length * 2];
					for (int i = 0; i < (int)attributeCount; i++)
					{
						array2[i] = array[i];
					}
					this._ownernode._attributeCount = attributeCount;
					this._ownernode._attributeArray = array2;
					array = array2;
				}
			}
			num = (int)this._ownernode._attributeCount;
			array[num] = newAttribute;
			HtmlNode expr_D7 = this._ownernode;
			expr_D7._attributeCount += 1;
			return num;
		}
		public void Add(string name, string value)
		{
			HtmlAttribute newAttribute = this._ownernode._ownerdocument.CreateAttribute(name, value);
			this.AddInternal(newAttribute);
		}
		public void Remove(HtmlAttribute attribute)
		{
			int attributeIndex = this.GetAttributeIndex(attribute);
			if (attributeIndex == -1)
			{
				this.RemoveAt(attributeIndex);
			}
		}
		internal void RemoveAt(int index)
		{
			HtmlAttribute[] attributeArray = this._ownernode._attributeArray;
			HtmlNode expr_14 = this._ownernode;
			byte b = expr_14._attributeCount -= 1;
			for (int i = index; i < (int)b; i++)
			{
				attributeArray[i] = attributeArray[i + 1];
			}
			attributeArray[(int)b] = default(HtmlAttribute);
		}
		private int GetAttributeIndex(HtmlAttribute attribute)
		{
			HtmlAttribute[] attributeArray = this._ownernode._attributeArray;
			if (attributeArray == null)
			{
				return -1;
			}
			for (int i = 0; i < (int)this._ownernode._attributeCount; i++)
			{
				if (attributeArray[i].OriginalName == attribute.OriginalName)
				{
					return i;
				}
			}
			return -1;
		}
		internal int GetAttributeIndex(string name)
		{
			HtmlAttribute[] attributeArray = this._ownernode._attributeArray;
			if (attributeArray == null)
			{
				return -1;
			}
			for (int i = 0; i < (int)this._ownernode._attributeCount; i++)
			{
				if (attributeArray[i].OriginalName.Length == name.Length && attributeArray[i].Name == name)
				{
					return i;
				}
			}
			return -1;
		}
		public void Remove(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			string b = name.ToLowerFast();
			byte attributeCount = this._ownernode._attributeCount;
			HtmlAttribute[] attributeArray = this._ownernode._attributeArray;
			for (int i = 0; i < (int)attributeCount; i++)
			{
				HtmlAttribute htmlAttribute = attributeArray[i];
				if (htmlAttribute.Name == b)
				{
					this.RemoveAt(i);
					return;
				}
			}
		}
	}
}
