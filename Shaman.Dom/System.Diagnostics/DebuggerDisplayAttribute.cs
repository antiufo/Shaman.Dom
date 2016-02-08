#if SALTARELLE
using System;
namespace System.Diagnostics
{
	public class DebuggerDisplayAttribute : Attribute
	{
		public DebuggerDisplayAttribute(string text)
		{
		}
	}
}
#endif