using System;

namespace LevelImposter.Core;

/// <summary>
///     Allows Ladders to have a different cooldown
/// </summary>
public class EditableLadderConsole(IntPtr intPtr) : Ladder(intPtr)
{
    private float _cooldownDuration = 5.0f;

    public override float MaxCoolDown => _cooldownDuration;

    /// <summary>
    ///     Sets the cooldown duration of the ladder
    /// </summary>
    /// <param name="duration">Duration in seconds</param>
    public void SetCooldownDuration(float duration)
    {
        _cooldownDuration = duration;
    }
}