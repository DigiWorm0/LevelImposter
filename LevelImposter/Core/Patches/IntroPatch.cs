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
    [HarmonyPatch(typeof(HudManager._CoShowIntro_d__83), nameof(HudManager._CoShowIntro_d__83.MoveNext))]
    public static class IntroPatch
    {
        public static bool Prefix()
        {
            if (MapLoader.CurrentMap == null)
                return true;
            if (LIShipStatus.Instance?.IsReady == true)
                return true;
            HudManager.Instance.StartCoroutine(CoWaitIntro().WrapToIl2Cpp());
            return false;
        }

        public static IEnumerator CoWaitIntro()
        {
            while (LIShipStatus.Instance?.IsReady != true)
                yield return null;
            yield return HudManager.Instance.CoShowIntro();
        }
    }
}
