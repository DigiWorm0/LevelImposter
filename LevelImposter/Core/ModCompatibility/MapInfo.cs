using System.Linq;
using LevelImposter.Core.Utils;

// Values here are used by other mods, so no references will be found in the current assembly.
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace LevelImposter.Core.ModCompatibility;

/// <summary>
///     Provides information about the currently active LevelImposter map and its features.
///     Allows external mods to modify game settings depending on the current map.
/// </summary>
public static class MapInfo
{
    /// <summary>
    ///     The current map ID. Null means no map is loaded.
    ///     <remarks>
    ///         Having a defined map ID does not mean the player is playing on a LevelImposter map.
    ///         It only means that map data is loaded and available.
    ///         LevelImposter tries to always have a randomized fallback map loaded for mods that include map randomization.
    ///     </remarks>
    /// </summary>
    public static string? MapID => GameConfiguration.CurrentMap?.id;

    public static bool HasCams => HasElementType("util-cams") ||
                                  HasElementType("util-cams2") ||
                                  HasElementType("util-cams3");

    public static bool HasBinoculars => HasElementType("sab-cams4");

    public static bool HasAdminTable => HasElementType("util-admin");
    public static bool HasVitals => HasElementType("util-vitals");
    public static bool HasSpores => HasElementType("util-spore");
    public static bool HasMovingPlatform => HasElementType("util-platform");

    public static bool HasLadder => HasElementType("util-ladder1") ||
                                    HasElementType("util-ladder2");

    public static bool HasDoors => HasElementType("sab-doorh") ||
                                   HasElementType("sab-doorv");

    public static bool HasVents => HasElementType("sab-vent1") ||
                                   HasElementType("sab-vent2") ||
                                   HasElementType("sab-vent3");

    public static bool HasCustomEjectAnimation => HasElementType("util-eject");
    public static bool HasTeleporter => HasElementType("util-tele");
    public static bool HasDeathTrigger => HasElementType("util-triggerdeath");
    public static bool HasDecontamination => HasElementType("util-decontamination");

    public static bool HasDoorLogs => false; // <-- Not in LI currently

    /// <summary>
    ///     Returns the type of doors this map uses.
    /// </summary>
    /// <returns>
    ///     "none" - This map does not include doors.
    ///     "skeld" - This map uses the Skeld door timer (automatic).
    ///     "polus" - This map uses the Polus door UI (manual, switches).
    ///     "airship" - This map uses the Airship door UI (manual, card swipe).
    /// </returns>
    public static string GetDoorType()
    {
        if (!HasDoors)
            return "none";

        var door = GameConfiguration.CurrentMap?.elements.FirstOrDefault(e => e.type.StartsWith("sab-door"));
        return door?.properties.doorType ?? "skeld";
    }

    /// <summary>
    ///     Patchable function that is called whenever the map changes.
    ///     Allows external mods to listen for LI map changes using HarmonyX.
    ///     <remarks>
    ///         Alternatively, mods can use the GameConfiguration.OnMapChange event.
    ///     </remarks>
    /// </summary>
    public static void OnMapChange()
    {
        // Should be patched by another plugin
    }

    /// <summary>
    ///     Searches the map for an LI element of a specific type
    /// </summary>
    /// <param name="elementType">The type of the element to search for</param>
    /// <returns>
    ///     True if the map contains at least one element of that type.
    ///     False if the map is null or no element exists.
    /// </returns>
    public static bool HasElementType(string elementType)
    {
        return GameConfiguration.CurrentMap?.elements.Any(e => e.type == elementType) ?? false;
    }
}