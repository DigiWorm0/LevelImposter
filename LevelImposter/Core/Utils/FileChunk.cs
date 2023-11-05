using System.IO;

namespace LevelImposter.Core
{
    public class FileChunk
    {
        private string _filePath;
        private long _offset = -1;
        private long _length = -1;

        public FileChunk(string filePath, long offset, long length)
        {
            _filePath = filePath;
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// Opens a stream to the file chunk.
        /// </summary>
        /// <returns>A stream to the cooresponding file chunk</returns>
        public FileChunkStream OpenStream()
        {
            FileStream fileStream = File.OpenRead(_filePath);
            return new FileChunkStream(fileStream, _offset, _length);
        }
    }
}
