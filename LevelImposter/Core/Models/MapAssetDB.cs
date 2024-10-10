using System;
using System.Collections.Generic;
using System.IO;

namespace LevelImposter.Core;

public class MapAssetDB
{
    public Dictionary<Guid, DBElement> DB { get; } = new();

    public void Add(Guid id, byte[] rawData)
    {
        DB.Add(id, new DBElement { rawData = rawData });
    }

    public void Add(Guid id, FileChunk fileChunk)
    {
        DB.Add(id, new DBElement { fileChunk = fileChunk });
    }

    public DBElement? Get(Guid? id)
    {
        if (id == null)
            return null;
        DB.TryGetValue((Guid)id, out var result);
        if (result == null)
            LILogger.Warn($"No such map asset with id {id}");
        return result;
    }

    public class DBElement : IStreamable
    {
        public byte[]? rawData { get; set; }
        public FileChunk? fileChunk { get; set; }

        public Stream OpenStream()
        {
            if (rawData != null)
                return new MemoryStream(rawData);
            if (fileChunk != null)
                return fileChunk.OpenStream();
            throw new Exception("No data to convert to stream");
        }

        public byte[] ToBytes()
        {
            if (rawData != null)
                return rawData;
            if (fileChunk == null)
                throw new Exception("No data to convert to bytes");

            var stream = fileChunk.OpenStream();
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            return buffer;
        }
    }
}