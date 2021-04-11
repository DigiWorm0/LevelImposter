using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.Models;
using Reactor;

namespace LevelImposter
{
    [BepInPlugin(ID, "LevelImposter", VERSION)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class MainHarmony : BasePlugin
    {
        public const string VERSION = "0.1.4";
        public const string ID = "com.DigiWorm.LevelImposter";

        public Harmony Harmony { get; } = new Harmony(ID);

        public override void Load()
        {
            LILogger.Init();
            VersionCheck.CheckVersion();
            VersionCheck.CheckNewtonsoft();
            AssetDB.Init();
            Harmony.PatchAll();
            LILogger.LogMsg("LevelImposter Initialized.");
        }
    }
}
