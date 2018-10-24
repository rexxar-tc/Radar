using Sandbox.ModAPI;
using VRage.Utils;

namespace RadarMod.ModSettings
{
    public class Settings
    {
        private const string SETTINGS_FILE = "RadarSettings.xml";
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        public SessionSettings Session { get; private set; }

        public Settings()
        {
            Load();
        }

        public void Load()
        {
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(SETTINGS_FILE, typeof(RadarCore)))
            {
                Session = new SessionSettings();
                Session.SettingsChanged += Session_SettingsChanged;
                Save();
                return;
            }
            var f = MyAPIGateway.Utilities.ReadFileInWorldStorage(SETTINGS_FILE, typeof(RadarCore));
            Session = MyAPIGateway.Utilities.SerializeFromXML<SessionSettings>(f.ReadToEnd());
            Session.SettingsChanged += Session_SettingsChanged;
        }

        public void ConsumeSync(SessionSettings settings)
        {
            Session = settings;
            Save();
        }

        private void Session_SettingsChanged()
        {
            Save();
        }

        public void Save()
        {
            if (Session == null)
                return;
            
            string s = MyAPIGateway.Utilities.SerializeToXML(Session);
            using (var w = MyAPIGateway.Utilities.WriteFileInWorldStorage(SETTINGS_FILE, typeof(RadarCore)))
            {
                w.Write(s);
                w.Flush();
            }
        }
    }
}
