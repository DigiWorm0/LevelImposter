using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;
using Hazel;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Core
{
    /*
     *      Prevents the intro scene from 
     *      playing until the map is loaded in
     */
    [HarmonyPatch(typeof(HudManager._CoShowIntro_d__82), nameof(HudManager._CoShowIntro_d__82.MoveNext))]
    public static class IntroPatch
    {
        private static bool _canShowIntro = false;

        public static bool Prefix(HudManager._CoShowIntro_d__82 __instance)
        {
            if (MapLoader.CurrentMap == null)
                return true;
            if (_canShowIntro)
                return true;
            HudManager.Instance.StartCoroutine(CoWaitIntro().WrapToIl2Cpp());
            return false;
        }

        public static IEnumerator CoWaitIntro()
        {
            while (LIShipStatus.Instance == null)
                yield return null;
            while (!LIShipStatus.Instance.IsReady)
                yield return null;
            _canShowIntro = true;
            yield return HudManager.Instance.CoShowIntro();
            _canShowIntro = false;
        }
    }
}
