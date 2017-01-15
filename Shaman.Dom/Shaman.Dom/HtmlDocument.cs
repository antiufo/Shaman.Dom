using Shaman.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

#if SALTARELLE
using StringToHtmlNodeDictionary = System.Collections.Generic.JsDictionary<string, Shaman.Dom.HtmlNode>;
using IntToHtmlNodeDictionary = System.Collections.Generic.JsDictionary<int, Shaman.Dom.HtmlNode>;
#else
using StringToHtmlNodeDictionary = System.Collections.Generic.Dictionary<string, Shaman.Dom.HtmlNode>;
using IntToHtmlNodeDictionary = System.Collections.Generic.Dictionary<int, Shaman.Dom.HtmlNode>;
#endif
namespace Shaman.Dom
{
    public class HtmlDocument
    {
        private enum ParseState
        {
            Text,
            WhichTag,
            Tag,
            BetweenAttributes,
            EmptyTag,
            AttributeName,
            AttributeBeforeEquals,
            AttributeAfterEquals,
            AttributeValue,
            Comment,
            QuotedAttributeValue,
            PcData
        }
        private int _c;
        private int _currentAttributeIndex = -1;
        private HtmlNode _currentnode;
#if !SALTARELLE
        private Encoding _declaredencoding;
#endif
        private HtmlNode _documentnode;
        private bool _fullcomment;
        private bool _pcdata;
        private int _index;
        internal StringToHtmlNodeDictionary Lastnodes = new StringToHtmlNodeDictionary();
        private HtmlNode _lastparentnode;
        private int _line;
        private int _lineposition;
        private int _maxlineposition;
        private HtmlDocument.ParseState _oldstate;
        internal IntToHtmlNodeDictionary Openednodes;
        private HtmlDocument.ParseState _state;
        private bool _isHtml = true;
        private bool _obeySelfClosingTags;
#if SALTARELLE
        internal string Text;
#else
        internal LazyTextReader Text;
#endif
        internal bool OptionAddDebuggingAttributes;
        internal bool OptionAutoCloseOnEnd;
        internal bool OptionCheckSyntax = true;
        internal bool OptionExtractErrorSourceText;
        internal int OptionExtractErrorSourceTextMaxLength = 100;
        internal bool OptionFixNestedTags = true;
        public bool OptionOutputAsXml;
        internal bool OptionOutputOptimizeAttributeValues;
        internal bool OptionOutputOriginalCase = true;
        internal bool OptionOutputUpperCase;
        internal bool OptionWriteEmptyNodes;
        internal static readonly string HtmlExceptionRefNotChild = "Reference node must be a child of this node";
        internal static readonly string HtmlExceptionUseIdAttributeFalse = "You need to set UseIdAttribute property to true to enable this feature";
        private static readonly string[] LiResetters = new string[]
        {
            "ul",
            "ol"
        };
        private static readonly string[] OptionResetters = new string[]
        {
            "select"
        };
        private static readonly string[] TrResetters = new string[]
        {
            "table",
        };
        private static readonly string[] PResetters = new string[]
        {
            "div",
            "header",
            "footer",
            "article",
            "section"
        };
        private static readonly string[] ThTdResetters = new string[]
        {
            "tr",
            "table",
        };
#if SALTARELLE
        private HtmlAttribute[] attributesScratchpad = new HtmlAttribute[0];
#else
        private HtmlAttribute[] attributesScratchpad = new HtmlAttribute[32];
#endif
        private int _currentNodeNameStartIndex;
        private int _currentAttributeNameStartIndex;
        private int _currentAttributeValueStartIndex;
        public object Tag
        {
            get;
            set;
        }
#if !SALTARELLE
        public Encoding DeclaredEncoding
		{
			get
			{
				return this._declaredencoding;
			}
		}
#endif
        public HtmlNode DocumentNode
        {
            get
            {
                return this._documentnode;
            }
        }

        public HtmlDocument(string html) : this()
        {
            LoadHtml(html);
        }
        public HtmlDocument()
        {
            this._documentnode = this.CreateNode(HtmlNodeType.Document, 0);
        }
        public static string GetXmlName(string name)
        {
            string text = string.Empty;
            bool flag = true;
            for (int i = 0; i < name.Length; i++)
            {
                if ((name[i] >= 'a' && name[i] <= 'z') || (name[i] >= '0' && name[i] <= '9') || name[i] == '_' || name[i] == '-' || name[i] == '.')
                {
                    text += name[i].ToString();
                }
                else
                {
#if SALTARELLE
                    throw new NotSupportedException("Bad XML element name: " + name);
#else
                    flag = false;
					byte[] bytes = Encoding.UTF8.GetBytes(new char[]
					{
						name[i]
					});
					for (int j = 0; j < bytes.Length; j++)
					{
						text += bytes[j].ToString("x2");
					}
					text += "_";
#endif
                }
            }
            if (flag)
            {
                return text;
            }
            return "_" + text;
        }
        internal static bool IsWhiteSpace(int c)
        {
            return c == 10 || c == 13 || c == 32 || c == 9;
        }
        public HtmlAttribute CreateAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlAttribute result = this.CreateAttribute();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c >= 'A' && c <= 'Z')
                {
                    result._quoteType |= (AttributeValueQuote)2;
                    break;
                }
            }
            result._name = name;
            return result;
        }
        public HtmlAttribute CreateAttribute(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlAttribute result = this.CreateAttribute(name);
            result.Value = value;
            return result;
        }
        public HtmlCommentNode CreateComment()
        {
            return (HtmlCommentNode)this.CreateNode(HtmlNodeType.Comment);
        }
        public HtmlCommentNode CreateComment(string comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException("comment");
            }
            HtmlCommentNode htmlCommentNode = this.CreateComment();
            htmlCommentNode.Comment = comment;
            return htmlCommentNode;
        }
        public HtmlNode CreateElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlNode htmlNode = this.CreateNode(HtmlNodeType.Element);
            htmlNode.TagName = name;
            return htmlNode;
        }
        public HtmlTextNode CreateTextNode()
        {
            return (HtmlTextNode)this.CreateNode(HtmlNodeType.Text);
        }
        public HtmlTextNode CreateTextNode(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            HtmlTextNode htmlTextNode = this.CreateTextNode();
            htmlTextNode.Text = text;
            return htmlTextNode;
        }
#if !SALTARELLE
        public void Load(Stream stream, Encoding initialEncoding)
		{
			this.Load(stream, null, initialEncoding, null);
		}
		public void Load(TextReader reader)
		{
			this.Load(null, reader, null, null);
		}
		public void Load(LazyTextReader reader)
		{
			this.Load(null, null, null, reader);
		}
#endif

#if SALTARELLE
        private void Load(string lazy)
#else
        private void Load(Stream stream, TextReader reader, Encoding encoding, LazyTextReader lazy)
#endif
        {
            if (this.OptionCheckSyntax)
            {
                this.Openednodes = new IntToHtmlNodeDictionary();
            }
            else
            {
                this.Openednodes = null;
            }
#if SALTARELLE
            this.Text = lazy;
#else
            this._declaredencoding = (encoding ?? Encoding.UTF8);
			if (lazy != null)
			{
				this.Text = lazy;
				this._declaredencoding = lazy.Encoding;
			}
			else
			{
				if (stream != null)
				{
					this.Text = new LazyTextReader(stream, encoding);
				}
				else
				{
					this.Text = new LazyTextReader(reader);
				}
			}
#endif
            this._documentnode = this.CreateNode(HtmlNodeType.Document, 0);
            try
            {
                this.Parse();
            }
            finally
            {
                this.Text = null;
            }
            if (!this.OptionCheckSyntax || this.Openednodes == null)
            {
                return;
            }
            foreach (var item in this.Openednodes)
            {
                var current = item.Value;
                if (current._starttag)
                {
                    if (this.OptionExtractErrorSourceText)
                    {
                        string text = current.OuterHtml;
                        if (text.Length > this.OptionExtractErrorSourceTextMaxLength)
                        {
                            text = text.SubstringCached(0, this.OptionExtractErrorSourceTextMaxLength);
                        }
                    }
                }
            }
            this.attributesScratchpad = null;
            this.Openednodes = null;
        }
        public void LoadHtml(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }
#if SALTARELLE
            this.Load(html);
#else
            using (StringReader stringReader = new StringReader(html))
			{
				this.Load(stringReader);
			}
#endif

        }

#if !SALTARELLE
        public void Save(Stream outStream)
		{
			StreamWriter writer = new StreamWriter(outStream, this.DeclaredEncoding ?? Encoding.UTF8);
			this.Save(writer);
		}
		public void Save(Stream outStream, Encoding encoding)
		{
			if (outStream == null)
			{
				throw new ArgumentNullException("outStream");
			}
			if (encoding == null)
			{
				throw new ArgumentNullException("encoding");
			}
			StreamWriter writer = new StreamWriter(outStream, encoding);
			this.Save(writer);
		}
		public void Save(StreamWriter writer)
		{
			this.Save((TextWriter)writer);
		}
#endif
        public void Save(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            this.DocumentNode.WriteTo(writer);
        }
#if !SALTARELLE
        public void Save(XmlWriter writer)
		{
			this.DocumentNode.WriteTo(writer);
			writer.Flush();
		}
#endif
        internal HtmlAttribute CreateAttribute()
        {
            return new HtmlAttribute();
        }
        internal HtmlNode CreateNode(HtmlNodeType type)
        {
            return this.CreateNode(type, -1);
        }
        internal HtmlNode CreateNode(HtmlNodeType type, int index)
        {
            switch (type)
            {
                case HtmlNodeType.Comment:
                    return new HtmlCommentNode(this, index);
                case HtmlNodeType.Text:
                    return new HtmlTextNode(this, index);
                default:
                    return new HtmlNode(type, this, index);
            }
        }
        internal HtmlNode GetXmlDeclaration()
        {
            if (!this._documentnode.HasChildNodes)
            {
                return null;
            }
            foreach (HtmlNode current in this._documentnode.ChildNodes)
            {
                if (current.TagName == "?xml")
                {
                    return current;
                }
            }
            return null;
        }
        internal void UpdateLastParentNode()
        {
            do
            {
                if (this._lastparentnode.Closed)
                {
                    this._lastparentnode = this._lastparentnode.ParentNode;
                }
            }
            while (this._lastparentnode != null && this._lastparentnode.Closed);
            if (this._lastparentnode == null)
            {
                this._lastparentnode = this._documentnode;
            }
        }
        private void CloseCurrentNode()
        {
            if (this._currentnode.Closed)
            {
                return;
            }
            bool flag = false;
            HtmlNode htmlNode = this.Lastnodes.TryGetValue(this._currentnode.TagName);
            if (htmlNode == null)
            {
                if (this._isHtml && HtmlNode.IsClosedElement(this._currentnode.TagName))
                {
                    this._currentnode.CloseNode(this._currentnode);
                    if (this._lastparentnode != null)
                    {
                        HtmlNode htmlNode2 = null;
                        Stack<HtmlNode> stack = new Stack<HtmlNode>();
                        for (HtmlNode htmlNode3 = this._lastparentnode.LastChild; htmlNode3 != null; htmlNode3 = htmlNode3.PreviousSibling)
                        {
                            if (htmlNode3.TagName == this._currentnode.TagName && !htmlNode3.HasChildNodes)
                            {
                                htmlNode2 = htmlNode3;
                                break;
                            }
                            stack.Push(htmlNode3);
                        }
                        if (htmlNode2 != null)
                        {
                            while (stack.Count != 0)
                            {
                                HtmlNode htmlNode4 = stack.Pop();
                                this._lastparentnode.RemoveChild(htmlNode4);
                                htmlNode2.AppendChild(htmlNode4);
                            }
                        }
                        else
                        {
                            this._lastparentnode.AppendChild(this._currentnode);
                        }
                    }
                }
                else
                {
                    if (!this._isHtml || !HtmlNode.IsEmptyElement(this._currentnode.TagName))
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                if (this.OptionFixNestedTags && this.FindResetterNodes(htmlNode, this.GetResetters(this._currentnode.TagName)))
                {
                    flag = true;
                }
                if (!flag)
                {
                    this.Lastnodes[this._currentnode.TagName] = htmlNode._prevwithsamename;
                    htmlNode.CloseNode(this._currentnode);
                }
            }
            if (!flag && this._lastparentnode != null && (!this._isHtml || !HtmlNode.IsClosedElement(this._currentnode.TagName) || this._currentnode._starttag))
            {
                this.UpdateLastParentNode();
            }
        }
        private void DecrementPosition()
        {
            this._index--;
            if (this._lineposition == 1)
            {
                this._lineposition = this._maxlineposition;
                this._line--;
                return;
            }
            this._lineposition--;
        }
        private HtmlNode FindResetterNode(HtmlNode node, string name)
        {
            HtmlNode htmlNode = this.Lastnodes.TryGetValue(name);
            if (htmlNode == null)
            {
                return null;
            }
            if (htmlNode.Closed)
            {
                return null;
            }
            if (htmlNode._streamposition >= node._streamposition)
            {
                return htmlNode;
            }
            return null;
        }
        private bool FindResetterNodes(HtmlNode node, string[] names)
        {
            if (names == null)
            {
                return false;
            }
            for (int i = 0; i < names.Length; i++)
            {
                if (this.FindResetterNode(node, names[i]) != null)
                {
                    return true;
                }
            }
            return false;
        }
        private void FixNestedTag(string name, string[] resetters)
        {
            if (resetters == null)
            {
                return;
            }
            HtmlNode htmlNode = this.Lastnodes.TryGetValue(this._currentnode.TagName);
            if (htmlNode == null || htmlNode.Closed)
            {
                return;
            }
            //#if SALTARELLE
            //            foreach (var item in this.Lastnodes)
            //            {
            //                var current = item.Value;
            //#else
            //            foreach (HtmlNode current in this.Lastnodes.Values)
            //            {
            //#endif
            //                if (current != null && current._outerstartindex > htmlNode._outerstartindex)
            //				{
            //					return;
            //				}
            //			}
            if (!this.FindResetterNodes(htmlNode, resetters))
            {
                htmlNode.CloseNode(null);
                return;
            }
        }
        private void FixNestedTags()
        {
            if (!this._currentnode._starttag)
            {
                return;
            }
            string tagName = this._currentnode.TagName;
            this.FixNestedTag(tagName, this.GetResetters(tagName));
        }
        private string[] GetResetters(string name)
        {
            switch (name)
            {
                case "li":
                    return HtmlDocument.LiResetters;
                case "option":
                    return HtmlDocument.OptionResetters;
                case "tr":
                    return HtmlDocument.TrResetters;
                case "p":
                    return HtmlDocument.PResetters;
                case "th":
                case "td":
                    return HtmlDocument.ThTdResetters;
            }
            return null;
        }
        private void IncrementPosition()
        {
            this._index++;
            this._maxlineposition = this._lineposition;
            if (this._c == 10)
            {
                this._lineposition = 1;
                this._line++;
                return;
            }
            this._lineposition++;
        }
        private bool NewCheck(bool nextIsLetter)
        {
            if (this._c != 60 || !nextIsLetter)
            {
                return false;
            }
            if (!this.PushNodeEnd(this._index - 1, true))
            {
                this._index = -1;
                return true;
            }
            this._state = HtmlDocument.ParseState.WhichTag;
            if (this.Text.ContainsIndex(this._index - 1 + 2 - 1) && this.Text[this._index] == '!')
            {
                this._state = HtmlDocument.ParseState.Comment;
                this._fullcomment = false;
                this._pcdata = false;
                if (this.Text.ContainsIndex(this._index + 2))
                {
                    if (this.Text[this._index + 1] == '-' && this.Text[this._index + 2] == '-')
                    {
                        this._fullcomment = true;
                    }
                    else
                    {
                        if (this.Text.ContainsIndex(this._index + 7) && this.Text[this._index + 1] == '[' && this.Text[this._index + 2] == 'C' && this.Text[this._index + 3] == 'D' && this.Text[this._index + 4] == 'A' && this.Text[this._index + 5] == 'T' && this.Text[this._index + 6] == 'A' && this.Text[this._index + 7] == '[')
                        {
                            this._pcdata = true;
                        }
                    }
                }
                this.PushNodeStart(this._pcdata ? HtmlNodeType.Text : HtmlNodeType.Comment, this._index - 1);
                this.PushNodeNameStart(true, this._index);
                this.PushNodeNameEnd(this._index + 1);
                return true;
            }
            this.PushNodeStart(HtmlNodeType.Element, this._index - 1);
            return true;
        }
        private void Parse()
        {
            int num = 0;
            this.Lastnodes = new StringToHtmlNodeDictionary();
            this._c = 0;
            this._fullcomment = false;
            this._line = 1;
            this._lineposition = 1;
            this._maxlineposition = 1;
            this._state = HtmlDocument.ParseState.Text;
            this._oldstate = this._state;
            this._documentnode._outerlength = 195948557;
            this._lastparentnode = this._documentnode;
            this._currentnode = this.CreateNode(HtmlNodeType.Text, 0);
            this._index = 0;
            this.PushNodeStart(HtmlNodeType.Text, 0);
            while (this._index != -1 && this.Text.ContainsIndex(this._index))
            {
                this._c = (int)this.Text[this._index];
                bool nextIsLetter = false;
                if (this.Text.ContainsIndex(this._index + 1))
                {
                    char c = this.Text[this._index + 1];
                    nextIsLetter = (c == '/' || c == '!' || c == '?' || this.IsLetter(c));
                }
                this.IncrementPosition();
                switch (this._state)
                {
                    case HtmlDocument.ParseState.Text:
                        if (this.NewCheck(nextIsLetter))
                        {
                        }
                        break;
                    case HtmlDocument.ParseState.WhichTag:
                        if (!this.NewCheck(nextIsLetter))
                        {
                            if (this._c == 47)
                            {
                                this.PushNodeNameStart(false, this._index);
                            }
                            else
                            {
                                this.PushNodeNameStart(true, this._index - 1);
                                this.DecrementPosition();
                            }
                            this._state = HtmlDocument.ParseState.Tag;
                        }
                        break;
                    case HtmlDocument.ParseState.Tag:
                        if (!this.NewCheck(nextIsLetter))
                        {
                            if (HtmlDocument.IsWhiteSpace(this._c))
                            {
                                this.PushNodeNameEnd(this._index - 1);
                                if (this._state == HtmlDocument.ParseState.Tag)
                                {
                                    this._state = HtmlDocument.ParseState.BetweenAttributes;
                                }
                            }
                            else
                            {
                                if (this._c == 47)
                                {
                                    this.PushNodeNameEnd(this._index - 1);
                                    if (this._state == HtmlDocument.ParseState.Tag)
                                    {
                                        this._state = HtmlDocument.ParseState.EmptyTag;
                                    }
                                }
                                else
                                {
                                    if (this._c == 62)
                                    {
                                        this.PushNodeNameEnd(this._index - 1);
                                        if (this._state == HtmlDocument.ParseState.Tag)
                                        {
                                            if (!this.PushNodeEnd(this._index, false))
                                            {
                                                this._index = -1;
                                            }
                                            else
                                            {
                                                if (this._state == HtmlDocument.ParseState.Tag)
                                                {
                                                    this._state = HtmlDocument.ParseState.Text;
                                                    this.PushNodeStart(HtmlNodeType.Text, this._index);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.BetweenAttributes:
                        if (!this.NewCheck(nextIsLetter) && !HtmlDocument.IsWhiteSpace(this._c))
                        {
                            if (this._c == 47 || this._c == 63)
                            {
                                if (this._currentnode != null && this._currentnode.TagName == "?document")
                                {
                                    foreach (HtmlAttribute current in this._currentnode.Attributes)
                                    {
                                        this._documentnode.SetAttributeValue(current.OriginalName, current.Value);
                                    }
                                }
                                if (this._obeySelfClosingTags || this._c == 63)
                                {
                                    this._state = HtmlDocument.ParseState.EmptyTag;
                                }
                            }
                            else
                            {
                                if (this._c == 62)
                                {
                                    if (!this.PushNodeEnd(this._index, false))
                                    {
                                        this._index = -1;
                                    }
                                    else
                                    {
                                        if (this._state == HtmlDocument.ParseState.BetweenAttributes)
                                        {
                                            this._state = HtmlDocument.ParseState.Text;
                                            this.PushNodeStart(HtmlNodeType.Text, this._index);
                                        }
                                    }
                                }
                                else
                                {
                                    this.PushAttributeNameStart(this._index - 1);
                                    this._state = HtmlDocument.ParseState.AttributeName;
                                }
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.EmptyTag:
                        if (!this.NewCheck(nextIsLetter))
                        {
                            if (this._c == 62)
                            {
                                if (!this.PushNodeEnd(this._index, true))
                                {
                                    this._index = -1;
                                }
                                else
                                {
                                    if (this._state == HtmlDocument.ParseState.EmptyTag)
                                    {
                                        this._state = HtmlDocument.ParseState.Text;
                                        this.PushNodeStart(HtmlNodeType.Text, this._index);
                                    }
                                }
                            }
                            else
                            {
                                this._state = HtmlDocument.ParseState.BetweenAttributes;
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.AttributeName:
                        if (!this.NewCheck(nextIsLetter))
                        {
                            if (HtmlDocument.IsWhiteSpace(this._c))
                            {
                                this.PushAttributeNameEnd(this._index - 1);
                                this._state = HtmlDocument.ParseState.AttributeBeforeEquals;
                            }
                            else
                            {
                                if (this._c == 61)
                                {
                                    this.PushAttributeNameEnd(this._index - 1);
                                    this._state = HtmlDocument.ParseState.AttributeAfterEquals;
                                }
                                else
                                {
                                    if (this._c == 62)
                                    {
                                        this.PushAttributeNameEnd(this._index - 1);
                                        if (!this.PushNodeEnd(this._index, false))
                                        {
                                            this._index = -1;
                                        }
                                        else
                                        {
                                            if (this._state == HtmlDocument.ParseState.AttributeName)
                                            {
                                                this._state = HtmlDocument.ParseState.Text;
                                                this.PushNodeStart(HtmlNodeType.Text, this._index);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.AttributeBeforeEquals:
                        if (!this.NewCheck(nextIsLetter) && !HtmlDocument.IsWhiteSpace(this._c))
                        {
                            if (this._c == 62)
                            {
                                if (!this.PushNodeEnd(this._index, false))
                                {
                                    this._index = -1;
                                }
                                else
                                {
                                    if (this._state == HtmlDocument.ParseState.AttributeBeforeEquals)
                                    {
                                        this._state = HtmlDocument.ParseState.Text;
                                        this.PushNodeStart(HtmlNodeType.Text, this._index);
                                    }
                                }
                            }
                            else
                            {
                                if (this._c == 61)
                                {
                                    this._state = HtmlDocument.ParseState.AttributeAfterEquals;
                                }
                                else
                                {
                                    this._state = HtmlDocument.ParseState.BetweenAttributes;
                                    this.DecrementPosition();
                                }
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.AttributeAfterEquals:
                        if (!this.NewCheck(nextIsLetter) && !HtmlDocument.IsWhiteSpace(this._c))
                        {
                            if (this._c == 39 || this._c == 34)
                            {
                                this._state = HtmlDocument.ParseState.QuotedAttributeValue;
                                this.PushAttributeValueStart(this._index, this._c);
                                num = this._c;
                            }
                            else
                            {
                                if (this._c == 62)
                                {
                                    if (!this.PushNodeEnd(this._index, false))
                                    {
                                        this._index = -1;
                                    }
                                    else
                                    {
                                        if (this._state == HtmlDocument.ParseState.AttributeAfterEquals)
                                        {
                                            this._state = HtmlDocument.ParseState.Text;
                                            this.PushNodeStart(HtmlNodeType.Text, this._index);
                                        }
                                    }
                                }
                                else
                                {
                                    this.PushAttributeValueStart(this._index - 1);
                                    this._state = HtmlDocument.ParseState.AttributeValue;
                                }
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.AttributeValue:
                        if (!this.NewCheck(nextIsLetter))
                        {
                            if (HtmlDocument.IsWhiteSpace(this._c))
                            {
                                this.PushAttributeValueEnd(this._index - 1);
                                this._state = HtmlDocument.ParseState.BetweenAttributes;
                            }
                            else
                            {
                                if (this._c == 62)
                                {
                                    this.PushAttributeValueEnd(this._index - 1);
                                    if (!this.PushNodeEnd(this._index, false))
                                    {
                                        this._index = -1;
                                    }
                                    else
                                    {
                                        if (this._state == HtmlDocument.ParseState.AttributeValue)
                                        {
                                            this._state = HtmlDocument.ParseState.Text;
                                            this.PushNodeStart(HtmlNodeType.Text, this._index);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.Comment:
                        if (this._c == 62)
                        {
                            if (this._fullcomment)
                            {
                                if (this.Text[this._index - 2] != '-')
                                {
                                    break;
                                }
                                if (this.Text[this._index - 3] != '-')
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (this._pcdata && (this.Text[this._index - 2] != ']' || this.Text[this._index - 3] != ']'))
                                {
                                    break;
                                }
                            }
                            if (this._pcdata)
                            {
                                ((HtmlTextNode)this._currentnode)._pcdata = true;
                            }
                            if (!this.PushNodeEnd(this._index, false))
                            {
                                this._index = -1;
                            }
                            else
                            {
                                this._state = HtmlDocument.ParseState.Text;
                                this.PushNodeStart(HtmlNodeType.Text, this._index);
                            }
                        }
                        break;
                    case HtmlDocument.ParseState.QuotedAttributeValue:
                        if (this._c == num)
                        {
                            this.PushAttributeValueEnd(this._index - 1);
                            this._state = HtmlDocument.ParseState.BetweenAttributes;
                        }
                        break;
                    case HtmlDocument.ParseState.PcData:
                        if (this.Text.ContainsIndex(this._currentnode.TagName.Length + 3 + (this._index - 1) - 1))
                        {
                            bool flag = this.Text[this._index - 1] == '<' && this.Text[this._index] == '/';
                            if (flag)
                            {
                                string tagName = this._currentnode.TagName;
                                for (int i = 0; i < tagName.Length; i++)
                                {
                                    char c2 = this.Text[this._index + i + 1];
                                    if (c2 >= 'A' && c2 <= 'Z')
                                    {
                                        c2 += ' ';
                                    }
                                    if (c2 != tagName[i])
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                            if (flag)
                            {
                                int num2 = (int)this.Text[this._index - 1 + 2 + this._currentnode.TagName.Length];
                                if (num2 == 62 || HtmlDocument.IsWhiteSpace(num2))
                                {
                                    HtmlTextNode htmlTextNode = (HtmlTextNode)this.CreateNode(HtmlNodeType.Text, this._currentnode._outerstartindex + this._currentnode._outerlength);
                                    htmlTextNode._outerlength = this._index - 1 - htmlTextNode._outerstartindex;
                                    htmlTextNode.Text = this.Text.SubstringCached(this._currentnode._outerstartindex + this._currentnode._outerlength, htmlTextNode._outerlength);
                                    htmlTextNode._pcdata = true;
                                    this._currentnode.AppendChild(htmlTextNode);
                                    this.PushNodeStart(HtmlNodeType.Element, this._index - 1);
                                    this.PushNodeNameStart(false, this._index - 1 + 2);
                                    this._state = HtmlDocument.ParseState.Tag;
                                    this.IncrementPosition();
                                }
                            }
                        }
                        break;
                }
            }
#if SALTARELLE
            int readChars = this.Text.Length;
#else
            this.Text.ReadToEnd();
			int readChars = this.Text.ReadChars;
#endif
            this._index = readChars;
            this._documentnode._outerlength = readChars;
            if (this._currentNodeNameStartIndex > 0 && this._currentnode.NodeType == HtmlNodeType.Element)
            {
                this.PushNodeNameEnd(readChars);
            }
            this.PushNodeEnd(readChars, false);
            this.Lastnodes.Clear();
        }
        private bool IsLetter(char ch)
        {
#if SALTARELLE
            return (ch >= 'A' && ch <= '>') || (ch >= 'a' && ch <= 'z');
#else
            return char.IsLetter(ch);
#endif
        }
        private void PushAttributeNameEnd(int index)
        {
            this._currentAttributeIndex++;
#if SALTARELLE
            attributesScratchpad[_currentAttributeIndex] = new HtmlAttribute();
#else
            if (this._currentAttributeIndex == this.attributesScratchpad.Length)
			{
				Array.Resize<HtmlAttribute>(ref this.attributesScratchpad, this.attributesScratchpad.Length * 2);
			}
#endif
            string text = this.Text.SubstringCached(this._currentAttributeNameStartIndex, index - this._currentAttributeNameStartIndex);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= 'A' && c <= 'Z')
                {
                    attributesScratchpad[_currentAttributeIndex]._quoteType = (attributesScratchpad[_currentAttributeIndex]._quoteType | (AttributeValueQuote)2);
                    break;
                }
            }
            this.attributesScratchpad[this._currentAttributeIndex]._name = text;
        }
        private void PushAttributeNameStart(int index)
        {
            this._currentAttributeNameStartIndex = index;
        }
        private void PushAttributeValueEnd(int index)
        {
            string value = HtmlEntity.DeEntitize(this.Text.SubstringCached(this._currentAttributeValueStartIndex, index - this._currentAttributeValueStartIndex));
            this.attributesScratchpad[this._currentAttributeIndex].Value = value;
        }
        private void FlushAttributes()
        {
            if (this._currentAttributeIndex == -1)
            {
                return;
            }
            int num = this._currentAttributeIndex + 1;
            if (this._currentnode._attributeCount != 0)
            {
                Debugger.Break();
            }
            this._currentnode._attributeCount = (byte)num;
#if SALTARELLE
            this._currentnode._attributeArray = attributesScratchpad;
            attributesScratchpad = new HtmlAttribute[0];
#else
            HtmlAttribute[] array = new HtmlAttribute[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.attributesScratchpad[i];
			}
			this._currentnode._attributeArray = array;
#endif
            this._currentAttributeIndex = -1;
        }
        private void PushAttributeValueStart(int index)
        {
            this.PushAttributeValueStart(index, 0);
        }
        private void PushAttributeValueStart(int index, int quote)
        {
            this._currentAttributeValueStartIndex = index;
            if (quote == 39)
            {
                attributesScratchpad[_currentAttributeIndex]._quoteType = (attributesScratchpad[_currentAttributeIndex]._quoteType | AttributeValueQuote.SingleQuote);
            }
        }
        private bool PushNodeEnd(int index, bool close)
        {
            this._currentnode._outerlength = index - this._currentnode._outerstartindex;
            if (this._currentnode._nodetype == HtmlNodeType.Text || this._currentnode._nodetype == HtmlNodeType.Comment)
            {
                if (this._currentnode._outerlength > 0)
                {
                    HtmlTextNode htmlTextNode = (this._currentnode._nodetype == HtmlNodeType.Text) ? ((HtmlTextNode)this._currentnode) : null;
                    HtmlCommentNode htmlCommentNode = (this._currentnode._nodetype == HtmlNodeType.Comment) ? ((HtmlCommentNode)this._currentnode) : null;
                    if (htmlTextNode != null)
                    {
                        if (htmlTextNode._pcdata)
                        {
                            htmlTextNode.Text = this.Text.SubstringCached(this._currentnode._outerstartindex + 9, this._currentnode._outerlength - 9 - 3);
                        }
                        else
                        {
                            htmlTextNode.Text = HtmlEntity.DeEntitize(this.Text.SubstringCached(this._currentnode._outerstartindex, this._currentnode._outerlength));
                        }
                    }
                    else
                    {
                        htmlCommentNode.Comment = this.Text.SubstringCached(this._currentnode._outerstartindex, this._currentnode._outerlength);
                    }
                    if (this._lastparentnode != null)
                    {
                        this._lastparentnode.AppendChild(this._currentnode);
                    }
                }
            }
            else
            {
                this.FlushAttributes();
                if (this._currentnode._starttag && this._lastparentnode != this._currentnode)
                {
                    if (this._lastparentnode != null)
                    {
                        string tagName = this._currentnode.TagName;
                        if (tagName != "?document")
                        {
                            this._lastparentnode.AppendChild(this._currentnode);
                        }
                    }
#if !SALTARELLE
                    this.ReadDocumentEncoding(this._currentnode);
#endif
                    HtmlNode prevwithsamename = this.Lastnodes.TryGetValue(this._currentnode.TagName);
                    this._currentnode._prevwithsamename = prevwithsamename;
                    this.Lastnodes[this._currentnode.TagName] = this._currentnode;
                    if (this._currentnode.NodeType == HtmlNodeType.Document || this._currentnode.NodeType == HtmlNodeType.Element)
                    {
                        this._lastparentnode = this._currentnode;
                    }
                    if (this._isHtml && HtmlNode.IsCDataElement(this._currentnode.TagName))
                    {
                        this._state = HtmlDocument.ParseState.PcData;
                        return true;
                    }
                    if (this._isHtml && (HtmlNode.IsClosedElement(this._currentnode.TagName) || HtmlNode.IsEmptyElement(this._currentnode.TagName)))
                    {
                        close = true;
                    }
                }
            }
            if (close || !this._currentnode._starttag)
            {
                this.CloseCurrentNode();
            }
            return true;
        }
        private void PushNodeNameEnd(int index)
        {
            this._currentnode.TagName = this.Text.SubstringCached(this._currentNodeNameStartIndex, index - this._currentNodeNameStartIndex);
            if (this.Openednodes.Count == 1 && this._isHtml)
            {
                string tagName = this._currentnode.TagName;
                if (tagName != "html" && !tagName.StartsWith("?") && !tagName.StartsWith("!"))
                {
                    bool flag = false;
                    bool flag2 = false;
                    foreach (HtmlNode current in this.DocumentNode.ChildNodes)
                    {
                        string tagName2 = current.TagName;
                        if (tagName2 == "?xml")
                        {
                            flag2 = true;
                        }
                        if (tagName2 == "html")
                        {
                            flag = true;
                        }
                    }
                    if (flag2 && !flag)
                    {
#if !SALTARELLE
                        this.Text.ReadToEnd();
#endif
                        this._isHtml = false;
                    }
                    if (flag2)
                    {
                        this._obeySelfClosingTags = true;
                    }
                }
            }
            if (this.OptionFixNestedTags)
            {
                this.FixNestedTags();
            }
        }
        private void PushNodeNameStart(bool starttag, int index)
        {
            this._currentnode._starttag = starttag;
            this._currentNodeNameStartIndex = index;
        }
        private void PushNodeStart(HtmlNodeType type, int index)
        {
            this._currentnode = this.CreateNode(type, index);
            this._currentnode._streamposition = index;
        }
#if !SALTARELLE
        private void ReadDocumentEncoding(HtmlNode node)
		{
			if (node.TagName == "body")
			{
				this.Text.ReadToEnd();
			}
			if (node.TagName != "meta")
			{
				return;
			}
			string text = node.GetAttributeValue("charset");
			if (text == null)
			{
				string attributeValue = node.GetAttributeValue("http-equiv");
				if (attributeValue == null)
				{
					return;
				}
				if (string.Compare(attributeValue, "content-type", StringComparison.OrdinalIgnoreCase) != 0)
				{
					return;
				}
				string attributeValue2 = node.GetAttributeValue("content");
				text = NameValuePairList.GetNameValuePairsValue(attributeValue2, "charset");
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (string.Equals(text, "utf8", StringComparison.OrdinalIgnoreCase))
			{
				text = "utf-8";
			}
			try
			{
				this._declaredencoding = Encoding.GetEncoding(text);
				this.Text.TrySetEncoding(this._declaredencoding);
			}
			catch (ArgumentException)
			{
			}
			this.Text.ReadToEnd();
		}
#endif
        public void WriteTo(TextWriter writer, bool writeDocumentNode)
        {
            this.DocumentNode.WriteTo(writer, writeDocumentNode);
        }

        public void SetOwnerDocumentRecursive(HtmlNode node)
        {
            foreach (var item in node.DescendantsAndSelf())
            {
                item._ownerdocument = this;
                item._streamposition = -1;
            }
        }



        internal static Uri GetAbsoluteUrlInternal(Uri baseUrl, string relative)
        {

            if (relative.StartsWith("http:") || relative.StartsWith("https:"))
                return new Uri(relative);

            if (relative.StartsWith("//"))
                return new Uri((baseUrl != null ? baseUrl.Scheme : "https") + ":" + relative);

            var firstColon = relative.IndexOf(':', 0, Math.Min(relative.Length, 15));
            var firstSlash = relative.IndexOf('/');
            var firstQuestionMark = relative.IndexOf('?');
            if (firstColon != -1 && (firstSlash == -1 || firstColon < firstSlash) && (firstQuestionMark == -1 || firstColon < firstQuestionMark))
                return new Uri(relative);

            if (baseUrl == null) throw new ArgumentException("Cannot create an absolute Uri without a base Uri.");

            return new Uri(baseUrl, relative);

        }

        public Uri BaseUrl
        {
            get
            {
#if !SALTARELLE
                if (_baseUrlCustom != null) return CustomPageUrlTypeConverter(_baseUrlCustom);
#endif

                if (_baseUrl != null) return _baseUrl;


                var b = DocumentNode.GetAttributeValue("base-url");
                if (b == null)
                {
                    foreach (var basenode in DocumentNode.DescendantsAndSelf("base"))
                    {
                        var h = basenode.GetAttributeValue("href");
                        if (h != null)
                        {
                            try
                            {
                                _baseUrl = GetAbsoluteUrlInternal(PageUrl, h);
                                b = _baseUrl.AbsoluteUri;
                            }
                            catch
                            {
                            }
                            break;
                        }
                    }
                    if (b == null)
                    {
                        _baseUrl = PageUrl;
                        b = _baseUrl != null ? _baseUrl.AbsoluteUri : string.Empty;
                    }
                    DocumentNode.SetAttributeValue("base-url", b);
                }
                else
                {
                    if (string.IsNullOrEmpty(b)) return null;
                    _baseUrl = new Uri(b);
                }
                return _baseUrl;
            }
        }

        public Uri PageUrl
        {
            get
            {
#if !SALTARELLE
                if (_pageUrlCustom != null) return CustomPageUrlTypeConverter(_pageUrlCustom);
#endif
                if (_pageUrl != null) return _pageUrl;

                var m = DocumentNode.GetAttributeValue("document-url");
                if (string.IsNullOrEmpty(m)) return null;

                _pageUrl = new Uri(m);

                return _pageUrl;
            }
            set
            {
#if !SALTARELLE
                _pageUrlCustom = null;
#endif
                if (value != null) DocumentNode.SetAttributeValue("document-url", value.AbsoluteUri);
                else DocumentNode.Attributes.Remove("document-url");
                _pageUrl = value;
            }
        }

#if !SALTARELLE
        public object _pageUrlCustom;
        public object _baseUrlCustom;
        public static Func<object, Uri> CustomPageUrlTypeConverter;
#endif
        private Uri _baseUrl;
        private Uri _pageUrl;

        public void ClearPageUrlCache()
        {
#if !SALTARELLE
            _pageUrlCustom = null;
            _baseUrlCustom = null;
#endif
            _pageUrl = null;
            _baseUrl = null;
        }
        
    }
}
