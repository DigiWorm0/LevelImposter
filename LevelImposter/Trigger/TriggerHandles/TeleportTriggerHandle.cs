using LevelImposter.Core;
using System;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class TeleportTriggerHandle : ITriggerHandle
    {
        public void OnTrigger(TriggerSignal signal)
        {
            if (signal.TriggerID != "teleportonce")
                return;

            // Get Teleporter
            var teleporter = signal.TargetObject.GetComponentOrThrow<LITeleporter>();

            // Teleport players in area
            teleporter.TeleportOnce();
        }
    }
}
