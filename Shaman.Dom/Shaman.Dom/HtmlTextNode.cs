using System;
namespace Shaman.Dom
{
	public class HtmlTextNode : HtmlNode
	{
		private string _text;
		internal bool _pcdata;
		public string Text
		{
			get
			{
				return this._text ?? string.Empty;
			}
			set
			{
				this._text = value;
			}
		}
		public bool IsPcData
		{
			get
			{
				return this._pcdata;
			}
		}
		internal HtmlTextNode(HtmlDocument ownerdocument, int index) : base(HtmlNodeType.Text, ownerdocument, index)
		{
		}
	}
}
