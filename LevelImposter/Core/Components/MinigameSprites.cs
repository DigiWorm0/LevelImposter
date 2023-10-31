using Il2CppInterop.Runtime.Attributes;
using LevelImposter.DB;
using PowerTools;
using QRCoder;
using QRCoder.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        private readonly Dictionary<string, int> BURGER_PAPER_TOPPINGS = new()
        {
            { "task-burger_paperbuntop", 0 },
            { "task-burger_paperbunbottom", 1 },
            { "task-burger_paperlettuce", 2 },
            { "task-burger_papermeat", 3 },
            { "task-burger_paperonion", 4 },
            { "task-burger_papertomato", 5 }
        };
        private readonly Dictionary<string, Vector2> PIVOTS = new()
        {
            { "task-toilet_plungerup", new Vector2(0.53f, 0.96f) },
            { "task-toilet_plungerdown", new Vector2(0.54f, 0.67f) },
            { "task-toilet_needle", new Vector2(0.57f, 0.5f) },
            { "task-toilet_stick", new Vector2(0.5f, 0.11f) },
            { "task-vending_item_1", new Vector2(0.5f, 0) },
            { "task-vending_item_2", new Vector2(0.5f, 0) },
            { "task-vending_item_3", new Vector2(0.5f, 0) },
            { "task-vending_item_4", new Vector2(0.5f, 0) },
            { "task-vending_item_5", new Vector2(0.5f, 0) },
            { "task-vending_item_6", new Vector2(0.5f, 0) },
        };

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
                    Vector2? pivot = PIVOTS.ContainsKey(minigameData.type) ? PIVOTS[minigameData.type] : null;
                    SpriteLoader.Instance?.LoadSpriteAsync(
                        minigameData.spriteData,
                        (spriteData) =>
                        {
                            LoadMinigameSprite(minigame, minigameData.type, spriteData?.Sprite);
                        },
                        minigameData.id.ToString(),
                        pivot
                    );
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
            LILogger.Info($"Loading minigame props for {minigame}");

            // Lights Panel
            bool isLights = _minigameProps?.lightsColorOn != null || _minigameProps?.lightsColorOff != null;
            if (isLights)
            {
                var lightsMinigame = minigame.Cast<SwitchMinigame>();
                lightsMinigame.OnColor = _minigameProps?.lightsColorOn?.ToUnity() ?? lightsMinigame.OnColor;
                lightsMinigame.OffColor = _minigameProps?.lightsColorOn?.ToUnity() ?? lightsMinigame.OffColor;
                LILogger.Info("Applied Light Props");
            }

            // Reactor Panel
            bool isReactor = _minigameProps?.reactorColorBad != null || _minigameProps?.reactorColorGood != null;
            if (isReactor)
            {
                var reactorMinigame = minigame.Cast<ReactorMinigame>();
                reactorMinigame.good = _minigameProps?.reactorColorGood?.ToUnity() ?? reactorMinigame.good;
                reactorMinigame.bad = _minigameProps?.reactorColorBad?.ToUnity() ?? reactorMinigame.bad;
                LILogger.Info("Applied Reactor Props");
            }

            // Fuel Task
            bool isFuel = _minigameProps?.fuelColor != null || _minigameProps?.fuelBgColor != null;
            if (isFuel)
            {
                var fuelStage = minigame.Cast<RefuelStage>();

                var fuelRenderer1 = fuelStage.transform.Find("DestGauge/BackFillMask/BackFillColor").GetComponent<SpriteRenderer>();
                fuelRenderer1.color = _minigameProps?.fuelColor?.ToUnity() ?? fuelRenderer1.color;

                var bgRenderer = fuelStage.transform.Find("blank").GetComponent<SpriteRenderer>();
                bgRenderer.color = _minigameProps?.fuelBgColor?.ToUnity() ?? bgRenderer.color;

                // Only on Stage 2
                var fuelRenderer2 = fuelStage.transform.Find("SrcGauge/BackFillMask/BackFillColor")?.GetComponent<SpriteRenderer>();
                if (fuelRenderer2 != null)
                    fuelRenderer2.color = _minigameProps?.fuelColor?.ToUnity() ?? fuelRenderer2.color;
                LILogger.Info("Applied Fuel Props");
            }

            // Telescope Task
            bool isTelescope = _minigameProps?.isStarfieldEnabled != null;
            if (isTelescope)
            {
                var starfield = minigame.transform.Find("BlackBg/starfield");
                if (starfield != null)
                    starfield.gameObject.SetActive(_minigameProps?.isStarfieldEnabled ?? true);
                LILogger.Info("Applied Telescope Props");
            }

            // Weapons Task
            bool isWeapons = _minigameProps?.weaponsColor != null;
            if (isWeapons)
            {
                var weaponsLine = minigame.transform.Find("TargetLines").GetComponent<LineRenderer>();
                var weaponsColor = _minigameProps?.weaponsColor?.ToUnity();
                weaponsLine.startColor = weaponsColor ?? weaponsLine.startColor;
                weaponsLine.endColor = weaponsColor ?? weaponsLine.endColor;
                weaponsLine.sharedMaterial?.SetColor("_Color", weaponsColor ?? weaponsLine.startColor);
                LILogger.Info("Applied Weapon Props");
            }

            // Boarding Pass
            bool isBoardingPass = !string.IsNullOrEmpty(_minigameProps?.qrCodeText);
            if (isBoardingPass)
            {
                // Wait for 2 frames for Start() to finish
                MapUtils.WaitForFrames(2, () =>
                {
                    // Generate a QR Code
                    var qrCodeGenerator = new QRCodeGenerator();
                    var qrCode = qrCodeGenerator.CreateQrCode(
                        _minigameProps?.qrCodeText ?? "",
                        QRCodeGenerator.ECCLevel.M,
                        false,
                        false,
                        QRCodeGenerator.EciMode.Default,
                        -1
                    );
                    // Create Texture
                    var texture = new UnityQRCode(qrCode).GetGraphic(1);
                    GCHandler.Register(texture);

                    // Create Sprite
                    var sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    GCHandler.Register(sprite);

                    // Apply Sprite
                    minigame.Cast<BoardPassGame>().renderer.sprite = sprite;
                    LILogger.Info("Applied Boarding Pass Props: " + _minigameProps?.qrCodeText);
                });
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
        /// <returns><c>true</c> if sprite load should continue, <c>false</c> otherwise.</returns>
        private bool LoadMinigameFieldSprite(Minigame minigame, string type, Sprite? sprite)
        {
            switch (type)
            {
                /* task-burger */
                case "task-burger_paperbuntop":
                case "task-burger_paperbunbottom":
                case "task-burger_paperlettuce":
                case "task-burger_papermeat":
                case "task-burger_paperonion":
                case "task-burger_papertomato":
                    var toppingIndex = BURGER_PAPER_TOPPINGS[type];
                    var burgerMinigame = minigame.Cast<BurgerMinigame>();
                    var currentToppingSprite = burgerMinigame.PaperToppings[toppingIndex];
                    // Find & update any symbols
                    foreach (var paperSlot in burgerMinigame.PaperSlots)
                        if (paperSlot.sprite == currentToppingSprite)
                            paperSlot.sprite = sprite;
                    burgerMinigame.PaperToppings[toppingIndex] = sprite;
                    return false;

                /* task-drill */
                case "task-drill_btn":
                    var buttonPaths = AssetDB.GetPaths(type);
                    foreach (var path in buttonPaths)
                    {
                        var drillButton = minigame.transform.Find(path);
                        drillButton.GetComponent<SpriteAnim>().enabled = false;
                        drillButton.GetComponent<Animator>().enabled = false;
                    }
                    return true;

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
                case "task-pass_idface":
                    minigame.Cast<BoardPassGame>().Image.sprite = sprite;
                    return false;

                /* task-telescope */
                case "task-telescope_bg":
                    var telescopeBG = minigame.transform.Find("BlackBg");
                    if (telescopeBG != null)
                    {
                        var spriteRenderer = telescopeBG.GetComponent<SpriteRenderer>();
                        spriteRenderer.color = Color.white;
                        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
                        spriteRenderer.sprite = sprite;
                    }
                    return false;

                /* task-temp */
                case "task-temp1_btn":
                case "task-temp2_btn":
                case "task-temp1_btndown":
                case "task-temp2_btndown":
                    var isDown = type.EndsWith("down");
                    var paths = AssetDB.GetPaths(type);
                    foreach (var path in paths)
                    {
                        var button = minigame.transform.Find(path);
                        var rolloverComponent = button.GetComponent<ButtonDownHandler>();

                        if (isDown)
                            rolloverComponent.DownSprite = sprite;
                        else
                            rolloverComponent.UpSprite = sprite;
                    }
                    return !isDown;

                /* task-toilet */
                case "task-toilet_pipe":
                    var pipeSystem = minigame.transform.Find("toilet_pipesystem");
                    pipeSystem.transform.position += new Vector3(0, 0, 0.5f);
                    return true;
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
                case "task-vending_item_6":
                    int vendingIndex1 = GetIndex(type);
                    var vendingMinigame1 = minigame.Cast<VendingMinigame>();
                    var currentVendingSprite1 = vendingMinigame1.Drinks[vendingIndex1];
                    // Find & update any slots
                    foreach (var vendingSlot in vendingMinigame1.Slots)
                        if (vendingSlot.DrinkImage.sprite == currentVendingSprite1 && sprite != null)
                            vendingSlot.DrinkImage.sprite = sprite;
                    vendingMinigame1.Drinks[vendingIndex1] = sprite;
                    return false;
                case "task-vending_drawing_1":
                case "task-vending_drawing_2":
                case "task-vending_drawing_3":
                case "task-vending_drawing_4":
                case "task-vending_drawing_5":
                case "task-vending_drawing_6":
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
                    var waterStage1 = minigame.TryCast<WaterStage>();
                    if (waterStage1 != null)
                        waterStage1.buttonUpSprite = sprite;
                    return false;
                case "task-waterjug1_btndown":
                case "task-waterjug2_btndown":
                    var waterStage2 = minigame.TryCast<WaterStage>();
                    if (waterStage2 != null)
                        waterStage2.buttonDownSprite = sprite;
                    return false;

                /* task-weapons */
                case "task-weapons_asteroid_1":
                case "task-weapons_asteroid_2":
                case "task-weapons_asteroid_3":
                case "task-weapons_asteroid_4":
                case "task-weapons_asteroid_5":
                    var asteroidPool1 = minigame.Cast<WeaponsMinigame>().asteroidPool;
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
                    var asteroidPool2 = minigame.Cast<WeaponsMinigame>().asteroidPool;
                    int asteroidIndex2 = GetIndex(type);
                    UpdateObjectPool(asteroidPool2, (Asteroid asteroid) =>
                    {
                        asteroid.BrokenImages[asteroidIndex2] = sprite;
                    });
                    return false;
                case "task-garbage_leaf_1":
                case "task-garbage_leaf_2":
                case "task-garbage_leaf_3":
                case "task-garbage_leaf_4":
                case "task-garbage_leaf_5":
                case "task-garbage_leaf_6":
                case "task-garbage_leaf_7":
                    var garbageMinigame1 = minigame.Cast<EmptyGarbageMinigame>();
                    var leafIndex = GetIndex(type);
                    var currentLeafPrefab = garbageMinigame1.LeafPrefabs[leafIndex];
                    foreach (var obj in garbageMinigame1.Objects)
                        if (obj.sprite == currentLeafPrefab.sprite)
                            obj.sprite = sprite;
                    return false;

                /* util-computer */
                case "util-computer_folder":
                case "util-computer_file":
                    var computerMinigame = minigame.Cast<TaskAdderGame>();

                    // Replace Prefab
                    Sprite? oldSprite = null;
                    if (type == "util-computer_folder")
                    {
                        oldSprite = computerMinigame.RootFolderPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
                        computerMinigame.RootFolderPrefab = MapUtils.ReplacePrefab(computerMinigame.RootFolderPrefab, minigame.transform);
                        computerMinigame.RootFolderPrefab.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
                    }
                    else
                    {
                        oldSprite = computerMinigame.TaskPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
                        computerMinigame.RoleButton = MapUtils.ReplacePrefab(computerMinigame.RoleButton, minigame.transform);
                        computerMinigame.TaskPrefab = MapUtils.ReplacePrefab(computerMinigame.TaskPrefab, minigame.transform);
                        computerMinigame.RoleButton.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
                        computerMinigame.TaskPrefab.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
                    }

                    // Replace Active Sprites
                    var spriteRenderers = minigame.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (var spriteRenderer in spriteRenderers)
                        if (spriteRenderer.sprite == oldSprite)
                            spriteRenderer.sprite = sprite;
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
