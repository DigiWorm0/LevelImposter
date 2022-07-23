using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class TaskBuilder : Builder
    {
        public GameObject taskContainer;
        public int consoleID = 0;

        private Dictionary<string, int> consoleIDPairs = new Dictionary<string, int> {
            { "task-garbage2", 1 },
            { "task-garbage3", 0 },
            { "task-garbage4", 2 },
            { "task-waterwheel1", 0 },
            { "task-waterwheel2", 1 },
            { "task-waterwheel3", 2 },
            { "task-fans1", 0 },
            { "task-fans2", 1 },
        };

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("task-"))
                return;

            if (taskContainer == null)
            {
                taskContainer = new GameObject("Tasks");
                taskContainer.transform.SetParent(LIShipStatus.Instance.transform);
            }

            TaskData taskData = AssetDB.tasks[elem.type];
            ShipStatus shipStatus = LIShipStatus.Instance.shipStatus;

            // Default Sprite
            obj.layer = (int)Layer.ShortObjects;
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = taskData.SpriteRenderer.sprite;
            }
            spriteRenderer.material = taskData.SpriteRenderer.material;

            // Parent
            SystemTypes systemType = 0;
            if (elem.properties.parent != null)
                systemType = RoomBuilder.GetRoom((Guid)elem.properties.parent);

            // Console
            Console console;
            Console origConsole = taskData.GameObj.GetComponent<Console>();
            if (elem.type == "task-pistols1" || elem.type == "task-rifles1")
            {
                console = obj.AddComponent<StoreArmsTaskConsole>();
                StoreArmsTaskConsole specialConsole = console.Cast<StoreArmsTaskConsole>();
                StoreArmsTaskConsole origSpecialConsole = origConsole.Cast<StoreArmsTaskConsole>();

                specialConsole.timesUsed = origSpecialConsole.timesUsed;
                specialConsole.Images = origSpecialConsole.Images;
                specialConsole.useSound = origSpecialConsole.useSound;
                specialConsole.usesPerStep = origSpecialConsole.usesPerStep;
            }
            else if (elem.type.StartsWith("task-towels") && elem.type != "task-towels1")
            {
                console = obj.AddComponent<TowelTaskConsole>();
                TowelTaskConsole specialConsole = console.Cast<TowelTaskConsole>();
                TowelTaskConsole origSpecialConsole = origConsole.Cast<TowelTaskConsole>();

                specialConsole.useSound = origSpecialConsole.useSound;
            }
            else
            {
                console = obj.AddComponent<Console>();
            }
            console.ConsoleId = consoleID;
            console.Image = spriteRenderer;
            console.onlyFromBelow = elem.properties.onlyFromBelow == null ? true : (bool)elem.properties.onlyFromBelow;
            console.usableDistance = elem.properties.range == null ? 1.0f : (float)elem.properties.range;
            console.Room = systemType;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;

            if (consoleIDPairs.ContainsKey(elem.type))
            {
                console.ConsoleId = consoleIDPairs[elem.type];
            }
            else
            {
                console.ConsoleId = consoleID;
                consoleID++;
            }

            // Collider
            if (!MapUtils.HasSolidCollider(elem))
                MapUtils.CloneColliders(origConsole.gameObject, obj);

            // Button
            PassiveButton origBtn = taskData.GameObj.GetComponent<PassiveButton>();
            if (origBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = origBtn.ClickMask;
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Task
            if (!string.IsNullOrEmpty(taskData.BehaviorName))
            {
                //MapUtils.Rename(taskData.Behavior.TaskType, elem.name); // TODO: Implement this in a different way...

                GameObject taskHolder = new GameObject(elem.name);
                taskHolder.transform.SetParent(taskContainer.transform);

                NormalPlayerTask origTask = taskData.Behavior;
                NormalPlayerTask task = taskHolder.AddComponent(taskData.Behavior.GetIl2CppType()).Cast<NormalPlayerTask>();
                task.StartAt = systemType;
                task.taskStep = origTask.taskStep;
                task.MaxStep = origTask.MaxStep;
                task.arrowSuspended = origTask.arrowSuspended;
                task.ShowTaskTimer = origTask.ShowTaskTimer;
                task.ShowTaskStep = origTask.ShowTaskStep;
                task.TaskTimer = origTask.TaskTimer;
                task.TimerStarted = origTask.TimerStarted;
                task.TaskType = origTask.TaskType;
                task.MinigamePrefab = origTask.MinigamePrefab;
                task.HasLocation = origTask.HasLocation;

                if (taskData.TaskType == TaskType.Common)
                    shipStatus.CommonTasks = MapUtils.AddToArr(shipStatus.CommonTasks, task);
                if (taskData.TaskType == TaskType.Short)
                    shipStatus.NormalTasks = MapUtils.AddToArr(shipStatus.NormalTasks, task);
                if (taskData.TaskType == TaskType.Long)
                    shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task);
            }

            // Medscan
            if (elem.type == "task-medscan")
            {
                if (shipStatus.MedScanner != null)
                    LILogger.Warn("Warning: Only 1 med scanner can be used per map");
                MedScannerBehaviour medscan = obj.AddComponent<MedScannerBehaviour>();
                shipStatus.MedScanner = medscan;
            }
        }

        public void PostBuild() { }
    }
}
