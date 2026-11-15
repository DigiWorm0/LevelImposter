using System;
using System.IO;
using System.Text;
using LevelImposter.MapFormat.Serialization;

namespace LevelImposter.CoreTests.Serialization;

public class MapFormatDetectionTests
{
    private static readonly byte[] Lim2Signature = "LIM2"u8.ToArray();
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];

    [Fact]
    public void IdentifyFormat_ShouldDetectSignaturedLIM2()
    {
        using var stream = CreateStream(Lim2Signature, LengthPrefix(2), "{}"u8.ToArray());

        LIDeserializer.IdentifyFormat(stream).ShouldBe(MapFileFormat.LIM2_Signature);
        stream.Position.ShouldBe(0);
    }

    [Fact]
    public void IdentifyFormat_ShouldDetectUnsignedLIM2()
    {
        using var stream = CreateStream(LengthPrefix(2), "{}"u8.ToArray());

        LIDeserializer.IdentifyFormat(stream).ShouldBe(MapFileFormat.LIM2);
    }

    [Fact]
    public void IdentifyFormat_ShouldDetectZIP()
    {
        using var stream = CreateStream(ZipSignature, new byte[16]);

        LIDeserializer.IdentifyFormat(stream).ShouldBe(MapFileFormat.LIM2_ZIP);
    }

    [Fact]
    public void IdentifyFormat_ShouldDetectLegacyJSON()
    {
        using var stream = CreateStream(Encoding.UTF8.GetBytes("""{"v":2,"elements":[]}"""));

        LIDeserializer.IdentifyFormat(stream).ShouldBe(MapFileFormat.Legacy);
    }

    [Fact]
    public void IdentifyFormat_ShouldRejectStreamsShorterThanSignature()
    {
        using var stream = CreateStream([0x4C, 0x49]);

        Should.Throw<InvalidDataException>(() => LIDeserializer.IdentifyFormat(stream));
    }

    [Fact]
    public void IdentifyFormat_ShouldRejectUnseekableStreams()
    {
        using var stream = new UnseekableStream();

        Should.Throw<InvalidDataException>(() => LIDeserializer.IdentifyFormat(stream));
    }

    [Fact]
    public void TryDeserializeMap_ShouldReportUnknownFormatInsteadOfThrowing()
    {
        // Defaults to LIM2, whose first four bytes are the JSON length.
        using var stream = CreateStream([0x00, 0x00, 0x00, 0x00, 0x01]);

        var success = LIDeserializer.TryDeserializeMap(
            stream,
            new MapLoadOptions(),
            out var map,
            out var result);

        success.ShouldBeFalse();
        map.ShouldBeNull();
        result.Success.ShouldBeFalse();
        result.Format.ShouldBe(MapFileFormat.LIM2);
        result.Error.ShouldNotBeNullOrEmpty();
        result.Exception.ShouldNotBeNull();
    }

    private static byte[] LengthPrefix(int length)
    {
        return BitConverter.GetBytes(length);
    }

    private static MemoryStream CreateStream(params byte[][] chunks)
    {
        var data = new MemoryStream();
        foreach (var chunk in chunks)
            data.Write(chunk);
        data.Position = 0;
        return data;
    }

    private sealed class UnseekableStream : MemoryStream
    {
        public override bool CanSeek => false;

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
