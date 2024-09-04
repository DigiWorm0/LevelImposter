using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class MapObjectDataBuilder : IElemBuilder
{
    public void Build(LIElement element, GameObject obj)
    {
        // Get Ship Status
        var shipStatus = LIShipStatus.GetInstanceOrNull();
        if (shipStatus == null)
            throw new MissingShipException();

        // All map objects will have a MapObjectData component
        var mapObjectData = obj.AddComponent<MapObjectData>();
        mapObjectData.SetSourceElement(element);

        // Add to DB
        shipStatus.MapObjectDB.AddObject(element.id, obj);
    }

    public void PostBuild()
    {
    }
}