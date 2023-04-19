using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Fixes the TOU miner
     *      and TOR JackInTheBox vents
     *      to work in LevelImposter.
     */
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
    public static class VentPatch
    {
        private static int _ventTotal = -1;

        public static void Postfix()
        {
            if (LIShipStatus.Instance == null)
                return;
            if (!ModCompatibility.IsTOUEnabled && !ModCompatibility.IsTOREnabled)
                return;
            if (_ventTotal == ShipStatus.Instance.AllVents.Count)
                return;

            // Iterate Vents
            _ventTotal = ShipStatus.Instance.AllVents.Count;
            foreach (var vent in ShipStatus.Instance.AllVents)
            {
                // Filter Vents
                bool isCloneVent = vent.name.EndsWith("(Clone)") || vent.name.StartsWith("JackInTheBoxVent");
                if (isCloneVent && vent.transform.childCount == 1)
                {
                    vent.name = $"AU_Vent{vent.Id}";

                    // Reset Vent Buttons
                    ButtonBehavior[] ventButtons = vent.GetComponentsInChildren<ButtonBehavior>(true);
                    foreach (var ventButton in ventButtons)
                    {
                        string[] split = ventButton.name.Split("-");
                        int dir = split.Length > 1 ? int.Parse(split[1]) : 0;
                            
                        Action action;
                        if (dir == 0)
                            action = vent.ClickRight;
                        else if (dir == 1)
                            action = vent.ClickLeft;
                        else
                            action = vent.ClickCenter;

                        ventButton.OnClick.AddListener(action);
                    }
                }
            }
        }
    }
}