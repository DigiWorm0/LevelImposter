using BepInEx.Unity.IL2CPP;

namespace LevelImposter.Core;

public static class ModCompatibility
{
    public const string REACTOR_ID = "gg.reactor.api";
    public const string TOR_GUID = "me.eisbison.theotherroles";
    public const string TOU_GUID = "com.slushiegoose.townofus";
    public const string REW_GUID = "me.alchlcdvl.reworked";
    public const string SUBMERGED_GUID = "Submerged";

    public static bool IsTOREnabled { get; private set; }

    public static bool IsTOUEnabled { get; private set; }

    public static bool IsSubmergedEnabled { get; private set; }

    public static bool IsReworkedEnabled { get; private set; }

    public static void Init()
    {
        IsTOREnabled = IsPlugin(TOR_GUID);
        IsTOUEnabled = IsPlugin(TOU_GUID);
        IsSubmergedEnabled = IsPlugin(SUBMERGED_GUID);
        IsReworkedEnabled = IsPlugin(REW_GUID);

        if (IsTOREnabled)
            LILogger.Info("LevelImposter detected TOR installed, compatibility enabled");
        if (IsTOUEnabled)
            LILogger.Info("LevelImposter detected TOU installed, compatibility enabled");
        if (IsReworkedEnabled)
            LILogger.Info("LevelImposter detected Reworked installed, compatibility enabled");
        if (IsSubmergedEnabled)
            LILogger.Info("LevelImposter detected Submerged installed, currently unsupported");
    }

    private static bool IsPlugin(string guid)
    {
        return IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out _);
    }
}