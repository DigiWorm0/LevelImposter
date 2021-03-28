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
    [BepInPlugin(ID)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class MainHarmony : BasePlugin
    {
        public const string ID = "com.DigiWorm.LevelImposter";

        public Harmony Harmony { get; } = new Harmony(ID);

        public override void Load()
        {
            LILogger.Init();
            AssetDB.Init();

            Harmony.PatchAll();
            LILogger.LogMsg("LevelImposter Initialized.");
        }
    }
}
