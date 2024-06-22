using UnityEngine;

namespace LevelImposter.Trigger
{
    public interface ITriggerHandle
    {
        public virtual void OnTrigger(GameObject gameObject, string triggerID) { }

        public virtual void OnTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin = null, int stackSize = 0) => OnTrigger(gameObject, triggerID);
    }
}
