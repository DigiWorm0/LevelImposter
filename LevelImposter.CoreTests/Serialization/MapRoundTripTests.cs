using System;
using System.IO;
using System.Text;
using LevelImposter.MapFormat.Models;
using LevelImposter.MapFormat.Serialization;
using LevelImposter.MapFormat.Storage;
using VerifyXunit;

namespace LevelImposter.CoreTests.Serialization;

public class MapRoundTripTests
{
    private static readonly Guid ElementID = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid AssetID = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly byte[] AssetBytes = [0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x01, 0xFF];

    [Fact]
    public async Task SerializedMap_ShouldPreserveMapData()
    {
        var original = CreateMap();
        using var serialized = new MemoryStream();
        LISerializer.SerializeMap(original, serialized).ShouldBeTrue();
        var originalBytes = serialized.ToArray();

        serialized.Position = 0;
        var loaded = LIDeserializer.TryDeserializeMap(
            serialized,
            new MapLoadOptions { LoadAssetDB = true },
            out var map,
            out var result);

        loaded.ShouldBeTrue(result.Error);
        result.Success.ShouldBeTrue();
        result.Format.ShouldBe(MapFileFormat.LIM2_Signature);
        result.MapID.ShouldBe(original.id);
        result.MapVersion.ShouldBe(LIMap.LIM_VERSION);
        result.ElementCount.ShouldBe(1);
        result.AssetCount.ShouldBe(1);
        map.ShouldNotBeNull();

        map.name.ShouldBe("Test Map");
        var element = map.elements.ShouldHaveSingleItem();
        element.id.ShouldBe(ElementID);
        element.type.ShouldBe("util-layer");
        element.properties.spriteID.ShouldBe(AssetID);
        element.properties.color.ShouldNotBeNull();
        element.properties.color.r.ShouldBe(0.1f);

        using var assetStream = map.MapAssetDB!.Get(AssetID)!.OpenStream();
        assetStream.ToManagedArray().ShouldBe(AssetBytes);

        // Serializing the reloaded map must reproduce the exact same file bytes.
        using var reserialized = new MemoryStream();
        LISerializer.SerializeMap(map, reserialized).ShouldBeTrue();
        reserialized.ToArray().ShouldBe(originalBytes);

        await Verify(ExtractMapJson(originalBytes));
    }

    [Fact]
    public void TryDeserializeMap_ShouldSkipAssetDBWhenNotRequested()
    {
        using var serialized = new MemoryStream();
        LISerializer.SerializeMap(CreateMap(), serialized).ShouldBeTrue();
        serialized.Position = 0;

        var loaded = LIDeserializer.TryDeserializeMap(
            serialized,
            new MapLoadOptions { LoadAssetDB = false },
            out var map,
            out var result);

        loaded.ShouldBeTrue(result.Error);
        result.AssetCount.ShouldBeNull();
        map.ShouldNotBeNull();
        map.MapAssetDB.ShouldBeNull();
    }

    private static LIMap CreateMap()
    {
        var map = new LIMap
        {
            v = LIMap.LIM_VERSION,
            id = "test-map",
            name = "Test Map",
            description = "Round-trip fixture for the offline suite.",
            authorID = "test-author-id",
            authorName = "Test Author",
            MapTargetValue = MapTarget.Game
        };

        map.elements =
        [
            new LIElement
            {
                id = ElementID,
                name = "Floor",
                type = "util-layer",
                x = 1.5f,
                y = -2.25f,
                z = 0f,
                xScale = 1f,
                yScale = 2f,
                rotation = 90f,
                properties = new LIProperties
                {
                    spriteID = AssetID,
                    color = new LIColor { r = 0.1f, g = 0.2f, b = 0.3f, a = 0.4f },
                    triggerTime = 1.25f
                }
            }
        ];

        map.MapAssetDB = new MapAssetDB();
        map.MapAssetDB.Add(AssetID, new MemoryStore(AssetBytes));
        return map;
    }

    private static string ExtractMapJson(byte[] fileBytes)
    {
        var jsonLength = BitConverter.ToInt32(fileBytes, 4);
        return Encoding.UTF8.GetString(fileBytes, 8, jsonLength);
    }
}
