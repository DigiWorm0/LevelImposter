using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
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

            // Teleporter
            LITeleporter tele = obj.AddComponent<LITeleporter>();
            tele.SetElement(elem);
        }

        public void PostBuild() { }
    }
}