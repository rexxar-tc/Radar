using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RadarMod.Utility
{
    public static class Utilities
    {
        private const string OB = @"<?xml version=""1.0"" encoding=""utf-16""?>
    <MyObjectBuilder_Cockpit xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
        <SubtypeName>LargeBlockCockpit</SubtypeName>
        <Owner>0</Owner>
        <CustomName>Control Stations</CustomName>
    </MyObjectBuilder_Cockpit> ";

        private static Random _random = new Random();

        /// <summary>
        /// Hacky way to get the ResourceDistributorComponent from a grid
        /// without benefit of the GridSystems.
        /// <para>Unfriendly to performance. Use sparingly and cache result.</para>
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static MyResourceDistributorComponent GetDistributor(MyCubeGrid grid)
        {
            try
            {
                if (grid == null || !grid.CubeBlocks.Any())
                    return null;

                //attempt to grab the distributor from an extant ship controller
                var controller = grid.GetFatBlocks().FirstOrDefault(b => (b as MyShipController)?.GridResourceDistributor != null);
                if (controller != null)
                    return ((MyShipController)controller).GridResourceDistributor;

                //didn't find a controller, so let's make one

                //var ob = MyAPIGateway.Utilities.SerializeFromXML<MyObjectBuilder_Cockpit>(OB);
                var ob = new MyObjectBuilder_Cockpit()
                         {
                             SubtypeName = "LargeBlockCockpit",
                             Owner = 0,
                             CustomName = "."
                         };
                //assign a random entity ID and hope we don't get collisions
                ob.EntityId = _random.Next(int.MinValue, int.MaxValue);
                //block position to something that will probably not have a block there already
                ob.Min = grid.WorldToGridInteger(grid.PositionComp.WorldAABB.Min) - new Vector3I(2);
                //note that this will slightly inflate the grid's boundingbox, but the Raze call later triggers a bounds recalc in 30 seconds

                //not exposed in the class but is in the interface???
                //also not synced
                var blk = ((IMyCubeGrid)grid).AddBlock(ob, false);
                var distributor = (blk.FatBlock as MyShipController)?.GridResourceDistributor;
                //hack to make it work on clients (removal not synced)
                grid.RazeBlocksClient(new List<Vector3I>() {blk.Position});
                //we don't need the block itself, we grabbed the distributor earlier
                blk.FatBlock?.Close();

                return distributor;
            }
            catch
            {
                return null;
            }
            
        }

        public static bool CompareSets<T>(IEnumerable<T> oldSet, IEnumerable<T> newSet)
        {
            if (oldSet.Count() != newSet.Count())
                return false;

            foreach (var t in newSet)
            {
                if (!oldSet.Contains(t))
                    return false;
            }

            return true;
        }

        public static bool CompareSets<T>(HashSet<T> compareTo, List<T> newSet)
        {
            if (compareTo.Count != newSet.Count)
                return false;

            foreach (var t in newSet)
            {
                if (!compareTo.Contains(t))
                    return false;
            }

            return true;
        }

        public static int RoundTo100(int input)
        {
            int num1 = input / 100;
            int num2 = num1 * 100;
            return Math.Max(100, num2);
        }

        public static void SendGPS(IMyGps point, Vector3D target, float maxDistance, float minDistance = 0)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                var d2 = Vector3D.DistanceSquared(target, player.GetPosition());
                if (d2 > maxDistance || d2 < minDistance)
                    continue;

                MyAPIGateway.Session.GPS.AddGps(player.IdentityId, point);
            }
        }

        public static void SendGPS(Vector3D point, string name, string description, Color color, int disappear, Vector3D target, float maxDistance, float minDistance = 0)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                var p = player.GetPosition();
                if (Vector3D.IsZero(p))
                    continue;

                var d2 = Vector3D.DistanceSquared(target,p );
                if (d2 > maxDistance || d2 < minDistance)
                    continue;

                MyVisualScriptLogicProvider.AddGPS(name, description, point, color, disappear);
            }
        }
    }
}
