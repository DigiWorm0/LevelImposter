using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class AmbientSoundBuilder : IElemBuilder
{
    private const string AMBIENT_SOUND_TYPE = "util-sound1";
    private const string TRIGGER_SOUND_TYPE = "util-triggersound";

    public void Build(LIElement elem, GameObject obj)
    {
        var isAmbient = elem.type == AMBIENT_SOUND_TYPE;
        var isTrigger = elem.type == TRIGGER_SOUND_TYPE;
        if (!isAmbient && !isTrigger)
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // AudioClip
        if (elem.properties.sounds == null)
        {
            LILogger.Warn($"{elem.name} missing audio listing");
            return;
        }

        if (elem.properties.sounds.Length <= 0)
        {
            LILogger.Warn($"{elem.name} missing audio elements");
            return;
        }

        // Sound Data
        var soundData = elem.properties.sounds[0];
        if (soundData == null)
        {
            LILogger.Warn($"{elem.name} missing audio data");
            return;
        }

        // Sound Player
        if (isAmbient)
        {
            var ambientPlayer = obj.AddComponent<AmbientSoundPlayer>();
            ambientPlayer.HitAreas = colliders;
            ambientPlayer.MaxVolume = soundData.volume;
            ambientPlayer.AmbientSound = WAVFile.LoadSound(soundData);
        }
        else if (isTrigger)
        {
            var triggerPlayer = obj.AddComponent<TriggerSoundPlayer>();
            triggerPlayer.Init(soundData, colliders);
        }
    }

    public void PostBuild()
    {
    }
}