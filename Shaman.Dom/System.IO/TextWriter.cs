#if SALTARELLE
using System;
using System.Runtime.CompilerServices;
namespace System.IO
{
	public abstract class TextWriter : IDisposable
	{
		public abstract void Write(object value);
		public abstract void Write(char value);
		public abstract void Write(char[] buffer);
		public abstract void Write(char[] buffer, int index, int count);
		public abstract void Write(string value);
		public abstract void Write(string format, params object[] args);
		public abstract void WriteLine(object value);
		public abstract void WriteLine(char[] buffer);
		public abstract void WriteLine(char[] buffer, int index, int count);
		public abstract void WriteLine(string value);
		public abstract void WriteLine(string format, params object[] args);
		public abstract void Dispose();
		[InlineCode("void 0")]
		public void Flush()
		{
		}
	}
}
#endif