using Shaman.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
#if SALTARELLE
using StringBuilder = System.Text.Saltarelle.StringBuilder;
#endif
namespace Shaman.Dom
{
    [DebuggerDisplay("Name: {OriginalName}}")]
    public class HtmlNode
    {
        private class DescendantsEnumerator : IEnumerator<HtmlNode>, IEnumerator, IDisposable
        {
            private readonly string nameFilter;
            private bool yieldSelf;
            private HtmlNode[][] stack;
            private int[] childCounts;
            private int[] positions;
            private int stackSize;
            private HtmlNode current;
            private HtmlNode baseNode;
            public HtmlNode Current
            {
                get
                {
                    return this.current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return this.current;
                }
            }
            public DescendantsEnumerator(HtmlNode.DescendantsEnumerable enumerable)
            {
                this.nameFilter = enumerable.name;
                this.yieldSelf = enumerable.self;
                this.baseNode = enumerable.htmlNode;
                this.stack = new HtmlNode[30][];
                this.childCounts = new int[30];
                this.stack[0] = this.baseNode._childNodesArray;
                this.childCounts[0] = this.baseNode._childNodesCount;
                this.positions = new int[30];
                this.stackSize = 1;
            }
            public void Dispose()
            {
                this.stack = null;
                this.childCounts = null;
                this.current = null;
                this.baseNode = null;
            }
            public bool MoveNext()
            {
                if (this.yieldSelf)
                {
                    this.yieldSelf = false;
                    if (this.nameFilter == null || this.baseNode.TagName == this.nameFilter)
                    {
                        this.current = this.baseNode;
                        return true;
                    }
                }
                while (this.stackSize != 0)
                {
                    HtmlNode[] array = this.stack[this.stackSize - 1];
                    int num = this.positions[this.stackSize - 1];
                    int num2 = this.childCounts[this.stackSize - 1];
#if SALTARELLE
                    if (Script.IsUndefined(num))
                    {
                        num = 0;
                    }
                    if (Script.IsUndefined(num2))
                    {
                        num2 = 0;
                    }
#endif
                    if (array == null || num == num2)
                    {
                        this.stackSize--;
                    }
                    else
                    {
                        HtmlNode htmlNode = array[num];
#if !SALTARELLE
                        if (this.stackSize == this.stack.Length)
                        {
                            HtmlNode[][] destinationArray = new HtmlNode[this.stackSize * 2][];
                            int[] destinationArray2 = new int[this.stackSize * 2];
                            int[] destinationArray3 = new int[this.stackSize * 2];
                            Array.Copy(this.stack, destinationArray, this.stackSize);
                            Array.Copy(this.positions, destinationArray3, this.stackSize);
                            Array.Copy(this.childCounts, destinationArray2, this.stackSize);
                            this.stack = destinationArray;
                            this.positions = destinationArray3;
                            this.childCounts = destinationArray2;
                        }
#endif
                        this.stack[this.stackSize] = htmlNode._childNodesArray;
                        this.childCounts[this.stackSize] = htmlNode._childNodesCount;
                        this.positions[this.stackSize] = 0;
                        int num3 = this.positions[this.stackSize - 1];
#if SALTARELLE
                        if (Script.IsUndefined(num3))
                        {
                            num3 = 0;
                        }
#endif
                        num3++;
                        this.positions[this.stackSize - 1] = num3;
                        this.stackSize++;
                        if (this.nameFilter == null || this.nameFilter == htmlNode.TagName)
                        {
                            this.current = htmlNode;
                            return true;
                        }
                    }
                }
                return false;
            }
            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
        private class DescendantsEnumerable : IEnumerable<HtmlNode>, IEnumerable
        {
            internal readonly HtmlNode htmlNode;
            internal readonly bool self;
            internal readonly string name;
            public DescendantsEnumerable(HtmlNode htmlNode, bool self, string name)
            {
                this.htmlNode = htmlNode;
                this.self = self;
                this.name = ((name != null) ? name.ToLowerFast() : null);
            }
            public IEnumerator<HtmlNode> GetEnumerator()
            {
                return new HtmlNode.DescendantsEnumerator(this);
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
        private bool _closed;
        internal int _index;
        private string _originalName;
        internal HtmlNodeType _nodetype;
        internal int _outerlength;
        internal int _outerstartindex;
        internal HtmlDocument _ownerdocument;
        internal HtmlNode _parentnode;
        internal HtmlNode _prevwithsamename;
        internal bool _starttag;
        internal int _streamposition;
        private long _nodeId;
#if SALTARELLE
        internal HtmlAttribute[] _attributeArray;
#else
        internal volatile HtmlAttribute[] _attributeArray;
#endif
        internal byte _attributeCount;
        public static readonly string HtmlNodeTypeNameComment;
        public static readonly string HtmlNodeTypeNameDocument;
        public static readonly string HtmlNodeTypeNameText;
#if SALTARELLE
        public static JsDictionary<string, HtmlElementFlag> ElementsFlags;
#else
        public static Dictionary<string, HtmlElementFlag> ElementsFlags;
#endif
        private string _lowerName;
        internal HtmlNode[] _childNodesArray;
        internal int _childNodesCount;

#if SALTARELLE
        private HtmlNodeCollection _childNodes;
        private HtmlAttributeCollection _attributes;
#endif
        public HtmlNodeCollection ChildNodes
        {
            get
            {
#if SALTARELLE
                return _childNodes ?? (_childNodes = new HtmlNodeCollection(this));
#else
                return new HtmlNodeCollection(this);
#endif
            }
        }
        public HtmlAttributeCollection Attributes
        {
            get
            {
#if SALTARELLE
                return _attributes ?? (_attributes = new HtmlAttributeCollection(this));
#else
                return new HtmlAttributeCollection(this);
#endif
            }
        }
        public long NodeId
        {
            get
            {
                return this._nodeId;
            }
            set
            {
                this._nodeId = value;
            }
        }
        public bool Closed
        {
            get
            {
                return this._closed;
            }
        }
        public HtmlNode FirstChild
        {
            get
            {
                if (this.HasChildNodes)
                {
                    return this.ChildNodes[0];
                }
                return null;
            }
        }
        public bool HasAttributes
        {
            get
            {
                return this._attributeCount != 0;
            }
        }
        public bool HasChildNodes
        {
            get
            {
                return this.ChildNodes.Count > 0;
            }
        }
        public string Id
        {
            get
            {
                return this.GetAttributeValue("id");
            }
            set
            {
                this.SetAttributeValue("id", value);
            }
        }
        public string InnerHtml
        {
            get
            {
                return this.WriteContentTo();
            }
        }
        public string InnerText
        {
            get
            {
                if (this._childNodesCount == 1 && this._childNodesArray[0]._nodetype == HtmlNodeType.Text)
                    return ((HtmlTextNode)_childNodesArray[0]).Text;

                if (this._nodetype == HtmlNodeType.Text)
                {
                    return ((HtmlTextNode)this).Text;
                }
                if (this._nodetype == HtmlNodeType.Comment)
                {
                    return ((HtmlCommentNode)this).Comment;
                }
                if (!this.HasChildNodes)
                {
                    return string.Empty;
                }
                var sb = new StringBuilder();
                foreach (var desc in this.DescendantsAndSelf())
                {
                    if (desc._nodetype == HtmlNodeType.Text) sb.Append(((HtmlTextNode)desc).Text);
                    else if (desc._nodetype == HtmlNodeType.Comment) sb.Append(((HtmlCommentNode)desc).Comment);
                }
                return sb.ToString();
            }
        }
        public HtmlNode LastChild
        {
            get
            {
                if (this.HasChildNodes)
                {
                    return this.ChildNodes[this.ChildNodes.Count - 1];
                }
                return null;
            }
        }
        public string TagName
        {
            get
            {
                if (this._lowerName == null)
                {
                    this._lowerName = ((this._originalName != null) ? this._originalName.ToLowerFast() : string.Empty);
                }
                return this._lowerName;
            }
            set
            {
                this._originalName = value;
                this._lowerName = null;
            }
        }
        public HtmlNode NextSibling
        {
            get
            {
                if (this._parentnode == null)
                {
                    return null;
                }
                int num = this._index + 1;
                if (num == this._parentnode._childNodesCount)
                {
                    return null;
                }
                return this._parentnode._childNodesArray[num];
            }
        }
        public HtmlNodeType NodeType
        {
            get
            {
                return this._nodetype;
            }
            internal set
            {
                this._nodetype = value;
            }
        }
        public string OriginalName
        {
            get
            {
                return this._originalName;
            }
        }
        public string OuterHtml
        {
            get
            {
                return this.WriteTo();
            }
        }
        public HtmlDocument OwnerDocument
        {
            get
            {
                return this._ownerdocument;
            }
            internal set
            {
                this._ownerdocument = value;
            }
        }
        public HtmlNode ParentNode
        {
            get
            {
                return this._parentnode;
            }
            internal set
            {
                this._parentnode = value;
            }
        }
        public HtmlNode PreviousSibling
        {
            get
            {
                if (this._parentnode == null)
                {
                    return null;
                }
                if (this._index == 0)
                {
                    return null;
                }
                return this._parentnode._childNodesArray[this._index - 1];
            }
        }
        public int StreamPosition
        {
            get
            {
                return this._streamposition;
            }
        }
        public string Class
        {
            get
            {
                return this.GetAttributeValue("class");
            }
        }
        public string Style
        {
            get
            {
                return this.GetAttributeValue("style");
            }
        }
        public IEnumerable<string> ClassList
        {
            get
            {
                string @class = this.Class;
                if (string.IsNullOrEmpty(@class))
                {
#if SALTARELLE
                    return new string[0];
#else
                    return Enumerable.Empty<string>();
#endif
                }
                return HtmlNode.GetClasses(@class);
            }
        }
        static HtmlNode()
        {
            HtmlNode.HtmlNodeTypeNameComment = "#comment";
            HtmlNode.HtmlNodeTypeNameDocument = "#document";
            HtmlNode.HtmlNodeTypeNameText = "#text";
#if SALTARELLE
            HtmlNode.ElementsFlags = new JsDictionary<string, HtmlElementFlag>();
#else
            HtmlNode.ElementsFlags = new Dictionary<string, HtmlElementFlag>();
#endif
            HtmlNode.ElementsFlags["script"] = HtmlElementFlag.CData;
            HtmlNode.ElementsFlags["style"] = HtmlElementFlag.CData;
            HtmlNode.ElementsFlags["noxhtml"] = HtmlElementFlag.CData;
            HtmlNode.ElementsFlags["base"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["link"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["meta"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["isindex"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["hr"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["col"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["param"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["embed"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["frame"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["wbr"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["bgsound"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["source"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["track"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["spacer"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["keygen"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["area"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["input"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["command"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["basefont"] = HtmlElementFlag.Empty;
            HtmlNode.ElementsFlags["br"] = (HtmlElementFlag.Empty | HtmlElementFlag.Closed);
        }
        public HtmlNode(HtmlNodeType type, HtmlDocument ownerdocument, int index)
        {
            this._nodetype = type;
            this._ownerdocument = ownerdocument;
            this._outerstartindex = index;
            switch (type)
            {
                case HtmlNodeType.Document:
                    this.TagName = HtmlNode.HtmlNodeTypeNameDocument;
                    this._closed = true;
                    break;
                case HtmlNodeType.Comment:
                    this.TagName = HtmlNode.HtmlNodeTypeNameComment;
                    this._closed = true;
                    break;
                case HtmlNodeType.Text:
                    this.TagName = HtmlNode.HtmlNodeTypeNameText;
                    this._closed = true;
                    break;
            }
            if (this._ownerdocument.Openednodes != null && !this.Closed && -1 != index)
            {
                this._ownerdocument.Openednodes[index] = this;
            }
            if (-1 != index || type != HtmlNodeType.Comment)
            {
            }
        }
        internal void EnsureCapacity(int capacity)
        {
#if SALTARELLE
            if (capacity != 0 && _childNodesArray == null)
            {
                _childNodesArray = new HtmlNode[0];
            }
#else
            if (this._childNodesArray == null)
            {
                if (capacity != 0)
                {
                    this._childNodesArray = new HtmlNode[4];
                    return;
                }
            }
            else
            {
                if (this._childNodesArray.Length < capacity)
                {
                    HtmlNode[] array = new HtmlNode[Math.Max(this._childNodesArray.Length * 2, capacity)];
                    for (int i = 0; i < this._childNodesCount; i++)
                    {
                        array[i] = this._childNodesArray[i];
                    }
                    this._childNodesArray = array;
                }
            }
#endif
        }
        public static HtmlNode CreateNode(string html)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return htmlDocument.DocumentNode.FirstChild;
        }
        public static bool IsCDataElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlElementFlag htmlElementFlag = HtmlNode.ElementsFlags.TryGetValue(name.ToLowerFast());
            return (htmlElementFlag & HtmlElementFlag.CData) != (HtmlElementFlag)0;
        }
        public static bool IsClosedElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlElementFlag htmlElementFlag = HtmlNode.ElementsFlags.TryGetValue(name.ToLowerFast());
            return (htmlElementFlag & HtmlElementFlag.Closed) != (HtmlElementFlag)0;
        }
        public static bool IsEmptyElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                return true;
            }
            if ('!' == name[0])
            {
                return true;
            }
            if ('?' == name[0])
            {
                return true;
            }
            HtmlElementFlag htmlElementFlag = HtmlNode.ElementsFlags.TryGetValue(name.ToLowerFast());
            return (htmlElementFlag & HtmlElementFlag.Empty) != (HtmlElementFlag)0;
        }
        public IEnumerable<HtmlNode> Ancestors()
        {
            var node = this.ParentNode;
            while (node != null)
            {
                yield return node;
                node = node.ParentNode;
            }
        }
        public IEnumerable<HtmlNode> Ancestors(string name)
        {
            var node = this.ParentNode;
            while (node != null)
            {
                if (node.TagName == name)
                    yield return node;
                node = node.ParentNode;
            }
        }
        public IEnumerable<HtmlNode> AncestorsAndSelf()
        {
            var node = this;
            while (node != null)
            {
                yield return node;
                node = node.ParentNode;
            }
        }
        public IEnumerable<HtmlNode> AncestorsAndSelf(string name)
        {
            var node = this;
            while (node != null)
            {
                if (node.TagName == name)
                    yield return node;
                node = node.ParentNode;
            }
        }
        public HtmlNode AppendChild(HtmlNode newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            this.ChildNodes.Append(newChild);
            return newChild;
        }
        public IEnumerable<HtmlNode> Descendants()
        {
            return new HtmlNode.DescendantsEnumerable(this, false, null);
        }
        public IEnumerable<HtmlNode> Descendants(string name)
        {
            return new HtmlNode.DescendantsEnumerable(this, false, name);
        }
        public IEnumerable<HtmlNode> DescendantsAndSelf()
        {
            return new HtmlNode.DescendantsEnumerable(this, true, null);
        }
        public IEnumerable<HtmlNode> DescendantsAndSelf(string name)
        {
            return new HtmlNode.DescendantsEnumerable(this, true, name);
        }
        public string GetAttributeValue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!this.HasAttributes)
            {
                return null;
            }
            int attributeIndex = this.Attributes.GetAttributeIndex(name);
            if (attributeIndex == -1)
            {
                return null;
            }
            return this._attributeArray[attributeIndex].Value;
        }
        public HtmlNode InsertAfter(HtmlNode newChild, HtmlNode refChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            if (refChild == null)
            {
                return this.PrependChild(newChild);
            }
            if (newChild == refChild)
            {
                return newChild;
            }
            int num = this.ChildNodes[refChild];
            if (num == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            this.ChildNodes.Insert(num + 1, newChild);
            return newChild;
        }
        public HtmlNode InsertBefore(HtmlNode newChild, HtmlNode refChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            if (refChild == null)
            {
                return this.AppendChild(newChild);
            }
            if (newChild == refChild)
            {
                return newChild;
            }
            int num = this.ChildNodes[refChild];
            if (num == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            this.ChildNodes.Insert(num, newChild);
            return newChild;
        }
        public HtmlNode PrependChild(HtmlNode newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            this.ChildNodes.Prepend(newChild);
            return newChild;
        }
        public void PrependChildren(HtmlNodeCollection newChildren)
        {
            foreach (HtmlNode current in newChildren)
            {
                this.PrependChild(current);
            }
        }
        public void Remove()
        {
            if (this.ParentNode != null)
            {
                this.ParentNode.ChildNodes.Remove(this);
            }
        }
        public void RemoveAllChildren()
        {
            if (!this.HasChildNodes)
            {
                return;
            }
            this.ChildNodes.Clear();
        }
        public HtmlNode RemoveChild(HtmlNode oldChild)
        {
            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }
            int num = this.ChildNodes[oldChild];
            if (num == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            this.ChildNodes.Remove(num);
            return oldChild;
        }
        public HtmlNode RemoveChild(HtmlNode oldChild, bool keepGrandChildren)
        {
            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }
            if (keepGrandChildren)
            {
                HtmlNode previousSibling = oldChild.PreviousSibling;
                foreach (HtmlNode current in oldChild.ChildNodes)
                {
                    this.InsertAfter(current, previousSibling);
                }
            }
            this.RemoveChild(oldChild);
            return oldChild;
        }
        public HtmlNode ReplaceChild(HtmlNode newChild, HtmlNode oldChild)
        {
            if (newChild == null)
            {
                return this.RemoveChild(oldChild);
            }
            if (oldChild == null)
            {
                return this.AppendChild(newChild);
            }
            int num = this.ChildNodes[oldChild];
            if (num == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            this.ChildNodes.Replace(num, newChild);
            return newChild;
        }
        public void SetAttributeValue(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            int attributeIndex = this.Attributes.GetAttributeIndex(name);
            if (attributeIndex == -1 && value == null)
            {
                return;
            }
            if (attributeIndex == -1)
            {
                this.Attributes.AddInternal(this._ownerdocument.CreateAttribute(name, value));
                return;
            }
            if (value == null)
            {
                this.Attributes.RemoveAt(attributeIndex);
                return;
            }
            this._attributeArray[attributeIndex].Value = value;
        }
        public void WriteContentTo(TextWriter outText)
        {
            foreach (HtmlNode current in this.ChildNodes)
            {
                current.WriteTo(outText);
            }
        }
        public string WriteContentTo()
        {
            StringWriter stringWriter = new StringWriter();
            this.WriteContentTo(stringWriter);
            return stringWriter.ToString();
        }
        public void WriteTo(TextWriter outText)
        {
            this.WriteTo(outText, false);
        }
        internal void WriteTo(TextWriter outText, bool writeDocumentNode)
        {
            switch (this._nodetype)
            {
                case HtmlNodeType.Document:
                    if (this._ownerdocument.OptionOutputAsXml)
                    {
#if SALTARELLE
                        outText.Write("<?xml version=\"1.0\"?>");
#else
                        outText.Write("<?xml version=\"1.0\" encoding=\"" + (this._ownerdocument.DeclaredEncoding ?? Encoding.UTF8).WebName + "\"?>");
#endif
                        if (this._ownerdocument.DocumentNode.HasChildNodes)
                        {
                            int num = this._ownerdocument.DocumentNode.ChildNodes.Count;
                            if (num > 0)
                            {
                                HtmlNode xmlDeclaration = this._ownerdocument.GetXmlDeclaration();
                                if (xmlDeclaration != null)
                                {
                                    num--;
                                }
                                if (num > 1)
                                {
                                    if (this._ownerdocument.OptionOutputUpperCase)
                                    {
                                        outText.Write("<SPAN>");
                                        this.WriteContentTo(outText);
                                        outText.Write("</SPAN>");
                                        return;
                                    }
                                    outText.Write("<span>");
                                    this.WriteContentTo(outText);
                                    outText.Write("</span>");
                                    return;
                                }
                            }
                        }
                    }
                    if (writeDocumentNode)
                    {
                        outText.Write("<?document");
                        this.WriteAttributes(outText, false);
                        outText.Write("?>");
                    }
                    this.WriteContentTo(outText);
                    return;
                case HtmlNodeType.Element:
                    {
                        string text = this._ownerdocument.OptionOutputUpperCase ? this.TagName.ToUpper() : this.TagName;
                        if (this._ownerdocument.OptionOutputOriginalCase)
                        {
                            text = this.OriginalName;
                        }
                        if (this._ownerdocument.OptionOutputAsXml)
                        {
                            if (text.Length <= 0)
                            {
                                break;
                            }
                            if (text[0] == '?')
                            {
                                return;
                            }
                            if (text.Trim().Length == 0)
                            {
                                return;
                            }
                            text = HtmlDocument.GetXmlName(text);
                        }
                        outText.Write("<" + text);
                        this.WriteAttributes(outText, false);
                        if (this.HasChildNodes)
                        {
                            outText.Write(">");
                            bool flag = false;
                            if (this._ownerdocument.OptionOutputAsXml && HtmlNode.IsCDataElement(this.TagName))
                            {
                                flag = true;
                                outText.Write("\r\n//<![CDATA[\r\n");
                            }
                            if (flag)
                            {
                                if (this.HasChildNodes)
                                {
                                    this.ChildNodes[0].WriteTo(outText);
                                }
                                outText.Write("\r\n//]]>//\r\n");
                            }
                            else
                            {
                                this.WriteContentTo(outText);
                            }
                            outText.Write("</" + text);
                            if (!this._ownerdocument.OptionOutputAsXml)
                            {
                                this.WriteAttributes(outText, true);
                            }
                            outText.Write(">");
                            return;
                        }
                        if (HtmlNode.IsEmptyElement(this.TagName))
                        {
                            if (this._ownerdocument.OptionWriteEmptyNodes || this._ownerdocument.OptionOutputAsXml)
                            {
                                outText.Write(" />");
                                return;
                            }
                            if (this.TagName.Length > 0 && this.TagName[0] == '?')
                            {
                                outText.Write("?");
                            }
                            outText.Write(">");
                            return;
                        }
                        else
                        {
                            outText.Write("></" + text + ">");
                        }
                        break;
                    }
                case HtmlNodeType.Comment:
                    {
                        string text2 = ((HtmlCommentNode)this).Comment;
                        if (this._ownerdocument.OptionOutputAsXml)
                        {
                            outText.Write("<!--" + HtmlNode.GetXmlComment((HtmlCommentNode)this) + " -->");
                            return;
                        }
                        outText.Write(text2);
                        return;
                    }
                case HtmlNodeType.Text:
                    {
                        HtmlTextNode htmlTextNode = (HtmlTextNode)this;
                        string text2 = htmlTextNode.Text;
                        if (!htmlTextNode.IsPcData)
                        {
                            outText.Write(HtmlEntity.Entitize(text2));
                            return;
                        }
                        bool optionOutputAsXml = this._ownerdocument.OptionOutputAsXml;
                        if (optionOutputAsXml)
                        {
                            outText.Write("<![CDATA[");
                        }
                        outText.Write(htmlTextNode.Text);
                        if (optionOutputAsXml)
                        {
                            outText.Write("]]>");
                            return;
                        }
                        break;
                    }
                default:
                    return;
            }
        }
#if !SALTARELLE
        public void WriteTo(XmlWriter writer)
        {
            switch (this._nodetype)
            {
                case HtmlNodeType.Document:
                    writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + (this._ownerdocument.DeclaredEncoding ?? Encoding.UTF8).WebName + "\"");
                    if (!this.HasChildNodes)
                    {
                        return;
                    }
                    using (HtmlNodeCollection.Enumerator enumerator = this.ChildNodes.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            HtmlNode current = enumerator.Current;
                            current.WriteTo(writer);
                        }
                        return;
                    }
                    break;
                case HtmlNodeType.Element:
                    {
                        string localName = this._ownerdocument.OptionOutputUpperCase ? this.TagName.ToUpper() : this.TagName;
                        if (this._ownerdocument.OptionOutputOriginalCase)
                        {
                            localName = this.OriginalName;
                        }
                        writer.WriteStartElement(localName);
                        HtmlNode.WriteAttributes(writer, this);
                        if (this.HasChildNodes)
                        {
                            foreach (HtmlNode current2 in this.ChildNodes)
                            {
                                current2.WriteTo(writer);
                            }
                        }
                        writer.WriteEndElement();
                        return;
                    }
                case HtmlNodeType.Comment:
                    writer.WriteComment(HtmlNode.GetXmlComment((HtmlCommentNode)this));
                    return;
                case HtmlNodeType.Text:
                    break;
                default:
                    return;
            }
            string text = ((HtmlTextNode)this).Text;
            writer.WriteString(text);
        }
#endif
        public string WriteTo()
        {
            string result;
            using (StringWriter stringWriter = new StringWriter())
            {
                this.WriteTo(stringWriter);
                stringWriter.Flush();
                result = stringWriter.ToString();
            }
            return result;
        }
        internal static string GetXmlComment(HtmlCommentNode comment)
        {
            string comment2 = comment.Comment;
            return comment2.SubstringCached(4, comment2.Length - 7).Replace("--", " - -");
        }
#if !SALTARELLE
        internal static void WriteAttributes(XmlWriter writer, HtmlNode node)
        {
            if (!node.HasAttributes)
            {
                return;
            }
            List<string> list = new List<string>();
            foreach (HtmlAttribute current in node.Attributes)
            {
                string xmlName = current.XmlName;
                if (!list.Contains(xmlName))
                {
                    list.Add(xmlName);
                    writer.WriteAttributeString(xmlName, current.Value);
                }
            }
        }
#endif
        internal void CloseNode(HtmlNode endnode)
        {
            if (!this._ownerdocument.OptionAutoCloseOnEnd)
            {
                foreach (HtmlNode current in this.ChildNodes)
                {
                    if (!current.Closed)
                    {
                        current.CloseNode(null);
                    }
                }
            }
            if (!this.Closed)
            {
                this._closed = true;
                if (this._ownerdocument.Openednodes != null)
                {
                    this._ownerdocument.Openednodes.Remove(this._outerstartindex);
                }
                HtmlNode htmlNode = this._ownerdocument.Lastnodes.TryGetValue(this.TagName);
                if (htmlNode == this)
                {
                    this._ownerdocument.Lastnodes.Remove(this.TagName);
                    this._ownerdocument.UpdateLastParentNode();
                }
                if (endnode == null || endnode == this)
                {
                    return;
                }
                this._outerlength = endnode._outerstartindex + endnode._outerlength - this._outerstartindex;
            }
        }
        private static IEnumerable<string> GetClasses(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ')
                {
                    int num = (i == str.Length - 1) ? -1 : str.IndexOf(' ', i + 1);
                    if (num == -1)
                    {
                        yield return str.SubstringCached(i);
                        break;
                    }
                    yield return str.SubstringCached(i, num - i);
                    i = num;
                }
            }
            yield break;
        }
        public bool HasClass(string @class)
        {
            string attributeValue = this.GetAttributeValue("class");
            if (string.IsNullOrEmpty(attributeValue))
            {
                return false;
            }
#if SALTARELLE
            // avoids infinite loop later
            if (@class.Length == 0) return false;
            var idx = 0;
            while (true)
            {
                idx = attributeValue.IndexOf(@class, idx);
                if (idx == -1) return false;
                var before = attributeValue.CharCodeAt(idx - 1);
                var after = attributeValue.CharCodeAt(idx + @class.Length);
                if (
                    (before == ' ' || double.IsNaN((int)before)) &&
                    (after == ' ' || double.IsNaN((int)after))
                    ) return true;
                idx += @class.Length; 
            }
#else
            if (attributeValue.IndexOf(' ') != -1)
            {

                for (int i = 0; i < attributeValue.Length; i++)
                {
                    if (attributeValue[i] != ' ')
                    {
                        int num = (i == attributeValue.Length - 1) ? -1 : attributeValue.IndexOf(' ', i + 1);
                        if (num == -1)
                        {
                            if (attributeValue.AsValueString().Substring(i) == @class) return true;
                            break;
                        }
                        if (attributeValue.AsValueString().Substring(i, num - i) == @class) return true;
                        i = num;
                    }
                }

                return false;
            }
            return attributeValue == @class;
        }
        internal void WriteAttribute(TextWriter outText, HtmlAttribute att)
        {
            string text = (att.QuoteType == AttributeValueQuote.DoubleQuote) ? "\"" : "'";
            string text2;
            if (this._ownerdocument.OptionOutputAsXml)
            {
                text2 = (this._ownerdocument.OptionOutputUpperCase ? att.XmlName.ToUpper() : att.XmlName);
                if (this._ownerdocument.OptionOutputOriginalCase)
                {
                    text2 = att.OriginalName;
                }
                outText.Write(string.Concat(new string[]
                {
                    " ",
                    text2,
                    "=",
                    text,
                    HtmlEntity.Entitize(att.XmlValue),
                    text
                }));
                return;
            }
            text2 = (this._ownerdocument.OptionOutputUpperCase ? att.Name.ToUpper() : (this.OwnerDocument.OptionOutputOriginalCase ? att.OriginalName : att.Name));
            if (!this._ownerdocument.OptionOutputOptimizeAttributeValues)
            {
                outText.Write(string.Concat(new string[]
                {
                    " ",
                    text2,
                    "=",
                    text,
                    att.Value,
                    text
                }));
                return;
            }
            if (att.Value.IndexOfAny(new char[]
            {
                '\n',
                '\r',
                '\t',
                ' '
            }) < 0)
            {
                outText.Write(" " + text2 + "=" + att.Value);
                return;
            }
            outText.Write(string.Concat(new string[]
            {
                " ",
                text2,
                "=",
                text,
                att.Value,
                text
            }));
        }
        internal void WriteAttributes(TextWriter outText, bool closing)
        {
            if (this._ownerdocument.OptionOutputAsXml)
            {
                foreach (HtmlAttribute current in this.Attributes)
                {
                    this.WriteAttribute(outText, current);
                }
                return;
            }
            if (!closing)
            {
                if (this._attributeCount != 0)
                {
                    foreach (HtmlAttribute current2 in this.Attributes)
                    {
                        this.WriteAttribute(outText, current2);
                    }
                }
                if (!this._ownerdocument.OptionAddDebuggingAttributes)
                {
                    return;
                }
                this.WriteAttribute(outText, this._ownerdocument.CreateAttribute("_closed", this.Closed.ToString()));
                this.WriteAttribute(outText, this._ownerdocument.CreateAttribute("_children", this.ChildNodes.Count.ToString()));
                int num = 0;
                foreach (HtmlNode current3 in this.ChildNodes)
                {
                    this.WriteAttribute(outText, this._ownerdocument.CreateAttribute("_child_" + num, current3.TagName));
                    num++;
                }
            }
        }
    }
}
