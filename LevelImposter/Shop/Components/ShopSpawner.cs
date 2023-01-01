using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.DB;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// Component that spawns the shop on <c>Start()</c>
    /// </summary>
    public class ShopSpawner : MonoBehaviour
    {
        public ShopSpawner(IntPtr intPtr) : base(intPtr)
        {
        }

        private static GameObject? _spinnerPrefab = null;
    
        /// <summary>
        /// Gets the shop loading spinner prefab
        /// </summary>
        /// <returns>The loading spinner prefab</returns>
        private static GameObject GetSpinnerPrefab()
        {
            if (_spinnerPrefab == null)
                _spinnerPrefab = MapUtils.LoadAssetBundle("spinner");
            return _spinnerPrefab;
        }

        /// <summary>
        /// Coroutine that instantiates the shop
        /// </summary>
        [HideFromIl2Cpp]
        public IEnumerator CoSpawnShop()
        {
            GameObject spinner = Instantiate(GetSpinnerPrefab(), transform);
            spinner.AddComponent<Spinner>();
            while (!AssetDB.IsReady)
                yield return null;

            if (ShopManager.Instance == null)
                ShopBuilder.Build();
            Destroy(gameObject);
        }

        public void Start()
        {
            StartCoroutine(CoSpawnShop().WrapToIl2Cpp());
        }
    }
}