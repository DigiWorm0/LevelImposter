using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    class MapApplicator
    {

        public MapApplicator()
        {

        }

        public void Apply(PolusShipStatus shipStatus)
        {
            if (!MapHandler.Load())
                return;

            MapData map = MapHandler.GetMap();
            Polus polus = new Polus(shipStatus);

            ClearMap(polus);
        }

        private void ClearMap(Polus polus)
        {
            polus.shipStatus.AllDoors = new UnhollowerBaseLib.Il2CppReferenceArray<PlainDoor>(0);
        }
    }
}
