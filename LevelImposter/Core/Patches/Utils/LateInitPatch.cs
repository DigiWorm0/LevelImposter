using HarmonyLib;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Runs a variety of initialization tasks after the game has started.
    /// </summary>
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    public static class LateInitPatch
    {
        private static bool _hasInitialized = false;

        public static void Postfix()
        {
            if (_hasInitialized)
                return;
            _hasInitialized = true;

            // Add Mod Stamp (In case Reactor is missing)
            DestroyableSingleton<ModManager>.Instance.ShowModStamp();

            // Increase player's max movement range
            // from (-50 - 50) to (-500 - 500)
            NetHelpers.XRange = new FloatRange(-500f, 500f);
            NetHelpers.YRange = new FloatRange(-500f, 500f);

            // Add Global Components that utilize
            // the Unity runtime in some way
            GameObject apiParent = new GameObject("LevelImposter");
            apiParent.AddComponent<LagLimiter>();
            apiParent.AddComponent<HTTPHandler>();
            apiParent.AddComponent<SpriteLoader>();
            UnityEngine.Object.DontDestroyOnLoad(apiParent);
        }
    }
}
