using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.DB;
using LevelImposter.Core;
using BepInEx.IL2CPP.Utils.Collections;

namespace LevelImposter.Shop
{
    public class ShopSpawner : MonoBehaviour
    {
        private static GameObject spinnerPrefab;

        public ShopSpawner(IntPtr intPtr) : base(intPtr)
        {
        }

        public void Start()
        {
            StartCoroutine(CoSpawnShop().WrapToIl2Cpp());
        }

        public static GameObject GetSpinnerPrefab()
        {
            if (spinnerPrefab == null)
                spinnerPrefab = MapUtils.LoadAssetBundle("spinner");
            return spinnerPrefab;
        }

        public IEnumerator CoSpawnShop()
        {
            GameObject spinner = Instantiate(GetSpinnerPrefab(), transform);
            spinner.AddComponent<Spinner>();
            while (!AssetDB.isReady)
                yield return null;

            if (!ShopManager.IsOpen)
                ShopBuilder.BuildShop();
            Destroy(gameObject);
        }
    }
}