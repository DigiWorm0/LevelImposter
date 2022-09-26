using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    public class LIMapConsole : OptionsConsole
    {
        public LIMapConsole(IntPtr intPtr) : base(intPtr)
        {
        }

        public override void Use()
        {
            bool canUse;
            CanUse(PlayerControl.LocalPlayer.Data, out canUse, out _);
            if (!canUse)
                return;

            PlayerControl.LocalPlayer.NetTransform.Halt();
            GameObject obj = ShopBuilder.BuildShop();
            obj.transform.SetParent(Camera.main.transform, false);
            obj.transform.localPosition = new Vector3(0, 0, -50);
            DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(null, obj, null);
        }
    }
}
