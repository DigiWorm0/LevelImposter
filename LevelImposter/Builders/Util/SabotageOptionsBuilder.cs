using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class SabotageOptionsBuilder : IElemBuilder
{
    public const string SABOTAGE_SOUND_NAME = "sabotageSound";


    public SabotageOptionsBuilder()
    {
        TriggerObject = null;
    }

    public static GameObject? TriggerObject { get; private set; }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-sabotages")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Singleton
        if (TriggerObject != null)
        {
            LILogger.Warn("Only 1 util-sabotages object can be placed per map");
            return;
        }

        TriggerObject = obj;

        // Sabotage Sound
        var sabotageSound = MapUtils.FindSound(elem.properties.sounds, SABOTAGE_SOUND_NAME);
        if (sabotageSound != null)
            shipStatus.SabotageSound = WAVFile.LoadSound(sabotageSound) ?? shipStatus.SabotageSound;
    }
}