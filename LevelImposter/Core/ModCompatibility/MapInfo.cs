using System.Collections.Generic;
using System.Linq;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

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
    // Internal object groups, you can ignore these
    private static readonly ObjectGroup CameraPanels = new("util-cams", "util-cams2", "util-cams3");
    private static readonly ObjectGroup Binoculars = new("sab-cams4");
    private static readonly ObjectGroup AdminTables = new("util-admin");
    private static readonly ObjectGroup Vitals = new("util-vitals");
    private static readonly ObjectGroup Spores = new("util-spore");
    private static readonly ObjectGroup MovingPlatforms = new("util-platform");
    private static readonly ObjectGroup Ladders = new("util-ladder1", "util-ladder2");
    private static readonly ObjectGroup Doors = new("sab-doorh", "sab-doorv");
    private static readonly ObjectGroup Vents = new("sab-vent1", "sab-vent2", "sab-vent3");
    private static readonly ObjectGroup CustomEjectAnimations = new("util-eject");
    private static readonly ObjectGroup Teleporter = new("util-tele");
    private static readonly ObjectGroup DeathTriggers = new("util-triggerdeath");
    private static readonly ObjectGroup Decontamination = new("util-decontamination");

    /// <summary>
    ///     The current map ID.
    ///     If no map is currently loaded, this is null.
    ///     <remarks>
    ///         Having a defined map ID does not mean the player is on a LevelImposter map.
    ///         It only means that map data is loaded and available.
    ///         LevelImposter tries to always have a randomized fallback map loaded for mods that include map randomization.
    ///     </remarks>
    /// </summary>
    public static string? MapID => GameConfiguration.CurrentMap?.id;

    // Fetch a list of GameObjects of the given object type
    // These will return [] until after ShipStatus.Awake (when the GameObjects are created)
    public static IEnumerable<GameObject> AllCameraPanels => CameraPanels.GetObjects();
    public static IEnumerable<GameObject> AllBinoculars => Binoculars.GetObjects();
    public static IEnumerable<GameObject> AllAdminTables => AdminTables.GetObjects();
    public static IEnumerable<GameObject> AllVitals => Vitals.GetObjects();
    public static IEnumerable<GameObject> AllSpores => Spores.GetObjects();
    public static IEnumerable<GameObject> AllMovingPlatforms => MovingPlatforms.GetObjects();
    public static IEnumerable<GameObject> AllLadders => Ladders.GetObjects();
    public static IEnumerable<GameObject> AllDoors => Doors.GetObjects();
    public static IEnumerable<GameObject> AllVents => Vents.GetObjects();
    public static IEnumerable<GameObject> AllCustomEjectAnimations => CustomEjectAnimations.GetObjects();
    public static IEnumerable<GameObject> AllTeleporters => Teleporter.GetObjects();
    public static IEnumerable<GameObject> AllDeathTriggers => DeathTriggers.GetObjects();

    // Check if the map contains the given object.
    // These can be checked in the lobby as soon as a map is selected in the UI.
    // You can patch `OnMapChange` or listen to `GameConfiguration.OnMapChange` to know when the map changes.
    public static bool HasCams => CameraPanels.HasAny();
    public static bool HasBinoculars => Binoculars.HasAny();
    public static bool HasAdminTable => AdminTables.HasAny();
    public static bool HasVitals => Vitals.HasAny();
    public static bool HasSpores => Spores.HasAny();
    public static bool HasMovingPlatform => MovingPlatforms.HasAny();
    public static bool HasLadder => Ladders.HasAny();
    public static bool HasDoors => Doors.HasAny();
    public static bool HasVents => Vents.HasAny();
    public static bool HasCustomEjectAnimation => CustomEjectAnimations.HasAny();
    public static bool HasTeleporter => Teleporter.HasAny();
    public static bool HasDeathTrigger => DeathTriggers.HasAny();
    public static bool HasDecontamination => Decontamination.HasAny();
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
    ///     Internal group of object types
    /// </summary>
    /// <param name="types">The types of objects to group together</param>
    internal class ObjectGroup(params string[] types)
    {
        public IEnumerable<LIElement> GetElements()
        {
            return GameConfiguration
                       .CurrentMap?
                       .elements
                       .Where(e => types.Contains(e.type))
                   ?? [];
        }

        public IEnumerable<GameObject> GetObjects()
        {
            return GetElements()
                .Select(e => LIBaseShip.Instance?.MapObjectDB.GetObject(e.id))
                .Where(go => go != null)
                .Select(go => go!);
        }

        public bool HasAny()
        {
            return GetElements().Any();
        }
    }
}