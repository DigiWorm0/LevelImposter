using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Models
{
    class Minimap
    {
        public Minimap(MapBehaviour behaviour)
        {
            this.behaviour = behaviour;
            this.prefab = behaviour.gameObject;
        }

        public GameObject prefab;
        public MapBehaviour behaviour;
    }
}
