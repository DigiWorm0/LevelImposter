using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace LevelImposter.RuntimeTests;

[BepInPlugin(PluginGuid, "LevelImposter Runtime Tests", PluginVersion)]
[BepInDependency(LevelImposterGuid, BepInDependency.DependencyFlags.HardDependency)]
[BepInProcess("Among Us.exe")]
public sealed class RuntimeTestsPlugin : BasePlugin
{
    public const string PluginGuid = "com.DigiWorm.LevelImposter.RuntimeTests";
    public const string LevelImposterGuid = "com.DigiWorm.LevelImposter";
    public const string PluginVersion = "0.1.0";

    private readonly Harmony _harmony = new(PluginGuid);

    public override void Load()
    {
        Log.LogInfo($"Runtime test harness loaded ({PluginVersion}).");

        RuntimeTestRegistry.Register<VanillaFreeplayLaunchTest>();
        RuntimeTestRunner.Initialize(Log);
        _harmony.PatchAll();
    }
}
