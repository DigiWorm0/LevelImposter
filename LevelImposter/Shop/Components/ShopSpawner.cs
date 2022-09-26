using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace LevelImposter.Shop
{
    public class ShopSpawner : MonoBehaviour
    {
        public ShopSpawner(IntPtr intPtr) : base(intPtr)
        {
        }

        public void Start()
        {
            if (!ShopManager.IsOpen)
                ShopBuilder.BuildShop();
            Destroy(gameObject);
        }
    }
}