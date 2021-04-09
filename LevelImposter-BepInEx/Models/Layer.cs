using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Models
{
    enum Layer
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Water = 4,
        UI,
        Players = 8,
        Ship,
        Shadow,
        Objects,
        ShortObjects,
        IlluminatedBlocking,
        Ghost,
        UICollider,
        DrawShadows
    }
}
