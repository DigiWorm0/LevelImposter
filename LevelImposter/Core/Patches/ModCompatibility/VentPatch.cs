using System;
using HarmonyLib;
using UnityEngine.Events;

namespace LevelImposter.Core;

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
        if (LIShipStatus.IsInstance())
            return;
        if (!ModCompatibility.IsTOUEnabled && !ModCompatibility.IsTOREnabled && !ModCompatibility.IsReworkedEnabled)
            return;
        if (_ventTotal == ShipStatus.Instance.AllVents.Count)
            return;
        _ventTotal = ShipStatus.Instance.AllVents.Count;

        // Iterate Vents
        for (var i = 0; i < ShipStatus.Instance.AllVents.Count; i++)
        {
            var vent = ShipStatus.Instance.AllVents[i];

            // Filter Vents
            var fixedVentTag = $"(V{i})";
            var isFixed = vent.name.Contains(fixedVentTag);
            var isLIVent = vent.transform.childCount == 1;
            if (isFixed || !isLIVent)
                continue;

            // Update Button Actions
            vent.name = $"{vent.name}{fixedVentTag}";
            for (var b = 0; b < vent.Buttons.Length; b++)
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