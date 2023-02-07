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
        public const string TOU_GUID = "com.slushiegoose.townofus";
        public const string SUBMERGED_GUID = "Submerged";
        public const ShipStatus.MapType SUBMERGED_MAP_TYPE = (ShipStatus.MapType)5;
        public const string TOR_GUID = "me.eisbison.theotherroles";

        private static bool _isTOUEnabled = false;
        private static bool _isSubmergedEnabled = false;
        private static bool _isTOREnabled = false;

        public static bool IsTOUEnabled => _isTOUEnabled;
        public static bool IsSubmergedEnabled => _isSubmergedEnabled;
        public static bool IsTOREnabled => _isTOREnabled;

        public static void Init()
        {
            _isTOUEnabled = IL2CPPChainloader.Instance.Plugins.TryGetValue(TOU_GUID, out PluginInfo _);
            if (_isTOUEnabled)
                LILogger.Info("LevelImposter detected TOU installed, compatibility enabled");

            _isSubmergedEnabled = IL2CPPChainloader.Instance.Plugins.TryGetValue(SUBMERGED_GUID, out PluginInfo _);
            if (_isSubmergedEnabled)
                LILogger.Info("LevelImposter detected Submerged installed, currently unsupported");

            _isTOREnabled = IL2CPPChainloader.Instance.Plugins.TryGetValue(TOR_GUID, out PluginInfo _);
            if (_isTOREnabled)
                LILogger.Info("LevelImposter detected TOR installed, compatibility enabled");
        }
    }
}
