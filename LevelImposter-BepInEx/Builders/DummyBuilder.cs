using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    class DummyBuilder : Builder
    {
        private PolusHandler polus;

        public DummyBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool PreBuild(MapAsset asset)
        {
            if (asset.type != "util-player")
                return true;
            GameObject dummy = new GameObject("Dummy");
            dummy.transform.SetParent(polus.gameObject.transform);
            dummy.transform.position = new Vector2(asset.x, -asset.y - PolusHandler.Y_OFFSET);

            polus.shipStatus.DummyLocations = AssetHelper.AddToArr(polus.shipStatus.DummyLocations, dummy.transform);
            return true;
        }

        public bool PostBuild()
        {
            return true;
        }
    }
}
