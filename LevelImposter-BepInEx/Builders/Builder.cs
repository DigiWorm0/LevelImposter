using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    interface Builder
    {
        /**
         *  Builds MapAsset before ShipStatus.OnEnable
         */
        public bool PreBuild(MapAsset asset);

        /**
         *  Wraps up every up after ShipStatus.OnEnable
         */
        public bool PostBuild();
    }
}
