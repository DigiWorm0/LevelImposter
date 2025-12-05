using System;
using System.IO;
using UnityEngine;

namespace LevelImposter.Core;

public static class StreamExtensions
{
    /// <summary>
    /// Copies data from the given stream into memory as a data block.
    /// This implementation uses a <see cref="PoolableMemoryBlock"/> to minimize allocations.
    /// 
    /// Note: This requires the Stream implementation to support <c>Length</c> property.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The created data block.</returns>
    /// <exception cref="InvalidOperationException">If the stream is too large or reading fails.</exception>
    public static IMemoryBlock ToDataBlock(this Stream stream)
    {
        // Check for int overflow
        if (stream.Length > int.MaxValue)
            throw new InvalidOperationException("Stream is too large to fit in a PoolableDataBlock.");
        
        // Create a new PoolableDataBlock
        var block = new PoolableMemoryBlock((int)stream.Length);
        var data = block.Get();

        // Read the stream into the data block
        var totalRead = 0;
        while (totalRead < data.Length)
        {
            var bytesRead = stream.Read(data, totalRead, data.Length - totalRead);
            if (bytesRead == 0)
                break;
            totalRead += bytesRead;
        }
        
        // Note: It's typical for streams to not read the full length requested and will not throw.
        return block;
    }
}