using System;
using LevelImposter.Core;

namespace LevelImposter.Trigger;

public class DeathTriggerHandle : ITriggerHandle
{
    public void OnTrigger(TriggerSignal signal)
    {
        if (signal.TriggerID != "killArea")
            return;

        // Get LIDeathArea
        var deathArea = signal.TargetObject.GetComponent<LIDeathArea>();

        // Check if the area is a LIDeathArea
        if (deathArea == null)
            throw new Exception($"{signal.TargetObject} is missing an LIDeathArea");

        // Kill all players in area
        deathArea.KillAllPlayers();
    }
}