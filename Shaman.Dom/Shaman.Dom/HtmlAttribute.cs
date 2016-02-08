using Shaman.Runtime;
using System;
using System.Diagnostics;
namespace Shaman.Dom
{
	[DebuggerDisplay("Name: {OriginalName}, Value: {Value}")]
#if SALTARELLE
    public class HtmlAttribute
#else
    public struct HtmlAttribute
#endif
    {
		internal const AttributeValueQuote MustNormalizeName = (AttributeValueQuote)2;
		internal string _name;
		internal AttributeValueQuote _quoteType;
		internal string _value;
		public static readonly HtmlAttribute None = default(HtmlAttribute);
		public string Name
		{
			get
			{
				if (this._name == null)
				{
					return null;
				}
				if ((byte)(this._quoteType & (AttributeValueQuote)2) != 0)
				{
					return this._name.ToLowerFast();
				}
				return this._name;
			}
		}
		public string OriginalName
		{
			get
			{
				return this._name;
			}
		}
		internal AttributeValueQuote QuoteType
		{
			get
			{
				return this._quoteType & AttributeValueQuote.SingleQuote;
			}
		}
		public string Value
		{
			get
			{
				return this._value ?? string.Empty;
			}
			set
			{
				this._value = value;
			}
		}
		internal string XmlName
		{
			get
			{
				return HtmlDocument.GetXmlName(this.Name);
			}
		}
		internal string XmlValue
		{
			get
			{
				return this.Value;
			}
		}
	}
}
