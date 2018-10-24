using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RadarMod.Communication;
using RadarMod.ModSettings;
using RadarMod.Utility;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace RadarMod
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), false, "RX_Radar")]
    public class RadarComponent : MyGameLogicComponent
    {
        private static bool _init;
        private IMyBatteryBlock _block;
        private DateTime _scanStart;
        private long? _requestingId;
        private static Dictionary<long, Dictionary<long, IMyGps>> _gpsCache = new Dictionary<long, Dictionary<long, IMyGps>>();
        private Dictionary<long, IMyGps> _localCache = new Dictionary<long, IMyGps>();

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();

            _block = Entity as IMyBatteryBlock;
            if (_block == null)
                return;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            _block.OnlyRecharge = true;

            if (_init)
                return;

            _init = true;
            CreateTerminalControls();
            _dish = _block.GetSubpart("radar");
        }
        
        private MyEntitySubpart _dish;

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (_dish == null)
            {
                return;
            }

            _dish.PositionComp.LocalMatrix = Matrix.CreateRotationY(0.01f) * _dish.PositionComp.LocalMatrix;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (DateTime.Now - _scanStart < TimeSpan.FromSeconds(Settings.Instance.Session.ScanningVisibleSeconds))
                return;

            NeedsUpdate &= ~MyEntityUpdateEnum.EACH_100TH_FRAME;
            DoScan(_block);
        }

        private void CreateTerminalControls()
        {
            TerminalHelpers.AddButton(_block, "Radar_Scan", "Scan", "Scan", RequestScan, ButtonEnabledGetter);
            TerminalHelpers.AddAction(_block, "Radar_DoScan", "Begin scan", RequestScan, ButtonEnabledGetter, @"Textures\GUI\Icons\Actions\Start.dds");
            TerminalHelpers.AddProperty(_block, "Radar_ScanResults", (b) => b.GameLogic.GetAs<RadarComponent>()?._lastScanResults, (b, v) => { });


            MyAPIGateway.TerminalControls.CustomControlGetter += TerminalControls_CustomControlGetter;
            MyAPIGateway.TerminalControls.CustomActionGetter += TerminalControls_CustomActionGetter;
        }

        private bool ButtonEnabledGetter(IMyTerminalBlock block)
        {
            var b = block as IMyBatteryBlock;
            var c = b?.GameLogic.GetAs<RadarComponent>();
            if (c == null)
                return false;

            if (b.CurrentStoredPower < Settings.Instance.Session.EnergyForScanMWh)
                return false;

            return true;
        }

        private void TerminalControls_CustomActionGetter(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            var b = block as IMyBatteryBlock;
            if (b == null)
                return;

            if (b.GameLogic.GetAs<RadarComponent>() == null)
                return;

            actions.RemoveAll(c => c.Id == "Recharge" || c.Id == "Discharge" || c.Id == "SemiAuto");
        }

        private void TerminalControls_CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            var b = block as IMyBatteryBlock;
            if (b == null)
                return;

            if (b.GameLogic.GetAs<RadarComponent>() == null)
                return;

            controls.RemoveAll(c => c.Id == "Recharge" || c.Id == "Discharge" || c.Id == "SemiAuto");
        }

        private static void RequestScan(IMyTerminalBlock block)
        {
            Communication.Communication.SendMessageToServer(new ScanMessage(block.EntityId, MyAPIGateway.Session.Player.IdentityId));
        }

        public void StartScan(long requestingId)
        {
            //TODO: Validate request vs block settings, power, enabled
            if (!_block.IsWorking)
            {
                MyAPIGateway.Utilities.ShowMessage("radar", "block not working");
                return;
            }
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            _scanStart = DateTime.Now;
            _requestingId = requestingId;

            Utilities.SendGPS(_block.GetPosition(), "RADAR SCANNING", "", Color.Yellow, Settings.Instance.Session.ScanningVisibleSeconds,
                              _block.GetPosition(), Settings.Instance.Session.ScanningVisibleMaxDistance, Settings.Instance.Session.ScanningVisibleMinDistance);
        }

        private Dictionary<Vector3D, float> _lastScanResults = new Dictionary<Vector3D, float>();
        private Dictionary<Vector3D, float> _resultSwap = new Dictionary<Vector3D, float>();

        private void DoScan(IMyTerminalBlock block)
        {

            MyVisualScriptLogicProvider.RemoveGPSForAll("RADAR SCANNING");
            if (!_requestingId.HasValue)
                return;

            Dictionary<long, IMyGps> globalCache;
            lock (_gpsCache)
            {
                if (!_gpsCache.TryGetValue(_requestingId.Value, out globalCache))
                {
                    globalCache = new Dictionary<long, IMyGps>();
                    _gpsCache[_requestingId.Value] = globalCache;
                }
            }

            var sphere = new BoundingSphereD(block.GetPosition(), Settings.Instance.Session.MaxDistance);
            var scanned = new HashSet<IMyCubeGrid>();
            var groups = new List<List<IMyCubeGrid>>();
            List<MyEntity> results = new List<MyEntity>();

            MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, results);

            foreach (var entity in results)
            {
                var grid = entity as MyCubeGrid;
                if (grid == null)
                    continue;
                var igrid = (IMyCubeGrid)grid;

                //ignore grids in the group belonging to the radar
                if (MyAPIGateway.GridGroups.HasConnection(igrid, block.CubeGrid, GridLinkTypeEnum.Mechanical))
                    continue;

                var g = MyAPIGateway.GridGroups.GetGroup(igrid, GridLinkTypeEnum.Mechanical);

                //check if we've already scanned this group
                if (scanned.Contains(g[0]))
                    continue;

                scanned.UnionWith(g);
                groups.Add(g);
            }

            foreach (var group in groups)
            {
                int blockcount = 0;
                int maxval = 0;
                MyCubeGrid parent = null;

                foreach (var igrid in group)
                {
                    var grid = (MyCubeGrid)igrid;
                    var ct = grid.BlocksCount;
                    blockcount += ct;
                    if (ct > maxval)
                    {
                        maxval = ct;
                        parent = grid;
                    }
                }

                if (parent == null)
                    continue;

                if (blockcount < Settings.Instance.Session.MinBlocksVisible)
                {
                    //MyAPIGateway.Utilities.ShowMessage(grid.DisplayName, "Max blocks");
                    continue;
                }

                if (Settings.Instance.Session.MaxBlocksVisible > 0 && blockcount > Settings.Instance.Session.MaxBlocksVisible)
                {
                    //MyAPIGateway.Utilities.ShowMessage(grid.DisplayName, "Min blocks");
                    continue;
                }

                var c = parent.GameLogic.GetAs<RadarGridComponent>();
                if (c == null)
                {
                    //MyAPIGateway.Utilities.ShowMessage(grid.DisplayName, "Component");
                    continue;
                }

                var p = c.Distributor.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId);
                if (p < Settings.Instance.Session.MinPowerVisibleMW)
                {
                    //MyAPIGateway.Utilities.ShowMessage(grid.DisplayName, "Min power");
                    continue;
                }

                var d = p * Settings.Instance.Session.MWToKmMultiplier * 1000;
                var d2 = d * d;
                var d3 = Vector3D.DistanceSquared(parent.PositionComp.GetPosition(), block.GetPosition());
                if (d3 > d2)
                {
                    //MyAPIGateway.Utilities.ShowMessage(grid.DisplayName, $"Distance: {p} : {d} : {Math.Sqrt(d3)}");
                    continue;
                }

                var sbo = new StringBuilder();
                sbo.Append(parent.IsStatic ? "Station " : "Ship ");
                sbo.Append((short)parent.EntityId);
                sbo.Append(" : ");
                MyValueFormatter.AppendWorkInBestUnit(p, sbo);
                IMyGps g;
                if (globalCache.TryGetValue(parent.EntityId, out g))
                {
                    g.Name = sbo.ToString();
                    g.Coords = parent.PositionComp.GetPosition();
                }
                else
                    g = MyAPIGateway.Session.GPS.Create(sbo.ToString(), "Detected grid", parent.PositionComp.GetPosition(), true);

                _localCache[parent.EntityId] = g;

                _resultSwap.Add(parent.PositionComp.GetPosition(), p);
            }

            foreach (var pair in _localCache)
            {
                if (globalCache.ContainsKey(pair.Key))
                    MyAPIGateway.Session.GPS.ModifyGps(_requestingId.Value, pair.Value);
                else
                    MyAPIGateway.Session.GPS.AddGps(_requestingId.Value, pair.Value);
            }

            foreach (var pair in globalCache)
            {
                if (!_localCache.ContainsKey(pair.Key))
                    MyAPIGateway.Session.GPS.RemoveGps(_requestingId.Value, pair.Value);
            }

            lock (_gpsCache)
                MyUtils.Swap(ref globalCache, ref _localCache);
            _localCache.Clear();

            MyUtils.Swap(ref _resultSwap, ref _lastScanResults);
            _resultSwap.Clear();
        }
    }
}
