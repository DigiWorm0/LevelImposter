using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    class AssetBuilder : Builder
    {
        private PolusHandler  polus;
        private TaskBuilder   taskBuilder;
        private CustomBuilder customBuilder;
        private DummyBuilder  spawnBuilder;
        private UtilBuilder   utilBuilder;

        public AssetBuilder(PolusHandler polus)
        {
            this.polus = polus;

            taskBuilder   = new TaskBuilder(polus);
            customBuilder = new CustomBuilder(polus);
            spawnBuilder  = new DummyBuilder(polus);
            utilBuilder   = new UtilBuilder(polus);
        }

        public bool Build(MapAsset asset)
        {
            try
            {
                if (asset.type == "existing")
                {
                    if (asset.data == "util-player")
                        return spawnBuilder.Build(asset);
                    else if (asset.data.StartsWith("util-"))
                        return utilBuilder.Build(asset);
                    else
                        return taskBuilder.Build(asset);
                }
                else if (asset.type == "custom")
                    return customBuilder.Build(asset);
                return false;
            }
            catch (Exception e)
            {
                LILogger.LogInfo(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        public static UnhollowerBaseLib.Il2CppReferenceArray<T> AddToArr<T>(UnhollowerBaseLib.Il2CppReferenceArray<T> arr, T value) where T : UnhollowerBaseLib.Il2CppObjectBase
        {
            List<T> list = new List<T>(arr);
            list.Add(value);
            return list.ToArray();
        }
    }
}
