using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RadarMod.Utility;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace RadarMod
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
    public class RadarGridComponent : MyGameLogicComponent
    {
        private MyResourceDistributorComponent _distributor;
        private HashSet<IMyCubeGrid> _groupCache;
        private bool _distributorDirty;

        public MyResourceDistributorComponent Distributor
        {
            get
            {
                if(_distributorDirty || _distributor == null)
                    _distributor = Utilities.GetDistributor(_grid);
                return _distributor;
            }
        }

        public HashSet<IMyCubeGrid> LogicalGroup => _groupCache;

        private MyCubeGrid _grid;
        public override void OnAddedToContainer()
        {
            _grid = Entity as MyCubeGrid;
            if (_grid == null)
                return;
            
            base.OnAddedToContainer();
            _groupCache = new HashSet<IMyCubeGrid>(MyAPIGateway.GridGroups.GetGroup(_grid, GridLinkTypeEnum.Logical));
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (Distributor == null) //grid not ready, try again later
            {
                NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                return;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            var g = MyAPIGateway.GridGroups.GetGroup(_grid, GridLinkTypeEnum.Logical);
            if (!Utilities.CompareSets(_groupCache, g))
            {
                _distributorDirty = true;
                _groupCache.Clear();
                _groupCache.UnionWith(g);
            }
            
            //MyAPIGateway.Utilities.ShowMessage(_grid.DisplayName, Distributor.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId).ToString() ?? "null");
        }

       
    }
}
