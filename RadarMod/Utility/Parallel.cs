using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;

namespace RadarMod.Utility
{
    public static class Parallel
    {
        public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
        {
            MyAPIGateway.Parallel.ForEach(collection, action);
        }
    }
}
