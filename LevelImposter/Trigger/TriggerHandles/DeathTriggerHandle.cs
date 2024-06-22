using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class DeathTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID)
        {
            if (triggerID != "killArea")
                return;

            // Kill Area
            LIDeathArea? deathArea = gameObject.GetComponent<LIDeathArea>();
            if (deathArea != null)
                deathArea.KillAllPlayers();
        }
    }
}
