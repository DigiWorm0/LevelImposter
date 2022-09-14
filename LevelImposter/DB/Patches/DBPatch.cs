using HarmonyLib;
using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LevelImposter.DB
{
    /*
     *      Loads all ship prefabs
     *      to memory so that au assets
     *      can be stored in a db
     */
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class DBPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            GameObject dbObj = new GameObject("AssetDB");
            UnityEngine.Object.DontDestroyOnLoad(dbObj);
            dbObj.AddComponent<AssetDB>();
        }
    }
}
