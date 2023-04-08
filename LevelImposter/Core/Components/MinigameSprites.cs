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

        private string? _elemType = null;
        private LIMinigameSprite[]? _minigameDataArr = null;
        private LIMinigameProps? _minigameProps = null;

        /// <summary>
        /// Initializes component with LIElement
        /// </summary>
        /// <param name="elem">Element that GameObject represents</param>
        [HideFromIl2Cpp]
        public void Init(LIElement elem)
        {
            _elemType = elem.type;
            _minigameDataArr = elem.properties.minigames ?? new LIMinigameSprite[0];
            _minigameProps = elem.properties.minigameProps ?? new();
        }

        /// <summary>
        /// Loads the sprites onto a minigame
        /// </summary>
        /// <param name="minigame">Minigame to load sprites to</param>
        public void LoadMinigame(Minigame minigame)
        {
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

        /// <summary>
        /// Loads all props into a minigame
        /// </summary>
        private void LoadMinigameProps(Minigame minigame)
        {
            bool isLights = _minigameProps?.lightsColorOn != null || _minigameProps?.lightsColorOff != null;
            bool isReactor = _minigameProps?.reactorColorBad != null || _minigameProps?.reactorColorGood != null;
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
        }

        /// <summary>
        /// Loads individual sprites onto a minigame
        /// </summary>
        /// <param name="minigame">Minigame to load sprite to</param>
        /// <param name="type">Type of LIMinigame</param>
        /// <param name="sprite">Sprite to load</param>
        private void LoadMinigameSprite(Minigame minigame, string type, Sprite? sprite)
        {
            if (!LoadMinigameFieldSprite(minigame, type, sprite))
                return;

            string[]? spritePaths = AssetDB.GetPaths(type);
            if (spritePaths == null)
                return;

            foreach (string path in spritePaths)
            {
                LILogger.Info($"Loading minigame sprite {type} at '{path}'");
                var spriteObjs = MapUtils.GetTransforms(path, minigame.transform);
                if (spriteObjs.Count <= 0)
                {
                    LILogger.Warn($"Could not find {type} at '{path}'");
                    continue;
                }
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

            /* task-telescope */
            if (type.StartsWith("task-telescope"))
            {
                var telescopeMinigame = minigame.Cast<TelescopeGame>();
                telescopeMinigame.ItemDisplay.sprite = telescopeMinigame.TargetItem.GetComponent<SpriteRenderer>().sprite;
            }
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

                /* task-toilet */
                case "task-toilet_plungerdown":
                    minigame.Cast<ToiletMinigame>().PlungerDown = sprite;
                    return false;
                case "task-toilet_plungerup":
                    minigame.Cast<ToiletMinigame>().PlungerUp = sprite;
                    return true;

                default:
                    return true;
            }
        }

        public void OnDestroy()
        {
            _minigameDataArr = null;
        }
    }
}
