#if SALTARELLE
using System;
using System.Text.Saltarelle;
namespace System.IO
{
	public class StringWriter : TextWriter
	{
		private StringBuilder sb = new StringBuilder();
		public override void Write(object value)
		{
			this.sb.Append(value);
		}
		public override void Write(char value)
		{
			this.sb.Append(value);
		}
		public override void Write(char[] buffer)
		{
			this.Write(buffer.Join(""));
		}
		public override void Write(char[] buffer, int index, int count)
		{
			this.Write(ArrayExtensions.Slice<char>(buffer, index, index + count));
		}
		public override void Write(string value)
		{
            this.sb.Append(value);
		}
		public override void Write(string format, params object[] args)
		{
			this.Write(string.Format(format, args));
		}
		public override void WriteLine(object value)
		{
			this.Write(value);
			this.Write("\n");
		}
		public override void WriteLine(char[] buffer)
		{
			this.Write(buffer);
			this.Write("\n");
		}
		public override void WriteLine(char[] buffer, int index, int count)
		{
			this.Write(buffer, index, count);
			this.Write("\n");
		}
		public override void WriteLine(string value)
		{
			this.WriteLine(value);
		}
		public override void WriteLine(string format, params object[] args)
		{
			this.Write(format, args);
			this.Write("\n");
		}
		public override void Dispose()
		{
			this.sb = null;
		}
		public override string ToString()
		{
			return this.sb.ToString();
		}
		public StringBuilder GetStringBuilder()
		{
			return this.sb;
		}
	}
}
#endif