using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RadarMod.Communication;
using RadarMod.ModSettings;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace RadarMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class RadarCore : MySessionComponentBase
    {
        private static bool _init;
        public override void LoadData()
        {
            //if (_init || MyAPIGateway.Utilities == null)
             //   return;
            //_init = true;
            
            Communication.Communication.Register();

            if(!MyAPIGateway.Multiplayer.IsServer)
                Communication.Communication.SendMessageToServer(new RequestSettingsMessage());
            else
                SetBatteryDefinitions();
        }

        public static void SetBatteryDefinitions()
        {
            var d = MyDefinitionManager.Static.GetAllDefinitions();
            foreach (var def in d)
            {
                var b = def as MyBatteryBlockDefinition;
                if (b == null)
                    continue;

                if (!b.Id.SubtypeName.EndsWith("Radar"))
                    continue;

                b.MaxStoredPower = Settings.Instance.Session.EnergyForScanMWh* 1.2f; //slight margin of error
            }
}

        protected override void UnloadData()
        {
            base.UnloadData();
            Communication.Communication.Unregister();
        }
    }
}
