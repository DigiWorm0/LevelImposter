using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Fixes the TOU miner vents
     *      to work in LevelImposter.
     */
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
    public static class TOU_MinerPatch
    {
        private static int _ventTotal = -1;

        public static void Postfix()
        {
            if (MapLoader.CurrentMap == null)
                return;
            if (!ModCompatibility.IsTOUEnabled && !ModCompatibility.IsTOREnabled)
                return;
            if (_ventTotal == ShipStatus.Instance.AllVents.Count)
                return;

            _ventTotal = ShipStatus.Instance.AllVents.Count;
            foreach (var vent in ShipStatus.Instance.AllVents)
            {
                if ((vent.name.EndsWith("(Clone)") || vent.name.StartsWith("JackInTheBoxVent")) && vent.transform.childCount == 1)
                {
                    if (ModCompatibility.IsTOUEnabled)
                        vent.name = $"TOU_Vent{vent.Id}";

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