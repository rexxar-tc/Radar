using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using RadarMod.ModSettings;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace RadarMod.Communication
{
    [ProtoContract]
    public class ScanMessage : Message
    {
        [ProtoMember(201)]
        public long BlockID;
        [ProtoMember(202)]
        public long RequesterID;

        public ScanMessage(long blockId, long requesterId)
        {
            BlockID = blockId;
            RequesterID = requesterId;
        }

        public ScanMessage() { }

        public override void HandleServer()
        {
            IMyEntity entity;

            if (!MyAPIGateway.Entities.TryGetEntityById(BlockID, out entity))
                return;

            var c = entity.GameLogic.GetAs<RadarComponent>();
            if (c == null)
            {
                //MyAPIGateway.Utilities.ShowMessage("radar","no component");
                return;
            }

            var b = entity as MyBatteryBlock;
            if (b == null)
            {
                return;
            }
            
            if (b.CurrentStoredPower < Settings.Instance.Session.EnergyForScanMWh)
            {

                //MyAPIGateway.Utilities.ShowMessage("radar", "no power");
                return;
            }

            b.CurrentStoredPower -= Settings.Instance.Session.EnergyForScanMWh;

            c.StartScan(RequesterID);

            Communication.SendMessageToClients(this, true, MyAPIGateway.Multiplayer.MyId);
        }

        public override void HandleClient()
        {
            IMyEntity entity;

            if (!MyAPIGateway.Entities.TryGetEntityById(BlockID, out entity))
                return;

            var c = entity.GameLogic.GetAs<RadarComponent>();
            if (c == null)
                return;

            var b = entity as MyBatteryBlock;
            if (b == null)
                return;

            b.CurrentStoredPower -= Settings.Instance.Session.EnergyForScanMWh;
        }
    }
}
