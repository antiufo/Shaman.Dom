#if SALTARELLE
using System;
namespace System.IO
{
	public class StringReader : TextReader
	{
		private string data;
		private int index;
		public StringReader(string text)
		{
			this.data = text;
		}
		public override void Close()
		{
			this.data = null;
		}
		public override void Dispose()
		{
			this.Close();
		}
		public override int Peek()
		{
			if (this.index >= this.data.Length)
			{
				return -1;
			}
			return (int)this.data[this.index];
		}
		public override int Read()
		{
			int expr_06 = this.Peek();
			if (expr_06 != -1)
			{
				this.index++;
			}
			return expr_06;
		}
		public override int Read(char[] buffer, int index, int count)
		{
			int length = this.data.Length;
			int num = 0;
			int num2 = index;
			while (num2 < length && num < count)
			{
				buffer[num] = this.data[num2];
				num2++;
				num++;
			}
			return num;
		}
		public override int ReadBlock(char[] buffer, int index, int count)
		{
			return this.Read(buffer, index, count);
		}
		public override string ReadLine()
		{
			if (this.index == this.data.Length)
			{
				return null;
			}
			int num = this.data.IndexOf('\n', this.index);
			if (num == -1)
			{
				string text = (this.data[this.data.Length - 1] == '\r') ? this.data.JsSubstring(this.index, this.data.Length - 1) : this.data;
				this.index = this.data.Length;
				if (text.Length == 0)
				{
					return null;
				}
				return text;
			}
			else
			{
				string text2 = this.data.JsSubstring(this.index, (num != 0 && this.data[num - 1] == '\r') ? (num - 1) : num);
				this.index = num + 1;
				if (text2.Length == 0 && this.index == this.data.Length)
				{
					return null;
				}
				return text2;
			}
		}
		public override string ReadToEnd()
		{
			string arg_32_0 = (this.index == 0) ? this.data : this.data.Substring(this.index);
			this.index = this.data.Length;
			return arg_32_0;
		}
	}
}
#endif