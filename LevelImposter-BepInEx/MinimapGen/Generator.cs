using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.MinimapGen
{
    interface Generator
    {
        public void Generate(MapAsset asset);
        public void Finish();
    }
}
