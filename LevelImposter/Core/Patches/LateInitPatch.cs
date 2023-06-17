using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Runs Late Initialization Code
     */
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

            // Increase max X and Y range from -50 - 50 >>> -500 - 500
            NetHelpers.XRange = new FloatRange(-500f, 500f);
            NetHelpers.YRange = new FloatRange(-500f, 500f);

            // Add Global Components
            GameObject apiParent = new GameObject("LevelImposter");
            apiParent.AddComponent<HTTPHandler>();
            apiParent.AddComponent<FileHandler>();
            apiParent.AddComponent<SpriteLoader>();
            UnityEngine.Object.DontDestroyOnLoad(apiParent);
        }
    }
}
