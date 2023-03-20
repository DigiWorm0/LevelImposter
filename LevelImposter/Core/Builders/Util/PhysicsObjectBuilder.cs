using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class PhysicsObjectBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            /*
            if (elem.type != "util-physics")
                return;

            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.mass = elem.properties.mass ?? 10.0f;
            rb.drag = elem.properties.drag ?? 100.0f;
            rb.freezeRotation = elem.properties.freezeRotation ?? true;
            rb.gravityScale = 0;

            obj.layer = (int)Layer.Default;
            */
            // TODO: Sync physics objects over network
        }

        public void PostBuild() { }
    }
}