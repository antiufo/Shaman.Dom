using Shaman.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
#if SALTARELLE
using System.Runtime.CompilerServices;
using StringBuilder = System.Text.Saltarelle.StringBuilder;
using IntToStringDictionary = System.Collections.Generic.JsDictionary<int, string>;
using StringToIntDictionary = System.Collections.Generic.JsDictionary<string, int>;
#else
using IntToStringDictionary = System.Collections.Generic.Dictionary<int, string>;
using StringToIntDictionary = System.Collections.Generic.Dictionary<string, int>;
#endif
namespace Shaman.Dom
{
    public class HtmlEntity
    {
        private enum ParseState
        {
            Text,
            EntityStart
        }
        private static readonly int _maxEntitySize;
        private static IntToStringDictionary _entityName;
        private static StringToIntDictionary _entityValue;
#if SALTARELLE
        private static StringBuilder sb;
#endif
#if SALTARELLE
        private static StringBuilder entity;
#endif

        static HtmlEntity()
        {
#if SALTARELLE
            HtmlEntity._entityName = new JsDictionary<int, string>();
            HtmlEntity._entityValue = new JsDictionary<string, int>();
#else
            HtmlEntity._entityName = new Dictionary<int, string>();
            HtmlEntity._entityValue = new Dictionary<string, int>();
#endif
            HtmlEntity.AddPair("nbsp", 160);
            HtmlEntity.AddPair("iexcl", 161);
            HtmlEntity.AddPair("cent", 162);
            HtmlEntity.AddPair("pound", 163);
            HtmlEntity.AddPair("curren", 164);
            HtmlEntity.AddPair("yen", 165);
            HtmlEntity.AddPair("brvbar", 166);
            HtmlEntity.AddPair("sect", 167);
            HtmlEntity.AddPair("uml", 168);
            HtmlEntity.AddPair("copy", 169);
            HtmlEntity.AddPair("ordf", 170);
            HtmlEntity.AddPair("laquo", 171);
            HtmlEntity.AddPair("not", 172);
            HtmlEntity.AddPair("shy", 173);
            HtmlEntity.AddPair("reg", 174);
            HtmlEntity.AddPair("macr", 175);
            HtmlEntity.AddPair("deg", 176);
            HtmlEntity.AddPair("plusmn", 177);
            HtmlEntity.AddPair("sup2", 178);
            HtmlEntity.AddPair("sup3", 179);
            HtmlEntity.AddPair("acute", 180);
            HtmlEntity.AddPair("micro", 181);
            HtmlEntity.AddPair("para", 182);
            HtmlEntity.AddPair("middot", 183);
            HtmlEntity.AddPair("cedil", 184);
            HtmlEntity.AddPair("sup1", 185);
            HtmlEntity.AddPair("ordm", 186);
            HtmlEntity.AddPair("raquo", 187);
            HtmlEntity.AddPair("frac14", 188);
            HtmlEntity.AddPair("frac12", 189);
            HtmlEntity.AddPair("frac34", 190);
            HtmlEntity.AddPair("iquest", 191);
            HtmlEntity.AddPair("Agrave", 192);
            HtmlEntity.AddPair("Aacute", 193);
            HtmlEntity.AddPair("Acirc", 194);
            HtmlEntity.AddPair("Atilde", 195);
            HtmlEntity.AddPair("Auml", 196);
            HtmlEntity.AddPair("Aring", 197);
            HtmlEntity.AddPair("AElig", 198);
            HtmlEntity.AddPair("Ccedil", 199);
            HtmlEntity.AddPair("Egrave", 200);
            HtmlEntity.AddPair("Eacute", 201);
            HtmlEntity.AddPair("Ecirc", 202);
            HtmlEntity.AddPair("Euml", 203);
            HtmlEntity.AddPair("Igrave", 204);
            HtmlEntity.AddPair("Iacute", 205);
            HtmlEntity.AddPair("Icirc", 206);
            HtmlEntity.AddPair("Iuml", 207);
            HtmlEntity.AddPair("ETH", 208);
            HtmlEntity.AddPair("Ntilde", 209);
            HtmlEntity.AddPair("Ograve", 210);
            HtmlEntity.AddPair("Oacute", 211);
            HtmlEntity.AddPair("Ocirc", 212);
            HtmlEntity.AddPair("Otilde", 213);
            HtmlEntity.AddPair("Ouml", 214);
            HtmlEntity.AddPair("times", 215);
            HtmlEntity.AddPair("Oslash", 216);
            HtmlEntity.AddPair("Ugrave", 217);
            HtmlEntity.AddPair("Uacute", 218);
            HtmlEntity.AddPair("Ucirc", 219);
            HtmlEntity.AddPair("Uuml", 220);
            HtmlEntity.AddPair("Yacute", 221);
            HtmlEntity.AddPair("THORN", 222);
            HtmlEntity.AddPair("szlig", 223);
            HtmlEntity.AddPair("agrave", 224);
            HtmlEntity.AddPair("aacute", 225);
            HtmlEntity.AddPair("acirc", 226);
            HtmlEntity.AddPair("atilde", 227);
            HtmlEntity.AddPair("auml", 228);
            HtmlEntity.AddPair("aring", 229);
            HtmlEntity.AddPair("aelig", 230);
            HtmlEntity.AddPair("ccedil", 231);
            HtmlEntity.AddPair("egrave", 232);
            HtmlEntity.AddPair("eacute", 233);
            HtmlEntity.AddPair("ecirc", 234);
            HtmlEntity.AddPair("euml", 235);
            HtmlEntity.AddPair("igrave", 236);
            HtmlEntity.AddPair("iacute", 237);
            HtmlEntity.AddPair("icirc", 238);
            HtmlEntity.AddPair("iuml", 239);
            HtmlEntity.AddPair("eth", 240);
            HtmlEntity.AddPair("ntilde", 241);
            HtmlEntity.AddPair("ograve", 242);
            HtmlEntity.AddPair("oacute", 243);
            HtmlEntity.AddPair("ocirc", 244);
            HtmlEntity.AddPair("otilde", 245);
            HtmlEntity.AddPair("ouml", 246);
            HtmlEntity.AddPair("divide", 247);
            HtmlEntity.AddPair("oslash", 248);
            HtmlEntity.AddPair("ugrave", 249);
            HtmlEntity.AddPair("uacute", 250);
            HtmlEntity.AddPair("ucirc", 251);
            HtmlEntity.AddPair("uuml", 252);
            HtmlEntity.AddPair("yacute", 253);
            HtmlEntity.AddPair("thorn", 254);
            HtmlEntity.AddPair("yuml", 255);
            HtmlEntity.AddPair("fnof", 402);
            HtmlEntity.AddPair("Alpha", 913);
            HtmlEntity.AddPair("Beta", 914);
            HtmlEntity.AddPair("Gamma", 915);
            HtmlEntity.AddPair("Delta", 916);
            HtmlEntity.AddPair("Epsilon", 917);
            HtmlEntity.AddPair("Zeta", 918);
            HtmlEntity.AddPair("Eta", 919);
            HtmlEntity.AddPair("Theta", 920);
            HtmlEntity.AddPair("Iota", 921);
            HtmlEntity.AddPair("Kappa", 922);
            HtmlEntity.AddPair("Lambda", 923);
            HtmlEntity.AddPair("Mu", 924);
            HtmlEntity.AddPair("Nu", 925);
            HtmlEntity.AddPair("Xi", 926);
            HtmlEntity.AddPair("Omicron", 927);
            HtmlEntity.AddPair("Pi", 928);
            HtmlEntity.AddPair("Rho", 929);
            HtmlEntity.AddPair("Sigma", 931);
            HtmlEntity.AddPair("Tau", 932);
            HtmlEntity.AddPair("Upsilon", 933);
            HtmlEntity.AddPair("Phi", 934);
            HtmlEntity.AddPair("Chi", 935);
            HtmlEntity.AddPair("Psi", 936);
            HtmlEntity.AddPair("Omega", 937);
            HtmlEntity.AddPair("alpha", 945);
            HtmlEntity.AddPair("beta", 946);
            HtmlEntity.AddPair("gamma", 947);
            HtmlEntity.AddPair("delta", 948);
            HtmlEntity.AddPair("epsilon", 949);
            HtmlEntity.AddPair("zeta", 950);
            HtmlEntity.AddPair("eta", 951);
            HtmlEntity.AddPair("theta", 952);
            HtmlEntity.AddPair("iota", 953);
            HtmlEntity.AddPair("kappa", 954);
            HtmlEntity.AddPair("lambda", 955);
            HtmlEntity.AddPair("mu", 956);
            HtmlEntity.AddPair("nu", 957);
            HtmlEntity.AddPair("xi", 958);
            HtmlEntity.AddPair("omicron", 959);
            HtmlEntity.AddPair("pi", 960);
            HtmlEntity.AddPair("rho", 961);
            HtmlEntity.AddPair("sigmaf", 962);
            HtmlEntity.AddPair("sigma", 963);
            HtmlEntity.AddPair("tau", 964);
            HtmlEntity.AddPair("upsilon", 965);
            HtmlEntity.AddPair("phi", 966);
            HtmlEntity.AddPair("chi", 967);
            HtmlEntity.AddPair("psi", 968);
            HtmlEntity.AddPair("omega", 969);
            HtmlEntity.AddPair("thetasym", 977);
            HtmlEntity.AddPair("upsih", 978);
            HtmlEntity.AddPair("piv", 982);
            HtmlEntity.AddPair("bull", 8226);
            HtmlEntity.AddPair("hellip", 8230);
            HtmlEntity.AddPair("prime", 8242);
            HtmlEntity.AddPair("Prime", 8243);
            HtmlEntity.AddPair("oline", 8254);
            HtmlEntity.AddPair("frasl", 8260);
            HtmlEntity.AddPair("weierp", 8472);
            HtmlEntity.AddPair("image", 8465);
            HtmlEntity.AddPair("real", 8476);
            HtmlEntity.AddPair("trade", 8482);
            HtmlEntity.AddPair("alefsym", 8501);
            HtmlEntity.AddPair("larr", 8592);
            HtmlEntity.AddPair("uarr", 8593);
            HtmlEntity.AddPair("rarr", 8594);
            HtmlEntity.AddPair("darr", 8595);
            HtmlEntity.AddPair("harr", 8596);
            HtmlEntity.AddPair("crarr", 8629);
            HtmlEntity.AddPair("lArr", 8656);
            HtmlEntity.AddPair("uArr", 8657);
            HtmlEntity.AddPair("rArr", 8658);
            HtmlEntity.AddPair("dArr", 8659);
            HtmlEntity.AddPair("hArr", 8660);
            HtmlEntity.AddPair("forall", 8704);
            HtmlEntity.AddPair("part", 8706);
            HtmlEntity.AddPair("exist", 8707);
            HtmlEntity.AddPair("empty", 8709);
            HtmlEntity.AddPair("nabla", 8711);
            HtmlEntity.AddPair("isin", 8712);
            HtmlEntity.AddPair("notin", 8713);
            HtmlEntity.AddPair("ni", 8715);
            HtmlEntity.AddPair("prod", 8719);
            HtmlEntity.AddPair("sum", 8721);
            HtmlEntity.AddPair("minus", 8722);
            HtmlEntity.AddPair("lowast", 8727);
            HtmlEntity.AddPair("radic", 8730);
            HtmlEntity.AddPair("prop", 8733);
            HtmlEntity.AddPair("infin", 8734);
            HtmlEntity.AddPair("ang", 8736);
            HtmlEntity.AddPair("and", 8743);
            HtmlEntity.AddPair("or", 8744);
            HtmlEntity.AddPair("cap", 8745);
            HtmlEntity.AddPair("cup", 8746);
            HtmlEntity.AddPair("int", 8747);
            HtmlEntity.AddPair("there4", 8756);
            HtmlEntity.AddPair("sim", 8764);
            HtmlEntity.AddPair("cong", 8773);
            HtmlEntity.AddPair("asymp", 8776);
            HtmlEntity.AddPair("ne", 8800);
            HtmlEntity.AddPair("equiv", 8801);
            HtmlEntity.AddPair("le", 8804);
            HtmlEntity.AddPair("ge", 8805);
            HtmlEntity.AddPair("sub", 8834);
            HtmlEntity.AddPair("sup", 8835);
            HtmlEntity.AddPair("nsub", 8836);
            HtmlEntity.AddPair("sube", 8838);
            HtmlEntity.AddPair("supe", 8839);
            HtmlEntity.AddPair("oplus", 8853);
            HtmlEntity.AddPair("otimes", 8855);
            HtmlEntity.AddPair("perp", 8869);
            HtmlEntity.AddPair("sdot", 8901);
            HtmlEntity.AddPair("lceil", 8968);
            HtmlEntity.AddPair("rceil", 8969);
            HtmlEntity.AddPair("lfloor", 8970);
            HtmlEntity.AddPair("rfloor", 8971);
            HtmlEntity.AddPair("lang", 9001);
            HtmlEntity.AddPair("rang", 9002);
            HtmlEntity.AddPair("loz", 9674);
            HtmlEntity.AddPair("spades", 9824);
            HtmlEntity.AddPair("clubs", 9827);
            HtmlEntity.AddPair("hearts", 9829);
            HtmlEntity.AddPair("diams", 9830);
            HtmlEntity.AddPair("quot", 34);
            HtmlEntity.AddPair("amp", 38);
            HtmlEntity.AddPair("lt", 60);
            HtmlEntity.AddPair("gt", 62);
            HtmlEntity.AddPair("OElig", 338);
            HtmlEntity.AddPair("oelig", 339);
            HtmlEntity.AddPair("Scaron", 352);
            HtmlEntity.AddPair("scaron", 353);
            HtmlEntity.AddPair("Yuml", 376);
            HtmlEntity.AddPair("circ", 710);
            HtmlEntity.AddPair("tilde", 732);
            HtmlEntity.AddPair("ensp", 8194);
            HtmlEntity.AddPair("emsp", 8195);
            HtmlEntity.AddPair("thinsp", 8201);
            HtmlEntity.AddPair("zwnj", 8204);
            HtmlEntity.AddPair("zwj", 8205);
            HtmlEntity.AddPair("lrm", 8206);
            HtmlEntity.AddPair("rlm", 8207);
            HtmlEntity.AddPair("ndash", 8211);
            HtmlEntity.AddPair("mdash", 8212);
            HtmlEntity.AddPair("lsquo", 8216);
            HtmlEntity.AddPair("rsquo", 8217);
            HtmlEntity.AddPair("sbquo", 8218);
            HtmlEntity.AddPair("ldquo", 8220);
            HtmlEntity.AddPair("rdquo", 8221);
            HtmlEntity.AddPair("bdquo", 8222);
            HtmlEntity.AddPair("dagger", 8224);
            HtmlEntity.AddPair("Dagger", 8225);
            HtmlEntity.AddPair("permil", 8240);
            HtmlEntity.AddPair("lsaquo", 8249);
            HtmlEntity.AddPair("rsaquo", 8250);
            HtmlEntity.AddPair("euro", 8364);
            HtmlEntity._maxEntitySize = 9;
        }
        private static void AddPair(string name, int val)
        {
            HtmlEntity._entityName[val] = name;
            HtmlEntity._entityValue[name] = val;
        }
        private HtmlEntity()
        {
        }
        public static string DeEntitize(string text)
        {
            if (text == null)
            {
                return null;
            }
            if (text.Length == 0)
            {
                return text;
            }
#if !SALTARELLE
            StringBuilder entity = null;
            StringBuilder sb = null;
#endif
            bool flag = false;
            HtmlEntity.ParseState parseState = HtmlEntity.ParseState.Text;
            for (int i = 0; i < text.Length; i++)
            {
                switch (parseState)
                {
                    case HtmlEntity.ParseState.Text:
                        {
                            char c = text[i];
                            if (c == '&')
                            {
                                parseState = HtmlEntity.ParseState.EntityStart;
                            }
                            else
                            {
                                if (flag)
                                {
                                    sb.Append(text[i]);
                                }
                            }
                            break;
                        }
                    case HtmlEntity.ParseState.EntityStart:
                        {
                            if (!flag)
                            {
                                flag = true;
                                if (sb == null)
                                {
#if SALTARELLE
                                    sb = new StringBuilder(text.Length);
#else
                                    sb = ReseekableStringBuilder.AcquirePooledStringBuilder();
#endif
                                }
                                else
                                {
                                    sb.Length = 0;
                                }
                                sb.Append(text, 0, i - 1);
                                if (entity == null)
                                {
#if SALTARELLE
                                    entity = new StringBuilder(10);
#else
                                    entity = ReseekableStringBuilder.AcquirePooledStringBuilder();
#endif
                                }
                                entity.Length = 0;
                            }
                            char c2 = text[i];
                            if (c2 == ';' || c2 == '<' || c2 == '"' || c2 == '\'' || HtmlEntity.IsWhiteSpace(c2))
                            {
                                bool flag2 = c2 == ';';
                                if (!flag2)
                                {
                                    i--;
                                }
                                if (entity.Length == 0)
                                {
                                    if (c2 == ';')
                                    {
                                        sb.Append("&;");
                                    }
                                    else
                                    {
                                        sb.Append('&');
                                    }
                                }
                                else
                                {
                                    if (entity[0] == '#')
                                    {
                                        string text2 = entity.ToString();
                                        try
                                        {
                                            string text3 = text2.SubstringCached(1).Trim().ToLowerFast();
                                            int fromBase;
                                            if (text3.StartsWith("x"))
                                            {
                                                fromBase = 16;
                                                text3 = text3.SubstringCached(1);
                                            }
                                            else
                                            {
                                                fromBase = 10;
                                            }
#if SALTARELLE
                                            int num = int.Parse(text3, fromBase);
#else
                                    int num = Convert.ToInt32(text3, fromBase);
#endif
                                            if (num > 65535)
                                            {
#if SALTARELLE
                                                sb.Append(StringFromCodePoint(num));
#else
                                                sb.Append(char.ConvertFromUtf32(num));
#endif
                                            }
                                            else
                                            {
                                                sb.Append((char)num);
                                            }
                                            goto IL_235;
                                        }
                                        catch
                                        {
                                            sb.Append("&#" + text2 + ";");
                                            goto IL_235;
                                        }
                                        goto IL_1DC;
                                    }
                                    goto IL_1DC;
                                    IL_235:
                                    entity.Remove(0, entity.Length);
                                    goto IL_24B;
                                    IL_1DC:
                                    int num2;
#if SALTARELLE
                                    num2 = HtmlEntity._entityValue[entity.ToStringCached()];
                                    if (!Script.IsNullOrUndefined(num2))
                                    {
                                        sb.Append((char)num2);
                                        goto IL_235;
                                    }
#else
                                    if (HtmlEntity._entityValue.TryGetValue(entity.ToStringCached(), out num2))
                                    {
                                        int num3 = num2;
                                        sb.Append((char)num3);
                                        goto IL_235;
                                    }
#endif
                                    sb.Append('&');
                                    sb.Append(entity);
                                    if (flag2)
                                    {
                                        sb.Append(';');
                                        goto IL_235;
                                    }
                                    goto IL_235;
                                }
                                IL_24B:
                                parseState = HtmlEntity.ParseState.Text;
                            }
                            else
                            {
                                if (c2 == '&')
                                {
                                    sb.Append("&" + entity);
                                    entity.Remove(0, entity.Length);
                                }
                                else
                                {
                                    entity.Append(text[i]);
                                    if (entity.Length > HtmlEntity._maxEntitySize)
                                    {
                                        parseState = HtmlEntity.ParseState.Text;
                                        sb.Append("&" + entity);
                                        entity.Remove(0, entity.Length);
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            if (parseState == HtmlEntity.ParseState.EntityStart && flag)
            {
                sb.Append("&" + entity);
            }
            if (!flag)
            {
                return text;
            }
            return sb.ToStringCached();
        }
        private static bool IsWhiteSpace(char ch)
        {
#if SALTARELLE
            return ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t';
#else
            return char.IsWhiteSpace(ch);
#endif
        }

#if SALTARELLE
        [InlineCode("{$System.String}.fromCodePoint({code})")]
        private static string StringFromCodePoint(int code)
        {
            return null;
        }
#endif

        public static string Entitize(string text)
        {
            if (text == null)
            {
                return null;
            }
#if !SALTARELLE
            StringBuilder sb = null;
#endif
            bool flag = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '&' || c == '<' || c == '>' || c == '"' || c == '\'')
                {
                    if (!flag)
                    {
                        flag = true;
                        if (sb == null)
                        {
                            sb = new StringBuilder(text.Length + text.Length / 5 + 10);
                        }
                        else
                        {
                            sb.Length = 0;
                        }
                        sb.Append(text, 0, i);
                    }
                    if (c == '&')
                    {
                        sb.Append("&amp;");
                    }
                    else if (c == '<')
                    {
                        sb.Append("&lt;");
                    }
                    else if (c == '>')
                    {
                        sb.Append("&gt;");
                    }
                    else if (c == '"')
                    {
                        sb.Append("&quot;");
                    }
                    else if (c == '\'')
                    {
                        sb.Append("&apos;");
                    }
                }
                else
                {
                    if (flag)
                    {
                        sb.Append(c);
                    }
                }
            }
            if (!flag)
            {
                return text;
            }
            return sb.ToStringCached();
        }
    }
}
