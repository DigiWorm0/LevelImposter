using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class TaskBuilder : Builder
    {
        private PolusHandler polus;
        private GameObject taskMgr;

        public TaskBuilder(PolusHandler polus)
        {
            this.polus = polus;
            taskMgr = new GameObject("TaskManager");
        }

        public bool Build(MapAsset asset)
        {
            AssetData original = AssetDB.Get(asset.data);

            // Object
            GameObject obj = new GameObject(asset.data);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = original.spriteRenderer.sprite;
            spriteRenderer.material = original.spriteRenderer.material;

            // Console
            Console origConsole = original.mapObj.GetComponent<Console>();
            Console console = obj.AddComponent<Console>();
            console.ConsoleId = origConsole.ConsoleId;
            console.AllowImpostor = false;
            console.checkWalls = false;
            console.GhostsIgnored = false;
            console.Image = spriteRenderer;
            console.onlyFromBelow = true;
            console.onlySameRoom = false;
            console.usableDistance = 1;
            console.TaskTypes  = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;
            polus.Add(obj, asset);

            // Box Collider
            if (original.mapObj.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D origBox = original.mapObj.GetComponent<CircleCollider2D>();
                CircleCollider2D box = obj.AddComponent<CircleCollider2D>();
                box.radius = origBox.radius;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            else
            {
                BoxCollider2D origBox = original.mapObj.GetComponent<BoxCollider2D>();
                BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
                box.size = origBox.size;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }

            // Button
            PassiveButton origBtn = original.mapObj.GetComponent<PassiveButton>();
            PassiveButton btn = obj.AddComponent<PassiveButton>();
            btn.ClickMask = origBtn.ClickMask;
            btn.OnMouseOver = new UnityEvent();
            btn.OnMouseOut = new UnityEvent();
            Action action = console.Use;
            btn.OnClick.AddListener(action);

            // Task
            NormalPlayerTask origTask = (NormalPlayerTask)original.shipBehavior;
            NormalPlayerTask task = taskMgr.AddComponent<NormalPlayerTask>();
            //task.Arrow = origTask.Arrow;
            task.taskStep = origTask.taskStep;
            task.MaxStep = origTask.MaxStep;
            task.arrowSuspended = origTask.arrowSuspended;
            task.ShowTaskTimer = origTask.ShowTaskTimer;
            task.ShowTaskStep = origTask.ShowTaskStep;
            task.TaskTimer = origTask.TaskTimer;
            task.TimerStarted = origTask.TimerStarted;
            task.StartAt = origTask.StartAt;
            task.TaskType = origTask.TaskType;
            task.MinigamePrefab = origTask.MinigamePrefab;
            task.HasLocation = origTask.HasLocation;
            task.LocationDirty = origTask.LocationDirty;

            // Apply to Task List
            polus.shipStatus.AllConsoles = AssetBuilder.AddToArr(polus.shipStatus.AllConsoles, console);
            if (original.objType == ObjType.CommonTask)
                polus.shipStatus.CommonTasks = AssetBuilder.AddToArr(polus.shipStatus.CommonTasks, task);
            if (original.objType == ObjType.ShortTask)
                polus.shipStatus.NormalTasks = AssetBuilder.AddToArr(polus.shipStatus.NormalTasks, task);
            if (original.objType == ObjType.LongTask)
                polus.shipStatus.LongTasks = AssetBuilder.AddToArr(polus.shipStatus.LongTasks, task);
            
            return true;
        }
    }
}
