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
using TMPro;


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

        private static GameObject? _loadingPrefab = null;
    
        /// <summary>
        /// Gets the shop loading spinner prefab
        /// </summary>
        /// <returns>The loading spinner prefab</returns>
        private static GameObject GetLoadingPrefab()
        {
            if (_loadingPrefab == null)
                _loadingPrefab = MapUtils.LoadAssetBundle("loadingscreen");
            return _loadingPrefab;
        }

        /// <summary>
        /// Coroutine that instantiates the shop
        /// </summary>
        [HideFromIl2Cpp]
        public IEnumerator CoSpawnShop()
        {
            {
                // Loading Spinners
                GameObject loadingPrefab = Instantiate(GetLoadingPrefab(), transform);
                Transform spinnerTransform = loadingPrefab.transform.GetChild(0);
                spinnerTransform.gameObject.AddComponent<Spinner>();
                Transform statusTransform = loadingPrefab.transform.GetChild(1);
                TMP_Text statusText = statusTransform.gameObject.GetComponent<TMP_Text>();
                while (!AssetDB.IsInit)
                {
                    var status = AssetDB.Instance?.Status ?? "";
                    if (statusText.text != status)
                        statusText.text = $"<b>Loading Among Us Assets</b>\n{status}";
                    yield return null;

                }

                if (ShopManager.Instance == null)
                    ShopBuilder.Build();
                Destroy(gameObject);
            }
        }

        public void Start()
        {
            StartCoroutine(CoSpawnShop().WrapToIl2Cpp());
        }
    }
}