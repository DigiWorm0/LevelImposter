using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP;
using BepInEx;

namespace LevelImposter.Core
{
    public static class ModCompatibility
    {
        public const string REACTOR_ID = "gg.reactor.api";
        public const string TOR_GUID = "me.eisbison.theotherroles";
        public const string TOU_GUID = "com.slushiegoose.townofus";
        public const string SUBMERGED_GUID = "Submerged";
        public const ShipStatus.MapType SUBMERGED_MAP_TYPE = (ShipStatus.MapType)5;

        private static bool _isTOREnabled = false;
        private static bool _isTOUEnabled = false;
        private static bool _isSubmergedEnabled = false;

        public static bool IsTOREnabled => _isTOREnabled;
        public static bool IsTOUEnabled => _isTOUEnabled;
        public static bool IsSubmergedEnabled => _isSubmergedEnabled;

        public static void Init()
        {
            _isTOREnabled = IsPlugin(TOR_GUID);
            _isTOUEnabled = IsPlugin(TOU_GUID);
            _isSubmergedEnabled = IsPlugin(SUBMERGED_GUID);

            if (_isTOREnabled)
                LILogger.Info("LevelImposter detected TOR installed, compatibility enabled");
            if (_isTOUEnabled)
                LILogger.Info("LevelImposter detected TOU installed, compatibility enabled");
            if (_isSubmergedEnabled)
                LILogger.Info("LevelImposter detected Submerged installed, currently unsupported");
        }

        private static bool IsPlugin(string guid)
        {
            return IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out PluginInfo _);
        }
    }
}
