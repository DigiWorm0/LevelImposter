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
        private DecBuilder    decBuilder;
        private RoomBuilder   roomBuilder;

        public AssetBuilder(PolusHandler polus)
        {
            this.polus = polus;

            taskBuilder   = new TaskBuilder(polus);
            customBuilder = new CustomBuilder(polus);
            spawnBuilder  = new DummyBuilder(polus);
            utilBuilder   = new UtilBuilder(polus);
            decBuilder    = new DecBuilder(polus);
            roomBuilder   = new RoomBuilder(polus);
        }

        public bool Build(MapAsset asset)
        {
            try
            {
                if (asset.spriteType == "existing")
                {
                    if (asset.type == "util-player")
                        return spawnBuilder.Build(asset);
                    else if (asset.type == "util-room")
                        return true;
                    else if (asset.type.StartsWith("util-"))
                        return utilBuilder.Build(asset);
                    else if (asset.type.StartsWith("dec-"))
                        return decBuilder.Build(asset);
                    else if (asset.type.StartsWith("room-"))
                        return roomBuilder.Build(asset);
                    else if (asset.type.StartsWith("task-"))
                        return taskBuilder.Build(asset);
                }
                else if (asset.spriteType == "custom")
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
