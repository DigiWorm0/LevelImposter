using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Core;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal class SabotageOptionsBuilder : IElemBuilder
{
    private const string SABOTAGE_SOUND_NAME = "sabotageSound";

    public static GameObject? TriggerObject { get; private set; }

    public void OnPreBuild()
    {
        TriggerObject = null;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-sabotages")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // Singleton
        if (TriggerObject != null)
        {
            LILogger.Warn("Only 1 util-sabotages object can be placed per map");
            return;
        }

        TriggerObject = obj;

        // Sabotage Sound
        var sabotageSound = elem.properties.sounds.FindSound(SABOTAGE_SOUND_NAME);
        if (sabotageSound != null)
            shipStatus.SabotageSound = WAVLoader.Load(sabotageSound) ?? shipStatus.SabotageSound;
    }
}