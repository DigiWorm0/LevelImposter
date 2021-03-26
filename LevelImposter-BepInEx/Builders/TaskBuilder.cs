using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    static class TaskBuilder
    {
        public static GameObject Build(string type)
        {
            AssetInfo info = ItemDB.Get(type);
            GameObject obj = new GameObject(type);

            // Sprite
            GameObject clone = GameObject.Find(info.sprite);
            SpriteRenderer cloneRender = clone.GetComponent<SpriteRenderer>();
            Sprite sprite = cloneRender.sprite;
            Material mat = cloneRender.material;

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.material = mat;

            // Console
            Console console = obj.AddComponent<Console>();
            console.AllowImpostor = false;
            console.checkWalls = false;
            console.GhostsIgnored = false;
            console.Image = spriteRenderer;
            console.onlyFromBelow = true;
            console.onlySameRoom = false;
            console.TaskTypes = new UnhollowerBaseLib.Il2CppStructArray<TaskTypes>(0);
            console.usableDistance = 1;

            TaskSet task = new TaskSet();
            task.taskType = (TaskTypes)info.taskID;
            console.ValidTasks = new TaskSet[] { task };

            return obj;
        }
    }
}
