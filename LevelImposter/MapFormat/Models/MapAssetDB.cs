using System;
using System.Collections.Generic;
using LevelImposter.Core.Utils;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.MapFormat.Models;

public class MapAssetDB
{
    public Dictionary<Guid, IDataStore> DB { get; } = new();

    public void Add(Guid id, IDataStore dataStore)
    {
        DB.Add(id, dataStore);
    }

    public IDataStore? Get(Guid? id)
    {
        if (id == null)
            return null;
        DB.TryGetValue((Guid)id, out var result);
        if (result == null)
            LILogger.Warn($"No such map asset with id {id}");
        return result;
    }
}
