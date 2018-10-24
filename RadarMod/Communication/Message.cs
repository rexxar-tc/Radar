using ProtoBuf;
using Sandbox.ModAPI;

namespace RadarMod.Communication
{
    [ProtoInclude(1, typeof(ScanMessage))]
    [ProtoInclude(2, typeof(RequestSettingsMessage))]
    [ProtoInclude(3, typeof(SettingsMessage))]
    [ProtoContract]
    public abstract class Message
    {
        [ProtoMember]
        public ulong SenderId;

        public Message()
        {
            SenderId = MyAPIGateway.Multiplayer.MyId;
        }

        public abstract void HandleServer();
        public abstract void HandleClient();
    }
}
