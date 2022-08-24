using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Bootstrap;

namespace LevelImposter.Core
{
    public static class ReactorCompatibility
    {
        private static bool? _enabled;
        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                    _enabled = BepInEx.IL2CPP.IL2CPPChainloader.Instance.Plugins.ContainsKey(LevelImposter.REACTOR_ID);
                return (bool)_enabled;
            }
        }
        
        public static void RegisterReactorRPC()
        {
            // TODO
        }
    }
}
