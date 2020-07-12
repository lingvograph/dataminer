using System.Collections.Generic;
using System.Linq;

namespace LingvoGraph
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> seq)
        {
            return seq ?? Enumerable.Empty<T>();
        }
    }
}