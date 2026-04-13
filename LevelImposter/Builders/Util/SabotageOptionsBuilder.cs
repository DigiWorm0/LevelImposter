using LevelImposter.AssetLoader;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

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
        var sabotageSound = MapUtils.FindSound(elem.properties.sounds, SABOTAGE_SOUND_NAME);
        if (sabotageSound != null)
            shipStatus.SabotageSound = WAVLoader.Load(sabotageSound) ?? shipStatus.SabotageSound;
    }
}