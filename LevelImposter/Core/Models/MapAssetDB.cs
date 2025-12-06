using System;
using System.Collections.Generic;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Core;

public class MapAssetDB
{
    public Dictionary<Guid, IDataStore> DB { get; } = new();

    public void Add(Guid id, byte[] rawData)
    {
        DB.Add(id, new MemoryStore(rawData));
    }

    public void Add(Guid id, FileChunkStore fileChunkStore)
    {
        DB.Add(id, fileChunkStore);
    }
    
    public void Add(Guid id, IDataStore streamable)
    {
        DB.Add(id, streamable);
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