using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Fixes instantiated vents 
     *      (TOU miner / TOR JackInTheBox)
     *      to append UnityAction
     */
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
    public static class VentPatch
    {
        public const string FIXED_VENT_TAG = "(LIFixed)";

        private static int _ventTotal = -1;

        public static void Postfix()
        {
            if (LIShipStatus.Instance == null)
                return;
            if (!ModCompatibility.IsTOUEnabled && !ModCompatibility.IsTOREnabled)
                return;
            if (_ventTotal == ShipStatus.Instance.AllVents.Count)
                return;
            _ventTotal = ShipStatus.Instance.AllVents.Count;

            // Iterate Vents
            foreach (var vent in ShipStatus.Instance.AllVents)
            {
                // Filter Vents
                bool isFixed = vent.name.Contains(FIXED_VENT_TAG);
                bool isLIVent = vent.transform.childCount == 1;
                if (isFixed || !isLIVent)
                    continue;

                // Update Button Actions
                vent.name = $"{vent.name}{FIXED_VENT_TAG}";
                for (int i = 0; i < vent.Buttons.Length; i++)
                {
                    var ventButton = vent.Buttons[i];
                    Action action = i switch
                    {
                        0 => vent.ClickRight,
                        1 => vent.ClickLeft,
                        2 => vent.ClickCenter,
                        _ => HandleError
                    };
                    ventButton.OnClick.AddListener(action);
                }
            }
        }

        private static void HandleError()
        {
            LILogger.Warn("Vent object has an extra button assigned");
        }
    }
}