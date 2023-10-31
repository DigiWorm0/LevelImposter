using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LevelImposter.Core
{
    public class FileChunkConverter : JsonConverter<FileChunk>
    {
        public static string FilePath = "";

        public override FileChunk? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();
            long offset = reader.TokenStartIndex;
            long length = reader.ValueSpan.Length;
            return new FileChunk(FilePath, offset, length);
        }

        public override void Write(Utf8JsonWriter writer, FileChunk value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
