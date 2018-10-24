using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using RadarMod.ModSettings;

namespace RadarMod.Communication
{
    [ProtoContract]
    public class RequestSettingsMessage : Message
    {
        public RequestSettingsMessage()
        {
            
        }

        public override void HandleServer()
        {
            Communication.SendMessageTo(SenderId, new SettingsMessage(Settings.Instance.Session));
        }

        public override void HandleClient()
        {
        }
    }
}
