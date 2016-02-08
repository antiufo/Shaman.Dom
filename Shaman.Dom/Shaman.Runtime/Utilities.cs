using System;
using System.Collections.Generic;
namespace Shaman.Runtime
{
	internal static class Utilities
	{
#if SALTARELLE
        internal static bool ContainsIndex(this string str, int index)
		{
			return index < str.Length;
		}

        internal static TValue TryGetValue<TKey, TValue>(this JsDictionary<TKey, TValue> dict, TKey key)
        {
            return dict[key];
        }
#endif
        internal static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
		{
			TValue result;
			dict.TryGetValue(key, out result);
			return result;
		}

	}
}
