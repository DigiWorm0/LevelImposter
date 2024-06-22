using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class TeleportTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(GameObject gameObject, string triggerID)
        {
            if (triggerID != "teleportonce")
                return;

            // Teleport
            var teleporter = gameObject.GetComponent<LITeleporter>();
            if (teleporter == null)
                LILogger.Warn($"{gameObject} is not a teleporter");
            teleporter?.TeleportOnce();
        }
    }
}
