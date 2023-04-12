using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LevelImposter.DB;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Core
{
    /// <summary>
    /// Stores and applies any
    /// minigame sprite data
    /// </summary>
    public class MinigameSprites : MonoBehaviour
    {
        public MinigameSprites(IntPtr intPtr) : base(intPtr)
        {
        }

        private LIMinigameSprite[]? _minigameDataArr = null;
        private LIMinigameProps? _minigameProps = null;

        /// <summary>
        /// Initializes component with LIElement
        /// </summary>
        /// <param name="elem">Element that GameObject represents</param>
        [HideFromIl2Cpp]
        public void Init(LIElement elem)
        {
            _minigameDataArr = elem.properties.minigames ?? new LIMinigameSprite[0];
            _minigameProps = elem.properties.minigameProps ?? new();
        }

        /// <summary>
        /// Loads the sprites onto a minigame
        /// </summary>
        /// <param name="minigame">Minigame to load sprites to</param>
        public void LoadMinigame(Minigame minigame)
        {
            try
            {
                var divertMinigame = minigame.TryCast<DivertPowerMetagame>();
                if (divertMinigame != null)
                    return;

                LoadMinigameProps(minigame);
                if (_minigameDataArr == null)
                    return;
                foreach (LIMinigameSprite minigameData in _minigameDataArr)
                {
                    SpriteLoader.Instance?.LoadSpriteAsync(minigameData.spriteData, (spriteData) => {
                        LoadMinigameSprite(minigame, minigameData.type, spriteData?.Sprite);
                    }, minigameData.id.ToString());
                }
            }
            catch (Exception e)
            {
                LILogger.Error($"Error while editing minigame:\n{e}");
            }
        }

        /// <summary>
        /// Loads all props into a minigame
        /// </summary>
        private void LoadMinigameProps(Minigame minigame)
        {
            bool isLights = _minigameProps?.lightsColorOn != null || _minigameProps?.lightsColorOff != null;
            bool isReactor = _minigameProps?.reactorColorBad != null || _minigameProps?.reactorColorGood != null;
            bool isFuel = _minigameProps?.fuelColor != null || _minigameProps?.fuelBgColor != null;
            LILogger.Info($"Loading minigame props for {minigame}");

            // Lights Panel
            if (isLights)
            {
                var lightsMinigame = minigame.Cast<SwitchMinigame>();
                lightsMinigame.OnColor = _minigameProps?.lightsColorOn?.ToUnity() ?? lightsMinigame.OnColor;
                lightsMinigame.OffColor = _minigameProps?.lightsColorOn?.ToUnity() ?? lightsMinigame.OffColor;
                LILogger.Info("Applied Light Props");
            }

            // Reactor Panel
            if (isReactor)
            {
                var reactorMinigame = minigame.Cast<ReactorMinigame>();
                reactorMinigame.good = _minigameProps?.reactorColorGood?.ToUnity() ?? reactorMinigame.good;
                reactorMinigame.bad = _minigameProps?.reactorColorBad?.ToUnity() ?? reactorMinigame.bad;
                LILogger.Info("Applied Reactor Props");
            }

            // Fuel Task
            if (isFuel)
            {
                var multistageMinigame = minigame.Cast<MultistageMinigame>();
                foreach (var stage in multistageMinigame.Stages)
                {
                    var fuelRenderer1 = stage.transform.Find("DestGauge/BackFillMask/BackFillColor").GetComponent<SpriteRenderer>();
                    fuelRenderer1.color = _minigameProps?.fuelColor?.ToUnity() ?? fuelRenderer1.color;

                    var bgRenderer = stage.transform.Find("blank").GetComponent<SpriteRenderer>();
                    bgRenderer.color = _minigameProps?.fuelBgColor?.ToUnity() ?? bgRenderer.color;

                    // Only on Stage 2
                    var fuelRenderer2 = stage.transform.Find("SrcGauge/BackFillMask/BackFillColor")?.GetComponent<SpriteRenderer>();
                    if (fuelRenderer2 != null)
                        fuelRenderer2.color = _minigameProps?.fuelColor?.ToUnity() ?? fuelRenderer2.color;
                }
                LILogger.Info("Applied Fuel Props");
            }
        }

        /// <summary>
        /// Loads individual sprites onto a minigame
        /// </summary>
        /// <param name="minigame">Minigame to load sprite to</param>
        /// <param name="type">Type of LIMinigame</param>
        /// <param name="sprite">Sprite to load</param>
        private void LoadMinigameSprite(Minigame minigame, string type, Sprite? sprite)
        {
            try
            {
                if (!LoadMinigameFieldSprite(minigame, type, sprite))
                    return;

                // Get all sprite paths
                string[]? spritePaths = AssetDB.GetPaths(type);
                if (spritePaths == null)
                    return;

                // Iterate through sprite path
                foreach (string path in spritePaths)
                {
                    LILogger.Info($"Loading minigame sprite {type} at '{path}'");
                    var spriteObjs = MapUtils.GetTransforms(path, minigame.transform);
                    if (spriteObjs.Count <= 0)
                    {
                        LILogger.Warn($"Could not find {type} at '{path}'");
                        continue;
                    }

                    // Iterate through objects located at path
                    foreach (var spriteObj in spriteObjs)
                    {
                        var spriteRenderer = spriteObj?.GetComponent<SpriteRenderer>();
                        if (spriteRenderer == null)
                        {
                            LILogger.Warn($"{type} SpriteRenderer is null at '{path}'");
                            continue;
                        }
                        spriteRenderer.sprite = sprite;
                    }
                }

                // Fixes a bug with task-telescope
                if (type.StartsWith("task-telescope"))
                {
                    var telescopeMinigame = minigame.Cast<TelescopeGame>();
                    var telescopeRenderer = telescopeMinigame.TargetItem.GetComponent<SpriteRenderer>();
                    telescopeMinigame.ItemDisplay.sprite = telescopeRenderer.sprite;
                }
            }
            catch (Exception e)
            {
                LILogger.Error($"Error while editing sprite {type}:\n{e}");
            }
        }

        /// <summary>
        /// Gets the index from an underscore seperated minigame type
        /// </summary>
        /// <param name="type">Minigame type</param>
        /// <returns>Index appended to the end</returns>
        private int GetIndex(string type)
        {
            var splitType = type.Split("_");
            if (splitType.Length > 2)
                return int.Parse(splitType[2]) - 1;
            return -1;
        }

        /// <summary>
        /// Loads a minigame's sprite into the minigame's class fields
        /// </summary>
        /// <param name="minigame">Minigame to load sprite to</param>
        /// <param name="type">Type of LIMinigame</param>
        /// <param name="sprite">Sprite to load</param>
        /// <returns>TRUE iff sprite load should continue</returns>
        private bool LoadMinigameFieldSprite(Minigame minigame, string type, Sprite? sprite)
        {
            switch (type)
            {
                /* task-fans */
                case "task-fans1_symbol_1":
                case "task-fans1_symbol_2":
                case "task-fans1_symbol_3":
                case "task-fans1_symbol_4":
                case "task-fans2_symbol_1":
                case "task-fans2_symbol_2":
                case "task-fans2_symbol_3":
                case "task-fans2_symbol_4":
                    int fansIndex = GetIndex(type);
                    var fansMinigame = minigame.Cast<StartFansMinigame>();
                    var currentFanSprite = fansMinigame.IconSprites[fansIndex];
                    // Find & update any symbols
                    foreach (var codeIcon in fansMinigame.CodeIcons)
                        if (codeIcon.sprite == currentFanSprite)
                            codeIcon.sprite = sprite;
                    fansMinigame.IconSprites[fansIndex] = sprite;
                    return false;

                /* task-keys */
                case "task-keys_key":
                    minigame.Cast<KeyMinigame>().normalImage = sprite;
                    return true;
                case "task-keys_keyinsert":
                    minigame.Cast<KeyMinigame>().insertImage = sprite;
                    return false;
                case "task-keys_keyslotinsert":
                    var keySlotsA = minigame.Cast<KeyMinigame>().Slots;
                    foreach (var keySlot in keySlotsA)
                        keySlot.Inserted = sprite;
                    return false;
                case "task-keys_keyslothighlight":
                    var keySlotsB = minigame.Cast<KeyMinigame>().Slots;
                    foreach (var keySlot in keySlotsB)
                        keySlot.Highlit = sprite;
                    return false;
                case "task-keys_keyslot":
                    var keySlotsC = minigame.Cast<KeyMinigame>().Slots;
                    foreach (var keySlot in keySlotsC)
                        keySlot.Finished = sprite;
                    return true;

                /* task-nodeswitch */
                case "task-nodeswitch_lighton":
                    var weatherGame1 = minigame.Cast<WeatherSwitchGame>();
                    foreach (var control in weatherGame1.Controls)
                        control.lightOn = sprite;
                    return false;
                case "task-nodeswitch_lightoff":
                    var weatherGame2 = minigame.Cast<WeatherSwitchGame>();
                    foreach (var control in weatherGame2.Controls)
                        control.lightOff = sprite;
                    return true;
                case "task-nodeswitch_screenlight":
                    var weatherGame3 = minigame.Cast<WeatherSwitchGame>();
                    foreach (var control in weatherGame3.Controls)
                        control.backgroundLight = sprite;
                    return true;
                case "task-nodeswitch_screendark":
                    var weatherGame4 = minigame.Cast<WeatherSwitchGame>();
                    foreach (var control in weatherGame4.Controls)
                        control.backgroundDark = sprite;
                    return false;

                /* task-pass */
                case "task-pass_back":
                    minigame.Cast<BoardPassGame>().passBack = sprite;
                    return false;
                case "task-pass_scanner":
                    minigame.Cast<BoardPassGame>().ScannerWaiting = sprite;
                    return true;
                case "task-pass_scanninga":
                    minigame.Cast<BoardPassGame>().ScannerAccept = sprite;
                    return false;
                case "task-pass_scanningb":
                    minigame.Cast<BoardPassGame>().ScannerScanning = sprite;
                    return false;

                /* task-toilet */
                case "task-toilet_plungerdown":
                    minigame.Cast<ToiletMinigame>().PlungerDown = sprite;
                    return false;
                case "task-toilet_plungerup":
                    minigame.Cast<ToiletMinigame>().PlungerUp = sprite;
                    return true;

                /* task-vending */
                case "task-vending_item_1":
                case "task-vending_item_2":
                case "task-vending_item_3":
                case "task-vending_item_4":
                case "task-vending_item_5":
                    int vendingIndex1 = GetIndex(type);
                    var vendingMinigame1 = minigame.Cast<VendingMinigame>();
                    var currentVendingSprite1 = vendingMinigame1.Drinks[vendingIndex1];
                    // Find & update any slots
                    foreach (var vendingSlot in vendingMinigame1.Slots)
                        if (vendingSlot.DrinkImage.sprite == currentVendingSprite1)
                            vendingSlot.DrinkImage.sprite = sprite;
                    vendingMinigame1.Drinks[vendingIndex1] = sprite;
                    return false;
                case "task-vending_drawing_1":
                case "task-vending_drawing_2":
                case "task-vending_drawing_3":
                case "task-vending_drawing_4":
                case "task-vending_drawing_5":
                    int vendingIndex2 = GetIndex(type);
                    var vendingMinigame2 = minigame.Cast<VendingMinigame>();
                    var currentVendingSprite2 = vendingMinigame2.DrawnDrinks[vendingIndex2];
                    // Update cooresponding drawing
                    if (vendingMinigame2.TargetImage.sprite == currentVendingSprite2)
                        vendingMinigame2.TargetImage.sprite = sprite;
                    vendingMinigame2.DrawnDrinks[vendingIndex2] = sprite;
                    return false;

                /* task-waterjug */
                case "task-waterjug1_btnup":
                case "task-waterjug2_btnup":
                    var multistageMinigame1 = minigame.Cast<MultistageMinigame>();
                    foreach (var stage in multistageMinigame1.Stages)
                        stage.Cast<WaterStage>().buttonUpSprite = sprite;
                    return true;
                case "task-waterjug1_btndown":
                case "task-waterjug2_btndown":
                    var multistageMinigame2 = minigame.Cast<MultistageMinigame>();
                    foreach (var stage in multistageMinigame2.Stages)
                        stage.Cast<WaterStage>().buttonDownSprite = sprite;
                    return false;

                /* task-weapons */
                case "task-weapons_asteroid_1":
                case "task-weapons_asteroid_2":
                case "task-weapons_asteroid_3":
                case "task-weapons_asteroid_4":
                case "task-weapons_asteroid_5":
                    ObjectPoolBehavior asteroidPool1 = minigame.Cast<WeaponsMinigame>().asteroidPool;
                    int asteroidIndex1 = GetIndex(type);
                    UpdateObjectPool(asteroidPool1, (Asteroid asteroid) =>
                    {
                        asteroid.AsteroidImages[asteroidIndex1] = sprite;
                        asteroid.GetComponent<SpriteRenderer>().sprite = asteroid.AsteroidImages[asteroid.imgIdx];
                    });
                    return false;
                case "task-weapons_broken_1":
                case "task-weapons_broken_2":
                case "task-weapons_broken_3":
                case "task-weapons_broken_4":
                case "task-weapons_broken_5":
                    ObjectPoolBehavior asteroidPool2 = minigame.Cast<WeaponsMinigame>().asteroidPool;
                    int asteroidIndex2 = GetIndex(type);
                    UpdateObjectPool(asteroidPool2, (Asteroid asteroid) =>
                    {
                        asteroid.BrokenImages[asteroidIndex2] = sprite;
                    });
                    return false;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Runs an update function on an entire object pool
        /// </summary>
        /// <typeparam name="T">Type to cast PoolableBehaviour to</typeparam>
        /// <param name="objectPool">ObjectPool to iterate over</param>
        /// <param name="onUpdate">Function to run on update</param>
        [HideFromIl2Cpp]
        private void UpdateObjectPool<T>(ObjectPoolBehavior objectPool, Action<T> onUpdate) where T : Il2CppSystem.Object
        {
            foreach (var child in objectPool.activeChildren)
                onUpdate(child.Cast<T>());
            foreach (var child in objectPool.inactiveChildren)
                onUpdate(child.Cast<T>());
            onUpdate(objectPool.Prefab.Cast<T>());
        }

        public void OnDestroy()
        {
            _minigameDataArr = null;
            _minigameProps = null;
        }
    }
}
