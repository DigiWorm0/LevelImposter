using HarmonyLib;
using LevelImposter.Builders;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using LevelImposter.Core;

namespace LevelImposter.Builders
{
    class TeleBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-tele")
                return;

            // Colliders
            Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
                collider.isTrigger = true;
            if (elem.properties.isGhostEnabled ?? false)
                obj.layer = (int)Layer.Default;

            // Teleporter
            LITeleporter tele = obj.AddComponent<LITeleporter>();
            tele.SetElement(elem);
        }

        public void PostBuild() { }
    }
}