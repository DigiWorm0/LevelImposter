using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    class FloatBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-blankfloat")
                return;

            LIFloat objFloat = obj.AddComponent<LIFloat>();
            objFloat.Init(elem);
        }

        public void PostBuild() { }
    }
}