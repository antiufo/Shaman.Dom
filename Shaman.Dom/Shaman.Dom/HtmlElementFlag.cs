using System;
namespace Shaman.Dom
{
	[Flags]
	public enum HtmlElementFlag
	{
		CData = 1,
		Empty = 2,
		Closed = 4
	}
}
