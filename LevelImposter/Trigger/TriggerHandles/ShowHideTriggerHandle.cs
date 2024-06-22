using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class ShowHideTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID)
        {
            // Show/Hide the object
            if (triggerID == "show")
                gameObject.SetActive(true);
            else if (triggerID == "hide")
                gameObject.SetActive(false);

            // Enable/Disable the components
            if (triggerID == "enable" || triggerID == "show")
                SetTriggerState(gameObject, true);
            else if (triggerID == "disable" || triggerID == "hide")
                SetTriggerState(gameObject, false);
            else if (triggerID == "toggle")
                SetTriggerState(gameObject);
        }

        /// <summary>
        /// Sets the state of the trigger components
        /// </summary>
        /// <param name="gameObject">GameObject to modify</param>
        /// <param name="isEnabled">null to toggle, true to enable, false to disable</param>
        private static void SetTriggerState(GameObject gameObject, bool? isEnabled = null)
        {
            // Trigger Sounds
            TriggerSoundPlayer? triggerSound = gameObject.GetComponent<TriggerSoundPlayer>();
            if (triggerSound != null)
            {
                if (isEnabled ?? !triggerSound.IsPlaying)
                    triggerSound.Play(true);
                else
                    triggerSound.Stop();
            }

            // Animation
            GIFAnimator? gifAnimator = gameObject.GetComponent<GIFAnimator>();
            if (gifAnimator != null)
            {
                if (isEnabled ?? !gifAnimator.IsAnimating)
                    gifAnimator.Play();
                else
                    gifAnimator.Stop();
            }

            // Get Togglable Components
            MonoBehaviour[] toggleComponents = new MonoBehaviour[]
            {
                gameObject.GetComponent<AmbientSoundPlayer>(),
                gameObject.GetComponent<TriggerConsole>(),
                gameObject.GetComponent<SystemConsole>(),
                gameObject.GetComponent<MapConsole>(),
                gameObject.GetComponent<LITeleporter>(),
                gameObject.GetComponent<LIShakeArea>()
            };

            // Toggle the components
            foreach (MonoBehaviour component in toggleComponents)
            {
                if (component == null)
                    continue;
                component.enabled = isEnabled ?? !component.enabled;
            }
        }
    }
}
