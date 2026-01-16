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
            MapIcon = null, // ICON-Skeld (78x78)
            MapImage = null, // IMAGE-SkeldBanner (844x89)
            NameImage = null // IMAGE-SkeldBannerWordart (342x72)
        };

        return _mapIcon;
    }
}