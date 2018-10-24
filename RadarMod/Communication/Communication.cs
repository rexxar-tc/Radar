using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace RadarMod.Communication
{
    public static class Communication
    {
        private const ushort NET_ID = 15832;
        private static List<IMyPlayer> _playerCache = new List<IMyPlayer>();

        public static void Register()
        {
            MyAPIGateway.Multiplayer.RegisterMessageHandler(NET_ID, MessageHandler);
        }

        public static void Unregister()
        {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(NET_ID, MessageHandler);
            lock(_playerCache)
                _playerCache = null;
        }

        private static void MessageHandler(byte[] bytes)
        {
            Message m = MyAPIGateway.Utilities.SerializeFromBinary<Message>(bytes);

            if(MyAPIGateway.Multiplayer.IsServer)
                m.HandleServer();
            else
                m.HandleClient();
        }

        public static void SendMessageTo(ulong steamId, Message message, bool reliable = true)
        {
            var d = MyAPIGateway.Utilities.SerializeToBinary(message);
            if(!reliable && d.Length >= 1000)
                throw new Exception($"Attempting to send unreliable message beyond message size limits! Message type: {message.GetType()} Content: {string.Join(" ", d)}");
            MyAPIGateway.Multiplayer.SendMessageTo(NET_ID, d, steamId, reliable);
        }

        public static void SendMessageToServer(Message message, bool reliable = true)
        {
            var d = MyAPIGateway.Utilities.SerializeToBinary(message);
            if (!reliable && d.Length >= 1000)
                throw new Exception($"Attempting to send unreliable message beyond message size limits! Message type: {message.GetType()} Content: {string.Join(" ", d)}");
            MyAPIGateway.Multiplayer.SendMessageToServer(NET_ID, d, reliable);
        }

        public static void SendMessageToClients(Message message, bool reliable = true, params ulong[] ignore)
        {
            var d = MyAPIGateway.Utilities.SerializeToBinary(message);
            if (!reliable && d.Length >= 1000)
                throw new Exception($"Attempting to send unreliable message beyond message size limits! Message type: {message.GetType()} Content: {string.Join(" ", d)}");

            lock (_playerCache)
            {
                MyAPIGateway.Players.GetPlayers(_playerCache);
                foreach (var player in _playerCache)
                {
                    var steamId = player.SteamUserId;
                    if (ignore?.Contains(steamId) == true)
                        continue;
                    MyAPIGateway.Multiplayer.SendMessageTo(NET_ID, d, steamId, reliable);
                }
                _playerCache.Clear();
            }
        }

    }
}
