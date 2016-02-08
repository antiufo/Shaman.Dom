#if SALTARELLE
using System;
using System.Collections.Generic;
namespace System.Text.Saltarelle
{
	public sealed class StringBuilder
	{
		private List<string> parts = new List<string>();
		private int length;
		public bool IsEmpty
		{
			get
			{
				return this.length == 0;
			}
		}
		public int Length
		{
			get
			{
				return this.length;
			}
			set
			{
				if (value != this.length)
				{
					if (value > this.length)
					{
						throw new ArgumentOutOfRangeException();
					}
					if (value == 0)
					{
						this.Clear();
						return;
					}
					this.Compact();
					string text = this.parts[0].Substring(0, value);
					this.parts[0] = text;
					this.length = value;
				}
			}
		}
		public char this[int index]
		{
			get
			{
				this.Compact();
				return this.parts[0][index];
			}
			set
			{
				this.Compact();
				string text = this.parts[0];
				text = text.Substring(0, index) + value.ToString() + text.Substring(index + 1);
				this.parts[0] = text;
			}
		}
		public StringBuilder()
		{
		}
		public StringBuilder(int initialSize)
		{
		}
		public StringBuilder(string initialText)
		{
			this.Append(initialText);
		}
		public StringBuilder Append(bool b)
		{
			return this.Append(b.ToString());
		}
		public StringBuilder Append(char c)
		{
			return this.Append(c.ToString());
		}
		public StringBuilder Append(int i)
		{
			return this.Append(i.ToString());
		}
		public StringBuilder Append(double d)
		{
			return this.Append(d.ToString());
		}
		public StringBuilder Append(object o)
		{
			if (o != null)
			{
				this.Append(o.ToString());
			}
			return this;
		}
		public StringBuilder Append(string s)
		{
			if (s != null && s.Length != 0)
			{
				this.length += s.Length;
				this.parts.Add(s);
				if (this.parts.Count > 100000)
				{
					this.Compact();
				}
			}
			return this;
		}
		private void Compact()
		{
			if (this.parts.Count > 1)
			{
				string text = string.Join("", this.parts);
				this.parts.Clear();
				this.parts.Add(text);
			}
		}
		public StringBuilder AppendLine()
		{
			return this.Append('\n');
		}
		public StringBuilder AppendLine(bool b)
		{
			return this.Append(b).AppendLine();
		}
		public StringBuilder AppendLine(char c)
		{
			return this.Append(c).AppendLine();
		}
		public StringBuilder AppendLine(int i)
		{
			return this.Append(i).AppendLine();
		}
		public StringBuilder AppendLine(double d)
		{
			return this.Append(d).AppendLine();
		}
		public StringBuilder AppendLine(object o)
		{
			return this.Append(o).AppendLine();
		}
		public StringBuilder AppendLine(string s)
		{
			return this.Append(s).AppendLine();
		}
		public void Clear()
		{
			this.length = 0;
			this.parts.Clear();
		}
		public override string ToString()
		{
			this.Compact();
			if (this.length == 0)
			{
				return "";
			}
			return this.parts[0];
		}
		public void Remove(int start, int count)
		{
			if (count != 0)
			{
				this.Compact();
				if (this.length == 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				string text = this.parts[0];
				this.parts[0] = text.Substring(0, start) + text.Substring(start + count);
				this.length = this.parts[0].Length;
				if (this.length == 0)
				{
					this.Clear();
				}
			}
		}
		internal void Append(string text, int p1, int p2)
		{
			this.Append(text.Substring(p1, p2));
		}
	}
}
#endif