namespace LevelImposter.CoreTests.Infrastructure;

/// <summary>
///     Resolves repository paths for files the tests share with the mod
///     (translations, map fixtures, and the asset database).
/// </summary>
public static class TestPaths
{
    /// <summary>
    ///     Environment variable that overrides repository discovery, for CI or
    ///     out-of-tree test runs.
    /// </summary>
    public const string RepoRootVariable = "LI_TEST_REPO_ROOT";

    public static string RepoRoot { get; } = FindRepoRoot();

    public static string AssetsDirectory => Path.Combine(RepoRoot, "LevelImposter", "Assets");

    public static string I18nDirectory => Path.Combine(AssetsDirectory, "i18n");

    public static string MapCorpusDirectory => Path.Combine(RepoRoot, "LevelImposter.CoreTests", "Corpus");

    public static string WorkshopManifestPath => Path.Combine(MapCorpusDirectory, "manifest.json");

    public static string WorkshopCacheDirectory =>
        Path.Combine(RepoRoot, "LevelImposter.CoreTests", ".cache", "maps");

    private static string FindRepoRoot()
    {
        var configuredRoot = Environment.GetEnvironmentVariable(RepoRootVariable);
        if (!string.IsNullOrWhiteSpace(configuredRoot) && IsRepoRoot(configuredRoot))
            return Path.GetFullPath(configuredRoot);

        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
            if (IsRepoRoot(directory.FullName))
                return directory.FullName;

        throw new InvalidOperationException(
            $"Could not locate LevelImposter.sln above {AppContext.BaseDirectory}. " +
            $"Set {RepoRootVariable} to the repository root.");
    }

    private static bool IsRepoRoot(string directory)
    {
        return File.Exists(Path.Combine(directory, "LevelImposter.sln"));
    }

    /// <summary>
    ///     Whether a file in the asset tree is named like a packaged resource.
    /// </summary>
    public static string AssetPath(string resourceName)
    {
        // Packaged resource names replace every folder with a dot, except the
        // extension separator: i18n.en-us.json -> i18n/en-us.json.
        var segments = resourceName.Split('.');
        if (segments.Length < 2)
            return Path.Combine(AssetsDirectory, resourceName);

        var extension = segments[^1];
        var relativePath = Path.Combine(segments[..^1]) + "." + extension;
        return Path.Combine(AssetsDirectory, relativePath);
    }

    /// <summary>
    ///     Loads a packaged text resource directly from the repository, matching
    ///     <c>PackagedResources.LoadText</c> behavior for tests.
    /// </summary>
    public static string? LoadAssetText(string resourceName)
    {
        var path = AssetPath(resourceName);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}