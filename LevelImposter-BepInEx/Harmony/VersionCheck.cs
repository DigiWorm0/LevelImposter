using LevelImposter.Models;
using System;
using UnityEngine;

namespace LevelImposter
{
    public static class VersionCheck
    {
        public const string AMONG_US_VERSION = "2021.4.14";

        public static bool CheckVersion()
        {
            if (Application.version != AMONG_US_VERSION)
            {
                LILogger.LogWarn("Warning: This version of LevelImposter is meant for Among Us " + AMONG_US_VERSION + ". You may experience unexpected behavior!");
                return false;
            }
            return true;
        }

        public static void CheckNewtonsoft()
        {
            var sampleObj = new Point();

            // Errors if dll isn't loaded, continues if it is
            Newtonsoft.Json.JsonConvert.SerializeObject(sampleObj);
        }
    }
}
