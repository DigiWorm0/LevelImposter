using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.DB;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace LevelImposter.Shop
{
    public class ShopSpawner : MonoBehaviour
    {
        public ShopSpawner(IntPtr intPtr) : base(intPtr)
        {
        }

        private static GameObject _spinnerPrefab;

        public void Start()
        {
            StartCoroutine(CoSpawnShop().WrapToIl2Cpp());
        }

        public static GameObject GetSpinnerPrefab()
        {
            if (_spinnerPrefab == null)
                _spinnerPrefab = MapUtils.LoadAssetBundle("spinner");
            return _spinnerPrefab;
        }

        public IEnumerator CoSpawnShop()
        {
            GameObject spinner = Instantiate(GetSpinnerPrefab(), transform);
            spinner.AddComponent<Spinner>();
            while (!AssetDB.IsReady)
                yield return null;

            if (!ShopManager.IsOpen)
                ShopBuilder.Build();
            Destroy(gameObject);
        }
    }
}