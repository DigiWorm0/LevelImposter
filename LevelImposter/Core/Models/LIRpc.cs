namespace LevelImposter.Core;
// Among Us     0 - 65
// TOR          60 - 73, 100 - 149
// Las Monjas   60 - 69, 75 - 194
// StellaRoles  60 - 169
// ToU          100 - 210, 220 - 251
// Submerged    210 - 214

// LI           90 - 99 (Wow we really need to coordinate this better)

public enum LIRpc
{
    FireTrigger = 90, // Fires a global trigger on an object
    SyncPhysicsObject, // Used by util-physics object
    SyncGameConfiguration, // Syncs the current map ID and settings
    SyncRandomSeed, // Syncs a random seed for util-triggerrand (No longer used)
    ResetPlayer, // Resets the player on Ctrl-R E S
    ReadyToStart, // Player signals they have all necessary map data and are ready to start
    KillPlayer, // Used by util-triggerdeath object
    SyncPlayerMover, // Used by LIPlayerMover to sync player transforms
    Reserved98,
    Reserved99
}