using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class SabotageOptionsBuilder
{
    private const string SABOTAGE_SOUND_NAME = "sabotageSound";

    public static GameObject? TriggerObject { get; private set; }

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        TriggerObject = null;
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes = ["util-sabotages"]
    )]
    public static void Build(ShipStatus shipStatus, LIElement element, GameObject gameObject)
    {
        // Singleton
        if (TriggerObject != null)
        {
            LILogger.Warn("Only 1 util-sabotages object can be placed per map");
            return;
        }

        TriggerObject = gameObject;

        // Sabotage Sound
        var sabotageSound = element.properties.sounds.FindSound(SABOTAGE_SOUND_NAME);
        if (sabotageSound != null)
            shipStatus.SabotageSound = WAVLoader.Load(sabotageSound) ?? shipStatus.SabotageSound;
    }
}