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
        public const string TOU_GUID = "com.slushiegoose.townofus";
        public const string SUBMERGED_GUID = "Submerged";
        public const ShipStatus.MapType SUBMERGED_MAP_TYPE = (ShipStatus.MapType)5;

        private static bool _isTOUEnabled = false;
        private static bool _isSubmergedEnabled = false;

        public static bool IsTOUEnabled => _isTOUEnabled;
        public static bool IsSubmergedEnabled => _isSubmergedEnabled;

        public static void Init()
        {
            _isTOUEnabled = IL2CPPChainloader.Instance.Plugins.TryGetValue(TOU_GUID, out PluginInfo _);
            _isSubmergedEnabled = IL2CPPChainloader.Instance.Plugins.TryGetValue(SUBMERGED_GUID, out PluginInfo _);
        }
    }
}
