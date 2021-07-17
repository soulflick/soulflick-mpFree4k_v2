using System;
using System.Collections.Generic;

namespace MpFree4k.Utilities
{
    public static class LinqUtils
    {
        public static IEnumerable<TSource> UniqueBy<TSource, TKey>(this IEnumerable<TSource> src, Func<TSource, TKey> key)
        {
            HashSet<TKey> res = new HashSet<TKey>();
            foreach (TSource e in src)
            {
                TKey k = key(e);
                if (res.Contains(k))
                    continue;
                res.Add(k);
                yield return e;
            }
        }
    }
}
