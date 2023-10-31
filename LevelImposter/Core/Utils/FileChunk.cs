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

            LILogger.Info($"({offset} + {length}) - " + ToString());
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

        public override string ToString()
        {
            using (var stream = OpenStream())
            {
                byte[] beginning = new byte[10];
                byte[] ending = new byte[10];

                stream.Read(beginning, 0, beginning.Length);
                stream.Seek(ending.Length, SeekOrigin.End);
                stream.Read(ending, 0, ending.Length);

                string beginningString = System.Text.Encoding.UTF8.GetString(beginning);
                string endingString = System.Text.Encoding.UTF8.GetString(ending);
                return $"{beginningString}...{endingString}";

                //return reader.ReadToEnd();
            }
        }
    }
}
