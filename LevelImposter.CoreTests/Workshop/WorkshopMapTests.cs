using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using LevelImposter.CoreTests.Infrastructure;
using LevelImposter.MapFormat.Serialization;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.CoreTests.Workshop;

public class WorkshopMapTests
{
    public static TheoryData<string> WorkshopMaps
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var map in WorkshopManifest.Load().Maps)
                data.Add(map.Id);
            return data;
        }
    }

    [Fact]
    public void Manifest_ShouldContainValidPinnedMaps()
    {
        var manifest = WorkshopManifest.Load();

        manifest.SchemaVersion.ShouldBe(1);
        manifest.Maps.ShouldNotBeEmpty();

        var ids = new HashSet<Guid>();
        foreach (var entry in manifest.Maps)
        {
            Guid.TryParse(entry.Id, out var id).ShouldBeTrue(entry.Id);
            ids.Add(id).ShouldBeTrue($"Duplicate map ID in workshop manifest: {entry.Id}");

            Path.GetFileName(entry.FileName).ShouldBe(entry.FileName);
            entry.FileName.ShouldStartWith(entry.Id);
            entry.Name.ShouldNotBeNullOrWhiteSpace();
            entry.AuthorName.ShouldNotBeNullOrWhiteSpace();
            entry.Format.ShouldNotBeNullOrWhiteSpace();
            entry.SizeBytes.ShouldNotBeNull();
            entry.Sha256!.ShouldMatch("^[a-fA-F0-9]{64}$");
        }
    }

    [Theory]
    [MemberData(nameof(WorkshopMaps))]
    public void CachedWorkshopMap_ShouldSatisfyStructuralInvariants(string mapID)
    {
        var entry = WorkshopManifest.Load().Maps.Single(map => map.Id == mapID);
        if (!File.Exists(entry.CachePath))
            Assert.Skip($"Map {entry.Id} is not cached. Run scripts/fetch-map-corpus.py.");

        var actualSha = ComputeSha256(entry.CachePath);
        var baselineStale = !string.Equals(actualSha, entry.Sha256, StringComparison.OrdinalIgnoreCase);
        if (baselineStale)
            Console.WriteLine(
                $"WARNING baseline-stale for {entry.Id}: cached SHA-256 {actualSha} differs from manifest {entry.Sha256}. " +
                "Running workshop structural invariants only.");
        else
            new FileInfo(entry.CachePath).Length.ShouldBe(entry.SizeBytes!.Value);

        using var formatStream = File.OpenRead(entry.CachePath);
        var format = LIDeserializer.IdentifyFormat(formatStream);
        if (!baselineStale)
            format.ToString().ShouldBe(entry.Format);

        // Legacy deserialization may rewrite the source map when FilePath is provided.
        // Use memory-backed asset stores for legacy maps and lazy file-backed stores otherwise.
        var filePath = format == MapFileFormat.Legacy ? "" : entry.CachePath;
        using var mapStream = File.OpenRead(entry.CachePath);
        var success = LIDeserializer.TryDeserializeMap(
            mapStream,
            new MapLoadOptions { LoadAssetDB = true, FilePath = filePath },
            out var map,
            out var result);

        success.ShouldBeTrue($"{entry.Id} ({entry.Name}): {result.Error}");
        result.Success.ShouldBeTrue();
        result.Format.ShouldBe(format);
        map.ShouldNotBeNull();
        map.id.ShouldBe(entry.Id);

        if (!baselineStale)
            map.v.ShouldBe(entry.MapVersion);

        map.elements.ShouldNotBeNull();
        map.properties.ShouldNotBeNull();
        map.spriteAtlases.ShouldNotBeNull();

        var elementIDs = new HashSet<Guid>();
        foreach (var element in map.elements)
        {
            element.ShouldNotBeNull();
            element.id.ShouldNotBe(Guid.Empty);
            elementIDs.Add(element.id).ShouldBeTrue(
                $"{entry.Id} contains duplicate element ID {element.id}");
            element.properties.ShouldNotBeNull();
        }

        map.MapAssetDB.ShouldNotBeNull();
        result.AssetCount.ShouldBe(map.MapAssetDB.DB.Count);
        foreach (var (assetID, assetStore) in map.MapAssetDB.DB)
        {
            assetID.ShouldNotBe(Guid.Empty);
            using var assetStream = assetStore.OpenStream();
            assetStream.CanRead.ShouldBeTrue();
            assetStream.Length.ShouldBeGreaterThan(0);
            assetStore.Peek(16).Length.ShouldBe(16);
        }
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
