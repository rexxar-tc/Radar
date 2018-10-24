using System.Collections.Generic;
using System.Linq;

namespace RadarMod.Utility
{
    public static class Extensions
    {
        public static bool Contains<T>(this IEnumerable<T> input, T query)
        {
            return Enumerable.Contains(input, query);
        }
    }
}
