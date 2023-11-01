using System;
using System.Collections.Generic;
using System.IO;

namespace LevelImposter.Core
{
    public class SpriteDB
    {
        private Dictionary<Guid, DBElement> _db = new();
        public Dictionary<Guid, DBElement> DB => _db;

        public void Add(Guid id, string rawData)
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
            public string? rawData { get; set; }
            public FileChunk? fileChunk { get; set; }

            public Stream OpenStream()
            {
                if (rawData != null)
                    return new MemoryStream(Convert.FromBase64String(rawData));
                else if (fileChunk != null)
                    return fileChunk.OpenStream();
                else
                    throw new Exception("No data to convert to stream");
            }

            public override string ToString()
            {
                if (rawData != null)
                    return rawData;
                else if (fileChunk != null)
                    return fileChunk.ToString();
                else
                    throw new Exception("No data to convert to string");
            }
        }
    }
}
