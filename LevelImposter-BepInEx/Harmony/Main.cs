using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;

namespace LevelImposter
{
    [BepInPlugin(ID)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class Main : BasePlugin
    {
        public const string ID = "com.DigiWorm.LevelImposter";

        public Harmony Harmony { get; } = new Harmony(ID);

        public override void Load()
        {
            LILogger.Init();
            Harmony.PatchAll();
            LILogger.LogMsg("LevelImposter Initialized.");
        }
    }
}
