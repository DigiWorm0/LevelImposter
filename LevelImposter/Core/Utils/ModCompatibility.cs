using BepInEx.Unity.IL2CPP;

namespace LevelImposter.Core.Utils;

public static class ModCompatibility
{
    public static bool IsVentCompatibilityEnabled { get; private set; }

    public static void Init()
    {
        IsVentCompatibilityEnabled = IsPlugin("me.eisbison.theotherroles") ||
                                     IsPlugin("auavengers.tou.mira");

        if (IsPlugin("Submerged"))
            LILogger.Warn("LevelImposter detected Submerged installed, currently unsupported");
    }

    private static bool IsPlugin(string guid)
    {
        return IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out _);
    }
}