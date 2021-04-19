using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    class TaskData : AssetData
    {
        public TaskType TaskType { get; set; }

        public string SpriteRendererName { get; set; }
        public string GameObjName { get; set; }
        public string BehaviorName { get; set; }
        public float Scale { get; set; }

        public SpriteRenderer SpriteRenderer { get; set; }
        public GameObject GameObj { get; set; }
        public NormalPlayerTask Behavior { get; set; }

        public override void ImportMap(GameObject map, ShipStatus shipStatus)
        {
            SpriteRenderer = MapSearcher.SearchComponent<SpriteRenderer>(map, SpriteRendererName);
            GameObj = MapSearcher.SearchChildren(map, GameObjName);

            if (!string.IsNullOrEmpty(BehaviorName))
            {
                if (TaskType == TaskType.Common)
                    Behavior = MapSearcher.SearchList(shipStatus.CommonTasks, BehaviorName);
                if (TaskType == TaskType.Short)
                    Behavior = MapSearcher.SearchList(shipStatus.NormalTasks, BehaviorName);
                if (TaskType == TaskType.Long)
                    Behavior = MapSearcher.SearchList(shipStatus.LongTasks, BehaviorName);
            }
            
        }
    }

    enum TaskType
    {
        Common,
        Short,
        Long
    }
}
