using LevelImposter.Models;
using UnityEngine;

namespace LevelImposter
{
    public static class VersionCheck
    {
        public const string AMONG_US_VERSION = "2021.3.31";

        public static bool CheckVersion()
        {
            if (Application.version != AMONG_US_VERSION)
            {
                LILogger.LogWarn("Warning: This version of LevelImposter is meant for Among Us " + AMONG_US_VERSION + ". You may experience unexpected behavior!");
                return false;
            }
            return true;
        }

        public static bool CheckNewtonsoft()
        {
            var sampleObj = new Point();
            try
            {
                Newtonsoft.Json.JsonConvert.SerializeObject(sampleObj);
                return true;
            }
            catch
            {
                LILogger.LogError("Cannot detect Json.Newtonsoft. Map files may not import correctly.");
                return false;
            }
        }
    }
}
