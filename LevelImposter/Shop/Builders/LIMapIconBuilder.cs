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
}