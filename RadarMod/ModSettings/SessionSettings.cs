using System;
using ProtoBuf;

namespace RadarMod.ModSettings
{
    [Serializable]
    [ProtoContract]
    public class SessionSettings
    {
        private float _mwToKmMultiplier;
        private float _minPowerVisibleMw;
        private int _scanningVisibleSeconds;
        private float _scanningVisibleMaxDistance;
        private float _scanningVisibleMinDistance;
        private double _maxDistance;
        private uint _maxBlocksVisible;
        private uint _minBlocksVisible;
        private float _energyForScanMWh;
        private float _passiveScanRange;
        private bool _enablePassiveScan;
        private bool _enablePassiveCharacterDetection;
        private bool _enableActiveCharacterDetection;

        public event Action SettingsChanged;

        [ProtoMember]
        public float MWToKmMultiplier
        {
            get { return _mwToKmMultiplier; }
            set
            {
                _mwToKmMultiplier = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public float MinPowerVisibleMW
        {
            get { return _minPowerVisibleMw; }
            set
            {
                _minPowerVisibleMw = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public uint MinBlocksVisible
        {
            get { return _minBlocksVisible; }
            set
            {
                _minBlocksVisible = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public uint MaxBlocksVisible
        {
            get { return _maxBlocksVisible; }
            set
            {
                _maxBlocksVisible = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public double MaxDistance
        {
            get { return _maxDistance; }
            set
            {
                _maxDistance = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public int ScanningVisibleSeconds
        {
            get { return _scanningVisibleSeconds; }
            set
            {
                _scanningVisibleSeconds = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public float ScanningVisibleMinDistance
        {
            get { return _scanningVisibleMinDistance; }
            set
            {
                _scanningVisibleMinDistance = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public float ScanningVisibleMaxDistance
        {
            get { return _scanningVisibleMaxDistance; }
            set
            {
                _scanningVisibleMaxDistance = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public float EnergyForScanMWh
        {
            get { return _energyForScanMWh; }
            set
            {
                _energyForScanMWh = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public float PassiveScanRange
        {
            get { return _passiveScanRange; }
            set
            {
                _passiveScanRange = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public bool EnablePassiveScan
        {
            get { return _enablePassiveScan; }
            set
            {
                _enablePassiveScan = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public bool EnablePassiveCharacterDetection
        {
            get { return _enablePassiveCharacterDetection; }
            set
            {
                _enablePassiveCharacterDetection = value;
                RaiseSettingsChanged();
            }
        }

        [ProtoMember]
        public bool EnableActiveCharacterDetection
        {
            get { return _enableActiveCharacterDetection; }
            set
            {
                _enableActiveCharacterDetection = value;
                RaiseSettingsChanged();
            }
        }
        public SessionSettings()
        {
            MWToKmMultiplier = 1f;
            MinPowerVisibleMW = 15f;
            MinBlocksVisible = 1;
            MaxBlocksVisible = uint.MaxValue;
            MaxDistance = 5000000;
            ScanningVisibleSeconds = 30;
            EnergyForScanMWh = 3;
        }

        private void RaiseSettingsChanged()
        {
            SettingsChanged?.Invoke();
        }
    }
}
