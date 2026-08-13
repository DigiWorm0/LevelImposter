using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;

namespace LevelImposter.Trigger.Handles;

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