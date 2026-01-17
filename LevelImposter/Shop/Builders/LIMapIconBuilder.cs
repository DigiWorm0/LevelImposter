using Il2CppSystem.Collections.Generic;
using LevelImposter.Core;

namespace LevelImposter.Shop;

public static class LIMapIconBuilder
{
    private static MapIconByName? _mapIcon = null;
    
    public static MapIconByName Get()
    {
        _mapIcon ??= new MapIconByName
        {
            Name = (MapNames)MapType.LevelImposter,
            MapIcon = MapUtils.LoadSpriteResource("LOBBY-Icon.png"),
            MapImage = MapUtils.LoadSpriteResource("LOBBY-Banner.png"),
            NameImage = MapUtils.LoadSpriteResource("LOBBY-WordArt.png"),
        };

        return _mapIcon;
    }

    /// <summary>
    /// Adds the LevelImposter map icon to the provided list if it is not already present.
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