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

        public bool Build(MapAsset asset)
        {
            GameObject dummy = new GameObject("Dummy");
            dummy.transform.SetParent(polus.gameObject.transform);
            dummy.transform.position = new Vector2(asset.x, -asset.y);

            polus.shipStatus.DummyLocations = AssetBuilder.AddToArr(polus.shipStatus.DummyLocations, dummy.transform);
            return true;
        }
    }
}
