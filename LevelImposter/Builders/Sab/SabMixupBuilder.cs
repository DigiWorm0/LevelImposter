using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

public class SabMixupBuilder : IElemBuilder
{
    private const SystemTypes MIXUP_TYPE = SystemTypes.MushroomMixupSabotage;

    public static MushroomMixupSabotageSystem? SabotageSystem { get; private set; }

    public void OnPreBuild()
    {
        SabotageSystem = null;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "sab-btnmixup")
            return;

        // Check Already Exists
        if (SabotageSystem != null)
        {
            LILogger.Warn("Only 1 mushroom mixup sabotage can exist at a time");
            return;
        }

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // ShipStatus Prefab
        var fungleShipStatus = AssetDB.GetObject("ss-fungle")?.GetComponent<FungleShipStatus>();
        if (fungleShipStatus == null)
            return;
        var prefabSystem = fungleShipStatus.specialSabotage;

        // Containers
        var taskContainer = new GameObject("Mixup Sabotage Task");
        taskContainer.transform.SetParent(shipStatus.transform);
        taskContainer.SetActive(false);

        var systemContainer = new GameObject("Mixup Sabotage System");
        systemContainer.transform.SetParent(shipStatus.transform);

        // Prefab
        var prefabTask = AssetDB.GetTask<MushroomMixupSabotageTask>(elem.type);
        if (prefabTask == null)
            return;

        // Create Task
        LILogger.Debug($" + Adding sabotage for {elem}...");
        var task = taskContainer.AddComponent<MushroomMixupSabotageTask>();
        {
            // Properties
            task.StartAt = prefabTask.StartAt;
            task.TaskType = prefabTask.TaskType;
            task.MinigamePrefab = prefabTask.MinigamePrefab;

            // Rename Task
            if (!string.IsNullOrEmpty(elem.properties.description))
                LIShipStatus.GetInstanceOrNull()?.Renames
                    .Add(StringNames.MushroomMixupSabotage, elem.properties.description);

            // Add Task
            shipStatus.SpecialTasks = MapUtils.AddToArr(shipStatus.SpecialTasks, task);
        }

        // Screen Tint
        var screenTintObj = new GameObject("Screen Tint");
        {
            screenTintObj.transform.SetParent(systemContainer.transform);

            // Mesh Filter
            var tintFilter = screenTintObj.AddComponent<MeshFilter>();
            tintFilter.mesh = prefabSystem.screenTint.GetComponent<MeshFilter>().sharedMesh;

            // Mesh Renderer
            var tintRenderer = screenTintObj.AddComponent<MeshRenderer>();
            tintRenderer.sharedMaterial = prefabSystem.screenTint.GetComponent<MeshRenderer>().sharedMaterial;
            tintRenderer.enabled = false;

            // Full Screen Scaler
            screenTintObj.AddComponent<FullScreenScaler>();

            // Screen Tint
            var screenTint = screenTintObj.AddComponent<MushroomMixupScreenTint>();
            screenTint.meshRenderer = tintRenderer;
            screenTint.maxOpacity = prefabSystem.screenTint.maxOpacity;

            // Fix Null Reference Exception
            screenTint.Awake();
            screenTint.enabled = true;
        }

        // Create New System
        SabotageSystem = systemContainer.AddComponent<MushroomMixupSabotageSystem>();
        {
            SabotageSystem.skinEmptyChance = prefabSystem.skinEmptyChance;
            SabotageSystem.skinIds = prefabSystem.skinIds;
            SabotageSystem.hatEmptyChance = prefabSystem.hatEmptyChance;
            SabotageSystem.hatIds = prefabSystem.hatIds;
            SabotageSystem.visorEmptyChance = prefabSystem.visorEmptyChance;
            SabotageSystem.visorIds = prefabSystem.visorIds;
            SabotageSystem.petEmptyChance = prefabSystem.petEmptyChance;
            SabotageSystem.petIds = prefabSystem.petIds;
            SabotageSystem.secondsForAutoHeal = elem.properties.sabDuration ?? 10;
            SabotageSystem.screenTint = screenTintObj.GetComponent<MushroomMixupScreenTint>();
            SabotageSystem.playerAnimationPrefab = prefabSystem.playerAnimationPrefab;
            SabotageSystem.activateSfx = prefabSystem.activateSfx;
            SabotageSystem.deactivateSfx = prefabSystem.deactivateSfx;
        }

        // Add New System
        var sabSystem = shipStatus.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
        shipStatus.Systems.Add(MIXUP_TYPE, SabotageSystem.Cast<ISystemType>());
        sabSystem.specials.Add(SabotageSystem.Cast<IActivatable>());
    }
}