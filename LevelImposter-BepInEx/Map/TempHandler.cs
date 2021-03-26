using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    static class TempHandler
    {
        public static void Create(Polus polus)
        {
            // Unlink Tasks / Utils
            polus.shipStatus.AllCameras         = new UnhollowerBaseLib.Il2CppReferenceArray<SurvCamera>(0);
            polus.shipStatus.AllDoors           = new UnhollowerBaseLib.Il2CppReferenceArray<PlainDoor>(0);
            polus.shipStatus.AllConsoles        = new UnhollowerBaseLib.Il2CppReferenceArray<Console>(0);
            polus.shipStatus.AllRooms           = new UnhollowerBaseLib.Il2CppReferenceArray<PlainShipRoom>(0);
            polus.shipStatus.AllStepWatchers    = new UnhollowerBaseLib.Il2CppReferenceArray<IStepWatcher>(0);
            polus.shipStatus.AllVents           = new UnhollowerBaseLib.Il2CppReferenceArray<Vent>(0);
            polus.shipStatus.DummyLocations     = new UnhollowerBaseLib.Il2CppReferenceArray<Transform>(0);
            polus.shipStatus.SpecialTasks       = new UnhollowerBaseLib.Il2CppReferenceArray<PlayerTask>(0);
            polus.shipStatus.CommonTasks        = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            polus.shipStatus.LongTasks          = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            polus.shipStatus.NormalTasks        = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            polus.shipStatus.FastRooms          = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, PlainShipRoom>();
            polus.shipStatus.MeetingSpawnCenter = new Vector2(0, 0);
            polus.shipStatus.MeetingSpawnCenter2= new Vector2(0, 0);

            // Move Children
            Transform polusTransform = polus.gameObject.transform;
            GameObject temp = new GameObject("temp");
            for (int i = polusTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = polusTransform.GetChild(i);
                child.SetParent(temp.transform);
            }
            temp.transform.SetParent(polusTransform);

            // Remove Map Hud
            GameObject.Destroy(GameObject.Find("MapButton"));
        }

        public static void Clear()
        {
            // Destory Temp
            GameObject.Destroy(
                GameObject.Find("temp")
            );
        }
    }
}
