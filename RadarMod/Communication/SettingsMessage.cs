using System.Collections.Generic;
using ProtoBuf;
using RadarMod.ModSettings;
using Sandbox.ModAPI;

namespace RadarMod.Communication
{
    [ProtoContract]
    public class SettingsMessage : Message
    {
        [ProtoMember(301)]
        private SessionSettings _settings;

        public SettingsMessage(SessionSettings settings)
        {
            _settings = settings;
        }

        public SettingsMessage()
        {

        }

        public override void HandleServer()
        {
        }

        public override void HandleClient()
        {
            Settings.Instance.ConsumeSync(_settings);
            RadarCore.SetBatteryDefinitions();
        }
    }
}
