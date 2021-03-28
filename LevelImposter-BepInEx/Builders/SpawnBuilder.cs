using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    class SpawnBuilder : Builder
    {
        private PolusHandler polus;

        public SpawnBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool Build(MapAsset asset)
        {
            Vector2 vector = new Vector2(asset.x, asset.y);
            polus.shipStatus.InitialSpawnCenter  = vector;
            polus.shipStatus.MeetingSpawnCenter  = vector;
            polus.shipStatus.MeetingSpawnCenter2 = vector;
            return true;
        }
    }
}
