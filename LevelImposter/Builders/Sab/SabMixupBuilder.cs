using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class SabMixupBuilder : IElemBuilder
    {
        private const SystemTypes MIXUP_TYPE = SystemTypes.MushroomMixupSabotage;

        private static MushroomMixupSabotageSystem? _sabotageSystem = null;
        public static MushroomMixupSabotageSystem? SabotageSystem => _sabotageSystem;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "sab-btnmixup")
                return;

            // Check Already Exists
            if (_sabotageSystem != null)
            {
                LILogger.Warn("Only 1 mushroom mixup sabotage can exist at a time");
                return;
            }

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

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
            LILogger.Info($" + Adding sabotage for {elem}...");
            MushroomMixupSabotageTask task = taskContainer.AddComponent<MushroomMixupSabotageTask>();
            {
                // Properties
                task.StartAt = prefabTask.StartAt;
                task.TaskType = prefabTask.TaskType;
                task.MinigamePrefab = prefabTask.MinigamePrefab;

                // Rename Task
                if (!string.IsNullOrEmpty(elem.properties.description))
                    LIShipStatus.Instance?.Renames.Add(StringNames.MushroomMixupSabotage, elem.properties.description);

                // Add Task
                shipStatus.SpecialTasks = MapUtils.AddToArr(shipStatus.SpecialTasks, task);
            }

            // Screen Tint
            GameObject screenTintObj = new GameObject("Screen Tint");
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
            _sabotageSystem = systemContainer.AddComponent<MushroomMixupSabotageSystem>();
            {
                _sabotageSystem.skinEmptyChance = prefabSystem.skinEmptyChance;
                _sabotageSystem.skinIds = prefabSystem.skinIds;
                _sabotageSystem.hatEmptyChance = prefabSystem.hatEmptyChance;
                _sabotageSystem.hatIds = prefabSystem.hatIds;
                _sabotageSystem.visorEmptyChance = prefabSystem.visorEmptyChance;
                _sabotageSystem.visorIds = prefabSystem.visorIds;
                _sabotageSystem.petEmptyChance = prefabSystem.petEmptyChance;
                _sabotageSystem.petIds = prefabSystem.petIds;
                _sabotageSystem.secondsForAutoHeal = elem.properties.sabDuration ?? 10;
                _sabotageSystem.screenTint = screenTintObj.GetComponent<MushroomMixupScreenTint>();
                _sabotageSystem.playerAnimationPrefab = prefabSystem.playerAnimationPrefab;
                _sabotageSystem.activateSfx = prefabSystem.activateSfx;
                _sabotageSystem.deactivateSfx = prefabSystem.deactivateSfx;
            }

            // Add New System
            var sabSystem = shipStatus.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
            shipStatus.Systems.Add(MIXUP_TYPE, _sabotageSystem.Cast<ISystemType>());
            sabSystem.specials.Add(_sabotageSystem.Cast<IActivatable>());
        }

        public void PostBuild() { }
    }
}
