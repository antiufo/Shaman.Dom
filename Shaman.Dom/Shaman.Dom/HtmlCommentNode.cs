using System;
namespace Shaman.Dom
{
	public class HtmlCommentNode : HtmlNode
	{
		private string _comment;
		public string Comment
		{
			get
			{
				return this._comment ?? string.Empty;
			}
			set
			{
				this._comment = value;
			}
		}
		internal HtmlCommentNode(HtmlDocument ownerdocument, int index) : base(HtmlNodeType.Comment, ownerdocument, index)
		{
		}
	}
}
