using System;
using System.Collections.Generic;
using LevelImposter.Core.Utils;
using LevelImposter.FileIO.DataBlock;
using LevelImposter.FileIO.DataStores;

namespace LevelImposter.Core.Models;

public class MapAssetDB
{
    public Dictionary<Guid, IDataStore> DB { get; } = new();

    public void Add(Guid id, MemoryBlock memoryBlock)
    {
        DB.Add(id, new MemoryStore(memoryBlock));
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