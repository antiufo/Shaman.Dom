# Shaman.Dom
An optimized version of HtmlAgilityPack with many bug fixes and improvements.

## Features
* Minimizes heap allocations and avoids duplicate strings in the heap
* `HtmlAttribute`, `HtmlAttributeCollection`, `HtmlNodeCollection` are now structs
* Proper encoding/unencoding of attributes
* Fixed allocation on each call of HtmlNode.get_TagName()
* Fixed imprecise parsing of `<form>` and `<table>`
* Fixed imprecise parsing of malformed entities
* Fixed imprecise parsing of extra closing tags
* `HtmlNode.HasClass()`, `HtmlNode.ClassList`
* Faster `Entitize()`, `DeEntitize()`
* Faster `Descendants()`/`DescendantsAndSelf()`
* Support for unicode surrogates
* Removed legacy stuff
* Support for lazily switching document encoding when `<meta charset>` is found
* Support for .NET Standard
* `HtmlDocument.Tag` for storing arbitrary data
* `HtmlDocument.PageUrl`, `HtmlDocument.BaseUrl` 
