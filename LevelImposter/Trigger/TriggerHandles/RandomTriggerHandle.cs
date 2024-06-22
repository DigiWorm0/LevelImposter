using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class RandomTriggerHandle : ITriggerHandle
    {
        private int _randomOffset = 0;

        public void OnTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin = null, int stackSize = 0)
        {
            if (triggerID != "random")
                return;

            // Get source element
            var objectData = gameObject.GetComponent<MapObjectData>();
            if (objectData == null)
            {
                LILogger.Warn($"{gameObject} is missing LI data");
                return;
            }

            // Get a random value
            // Seed is synced across all clients, so the same value is generated on all clients
            float randVal = RandomizerSync.GetRandom(objectData.ID, _randomOffset++);

            // Get the random chance (0 - 1)
            float randomChance = 1.0f / (objectData.Properties.triggerCount ?? 2);

            // Get the trigger index based on the random value (0 - triggerCount)
            int triggerIndex = Mathf.FloorToInt(randVal / randomChance);

            // Get the trigger ID
            string targetID = "onRandom " + (triggerIndex + 1);

            // Fire Trigger
            TriggerSystem.Trigger(gameObject, targetID, null, stackSize);
        }

    }
}
