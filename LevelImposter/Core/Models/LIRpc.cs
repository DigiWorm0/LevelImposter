﻿namespace LevelImposter.Core
{
    // Among Us     0 - 50
    // TOR          60 - 73, 100 - 149
    // Las Monjas   60 - 69, 75 - 194
    // StellaRoles  60 - 169
    // ToU          100 - 210, 220 - 251
    // Submerged    210 - 214

    // LI           51 - 59 (Guess I'll exist here...)

    public enum LIRpc
    {
        FireTrigger = 51,   // Fires a global trigger on an object
        TeleportPlayer,     // Uses a util-tele object
        SyncMapID,          // Syncs the map ID in the lobby
        SyncRandomSeed,     // Syncs a random seed for util-triggerrand
        ResetPlayer,        // Resets the player on Ctrl-R E S
        DownloadCheck       // Warns the host that the client is still downloading the map
    }
}
