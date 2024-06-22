using UnityEngine;

namespace LevelImposter.Trigger
{
    public class RepeatTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin = null, int stackSize = 0)
        {
            if (triggerID != "repeat")
                return;

            // Fire Trigger
            for (int i = 1; i <= 8; i++)
                TriggerSystem.Trigger(gameObject, "onRepeat " + i, null, stackSize + 1); // Do not pass the origin player to prevent excessive RPC calls
        }

    }
}
