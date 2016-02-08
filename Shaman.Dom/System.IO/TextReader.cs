#if SALTARELLE
using System;
namespace System.IO
{
	public abstract class TextReader : IDisposable
	{
		public abstract void Close();
		public abstract int Peek();
		public abstract int Read();
		public abstract int Read(char[] buffer, int index, int count);
		public abstract int ReadBlock(char[] buffer, int index, int count);
		public abstract string ReadLine();
		public abstract string ReadToEnd();
		public abstract void Dispose();
	}
}

#endif