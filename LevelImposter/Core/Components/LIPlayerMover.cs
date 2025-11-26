using System;
using LevelImposter.Trigger;

namespace LevelImposter.Core;

/// <summary>
///     Object that fires a trigger when the player enters/exits it's range
/// </summary>
public class LIPlayerMover(IntPtr intPtr) : PlayerArea(intPtr)
{
    public override void OnPlayerEnter(PlayerControl player)
    {
        if (player.AmOwner)
            player.transform.SetParent(transform);
    }

    public override void OnPlayerExit(PlayerControl player)
    {
        if (player.AmOwner)
            player.transform.SetParent(LIShipStatus.GetInstance().transform);
    }
}