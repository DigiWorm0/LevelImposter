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
        private static Dictionary<Guid, LITeleporter> _teleporterDb = new Dictionary<Guid, LITeleporter>();

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-tele")
                return;

            // Colliders
            Collider2D[] colliders = obj.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                collider.isTrigger = true;
            }

            // Teleporter
            LITeleporter tele = obj.AddComponent<LITeleporter>();
            tele.CurrentElem = elem;
            _teleporterDb[elem.id] = tele;
        }

        public void PostBuild()
        {
            foreach (var teleporter in _teleporterDb)
            {
                Guid? targetID = teleporter.Value.CurrentElem.properties.teleporter;
                if (targetID != null)
                {
                    LITeleporter target;
                    _teleporterDb.TryGetValue((Guid)targetID, out target);
                    teleporter.Value.CurrentTarget = target;
                }
            }
        }
    }
}