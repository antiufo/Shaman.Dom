#if SALTARELLE
using System;
using System.Collections.Generic;
using System.Text;
using Shaman.Dom;
using Shaman;
using Shaman.Runtime;

namespace System
{
    public class Uri
    {
        public readonly string AbsoluteUri;
        public readonly string AbsolutePath;
        public readonly string Scheme;
        public readonly string Authority;
        public readonly string Host;
        public readonly string Query;

        public const string UriSchemeHttp = "http";
        public const string UriSchemeHttps = "https";

        public Uri(string url)
            : this(null, url)
        {
        }

        public Uri(Uri baseUrl, string url)
        {
            if (url == null) throw new ArgumentNullException();
            if (url.Length == 0 && baseUrl == null) throw new FormatException("Empty URL.");
            var protocolLength = 0;
            if (url.StartsWith("http://"))
            {
                this.Scheme = UriSchemeHttp;
                protocolLength = 7;
            }
            else if (url.StartsWith("https://"))
            {
                this.Scheme = UriSchemeHttps;
                protocolLength = 8;
            }
            else
            {

                for (int i = 0; i < url.Length; i++)
                {
                    var ch = url[i];
                    if (ch == ':')
                    {
                        this.Scheme = url.SubstringCached(0, i);
                        this.AbsoluteUri = url;
                        return;
                    }
                    if (!(IsLetterOrDigit(ch) || ch == '-' || ch == '+')) break;
                }

                if (baseUrl == null || baseUrl.Authority == null) throw new ArgumentException("A non-empty, http/https base URL is required when the provided URL is relative.");
                this.Scheme = baseUrl.Scheme;
                protocolLength = this.Scheme.Length + 3;

                if (url[0] == '/')
                {
                    url = baseUrl.Scheme + "://" + baseUrl.Authority + url;
                }
                else
                {
                    var lastSlash = baseUrl.AbsolutePath.LastIndexOf('/');
                    if (lastSlash == baseUrl.AbsolutePath.Length - 1)
                        url = baseUrl.Scheme + "://" + baseUrl.Authority + baseUrl.AbsolutePath + url;
                    else
                        url = baseUrl.Scheme + "://" + baseUrl.Authority + baseUrl.AbsolutePath.SubstringCached(0, lastSlash + 1) + url;
                }

            }

            this.AbsoluteUri = url;

            var slash = url.IndexOf('/', protocolLength);
            if (slash == -1)
            {
                Authority = url.SubstringCached(protocolLength);
                AbsolutePath = "/";
            }
            else
            {
                Authority = url.SubstringCached(protocolLength, slash - protocolLength);

                var q = url.IndexOf('?', slash);
                if (q != -1)
                {
                    Query = url.SubstringCached(q);
                    AbsolutePath = url.SubstringCached(slash, q - slash);
                }
                else
                {
                    AbsolutePath = url.SubstringCached(slash);
                }
            }


            var p = Authority.IndexOf(':');
            Host = p != -1 ? Authority.SubstringCached(0, p) : Authority;
        }

        private static bool IsLetterOrDigit(char ch)
        {
            return
                (ch >= '0' && ch <= '9') ||
                (ch >= 'a' && ch <= 'z') ||
                (ch >= 'A' && ch <= 'Z');
        }

        internal static string EscapeDataString(string value)
        {
            return System.Web.HttpUtility.UrlEncode(value);
        }

        internal static string UnescapeDataString(string p)
        {
            return System.Web.HttpUtility.UrlDecode(p);
        }

        public override string ToString()
        {
            return this.AbsoluteUri;
        }

        public string GetQueryParameter(string name)
        {
            var q = Query;
            if (q == null) return null;
            var list = q.JsSplit('&', 4096);
            for (int i = 0; i < list.Length; i++)
            {
                var k = list[i].JsSplit('=', 2);
                if (k.Length == 2)
                {
                    if (UnescapeDataString(k[0]) == name)
                        return UnescapeDataString(k[1]);
                }
            }
            return null;
        }
    }
}
#endif