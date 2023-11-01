using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.IO;

namespace LevelImposter.Core
{
    public class MapAssetDB
    {
        private Dictionary<Guid, DBElement> _db = new();
        public Dictionary<Guid, DBElement> DB => _db;

        public void Add(Guid id, byte[] rawData)
        {
            _db.Add(id, new DBElement { rawData = rawData });
        }

        public void Add(Guid id, FileChunk fileChunk)
        {
            _db.Add(id, new DBElement { fileChunk = fileChunk });
        }

        public DBElement? Get(Guid? id)
        {
            if (id == null)
                return null;
            _db.TryGetValue((Guid)id, out DBElement? result);
            return result;
        }

        public class DBElement
        {
            public Il2CppStructArray<byte>? rawData { get; set; }
            public FileChunk? fileChunk { get; set; }

            public Stream OpenStream()
            {
                if (rawData != null)
                    return new MemoryStream(rawData);
                else if (fileChunk != null)
                    return fileChunk.OpenStream();
                else
                    throw new Exception("No data to convert to stream");
            }

            public byte[] ToBytes()
            {
                if (rawData != null)
                    return rawData;
                else if (fileChunk != null)
                    using (var stream = fileChunk.OpenStream())
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        return buffer;
                    }
                else
                    throw new Exception("No data to convert to bytes");
            }
        }
    }
}
