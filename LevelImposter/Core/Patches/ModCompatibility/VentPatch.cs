using HarmonyLib;
using System;
using UnityEngine.Events;

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
            for (int i = 0; i < ShipStatus.Instance.AllVents.Count; i++)
            {
                Vent vent = ShipStatus.Instance.AllVents[i];

                // Filter Vents
                string fixedVentTag = $"(V{i})";
                bool isFixed = vent.name.Contains(fixedVentTag);
                bool isLIVent = vent.transform.childCount == 1;
                if (isFixed || !isLIVent)
                    continue;

                // Update Button Actions
                vent.name = $"{vent.name}{fixedVentTag}";
                for (int b = 0; b < vent.Buttons.Length; b++)
                {
                    var ventButton = vent.Buttons[b];
                    ventButton.OnMouseOver = new UnityEvent();
                    ventButton.OnMouseOut = new UnityEvent();
                    Action action = b switch
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