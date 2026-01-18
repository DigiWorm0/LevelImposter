using System;

namespace LevelImposter.Core;

/// <summary>
/// Defines how and when an object should be garbage collected.
/// </summary>
[Flags]
public enum GCBehavior
{
    NeverDispose = 1,
    DisposeOnMapUnload = 2,
    DisposeOnLobbyUnload = 4,
    AlwaysDispose = DisposeOnMapUnload | DisposeOnLobbyUnload
}