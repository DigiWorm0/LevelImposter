using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class SoundTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID)
        {
            TriggerSoundPlayer? triggerSound = gameObject.GetComponent<TriggerSoundPlayer>();
            if (triggerSound == null)
                return;

            if (triggerID == "playonce")
                triggerSound.Play(false);
            else if (triggerID == "playloop")
                triggerSound.Play(true);
            else if (triggerID == "stop")
                triggerSound.Stop();
        }

    }
}
