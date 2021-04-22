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
        private PolusHandler    polus;
        private List<Builder>   builders;

        public AssetBuilder(PolusHandler polus)
        {
            this.polus = polus;

            this.builders = new List<Builder> {
                new TaskBuilder(polus),
                new CustomBuilder(polus),
                new DummyBuilder(polus),
                new UtilBuilder(polus),
                new DecBuilder(polus),
                new RoomBuilder(polus),
                new SabBuilder(polus),
                new ShipRoomBuilder(polus),
                new VentBuilder(polus),
                new CamBuilder(polus),
                new SpawnBuilder(polus)
            };
        }

        public bool PreBuild(MapAsset asset)
        {
            return RouteBuild(asset, false);
        }

        public bool PostBuild()
        {
            return RouteBuild(null, true);
        }

        private bool RouteBuild(MapAsset asset, bool isPost)
        {
            try
            {
                foreach (Builder builder in builders)
                {
                    bool success = false;

                    if (isPost)
                        success = builder.PostBuild();
                    else
                        success = builder.PreBuild(asset);

                    if (!success)
                        return false;
                }
                return true;
            }
            catch (Exception e)
            {
                LILogger.LogInfo(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }
    }
}
