using Il2CppSystem.Collections.Generic;
using LevelImposter.Core;

namespace LevelImposter.Shop;

public static class LIMapIconBuilder
{
    private static MapIconByName? _mapIcon;

    public static MapIconByName Get()
    {
        _mapIcon ??= new MapIconByName
        {
            Name = (MapNames)MapType.LevelImposter,
            MapIcon = PackagedResources.LoadSprite("LOBBY-Icon.png"),
            MapImage = PackagedResources.LoadSprite("LOBBY-Banner.png"),
            NameImage = PackagedResources.LoadSprite("LOBBY-WordArt.png")
        };

        return _mapIcon;
    }

    /// <summary>
    ///     Adds the LevelImposter map icon to the provided list if it is not already present.
    /// </summary>
    /// <param name="list">The list of MapIconByName to add to.</param>
    public static void AddToList(List<MapIconByName> list)
    {
        // Check if already added
        foreach (var icon in list)
            if ((MapType)icon.Name == MapType.LevelImposter)
                return;

        // Add to list
        list.Add(Get());
    }
}