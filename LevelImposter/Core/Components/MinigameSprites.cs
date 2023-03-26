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

        private LIMinigameSprite[]? minigameDataArr = null;
        private LIMinigameProps? minigameProps = null;

        /// <summary>
        /// Initializes component with LIElement
        /// </summary>
        /// <param name="elem">Element that GameObject represents</param>
        [HideFromIl2Cpp]
        public void Init(LIElement elem)
        {
            minigameDataArr = elem.properties.minigames ?? new LIMinigameSprite[0];
            minigameProps = elem.properties.minigameProps ?? new();
        }

        /// <summary>
        /// Loads the sprites onto a minigame
        /// </summary>
        /// <param name="minigame">Minigame to load sprites to</param>
        public void LoadMinigame(Minigame minigame)
        {
            LoadMinigameProps(minigame);
            if (minigameDataArr == null)
                return;
            foreach (LIMinigameSprite minigameData in minigameDataArr)
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
            bool isLights = minigameProps?.lightsColorOn != null || minigameProps?.lightsColorOff != null;
            bool isReactor = minigameProps?.reactorColorBad != null || minigameProps?.reactorColorGood != null;
            LILogger.Info($"Loading minigame props for {minigame}");

            // Lights Panel
            if (isLights)
            {
                var lightsMinigame = minigame.Cast<SwitchMinigame>();
                lightsMinigame.OnColor = minigameProps?.lightsColorOn?.ToUnity() ?? lightsMinigame.OnColor;
                lightsMinigame.OffColor = minigameProps?.lightsColorOn?.ToUnity() ?? lightsMinigame.OffColor;
                LILogger.Info("Applied Light Props");
            }

            // Reactor Panel
            if (isReactor)
            {
                var reactorMinigame = minigame.Cast<ReactorMinigame>();
                reactorMinigame.good = minigameProps?.reactorColorGood?.ToUnity() ?? reactorMinigame.good;
                reactorMinigame.bad = minigameProps?.reactorColorBad?.ToUnity() ?? reactorMinigame.bad;
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
            string[]? spritePaths = AssetDB.GetPaths(type);
            if (spritePaths == null)
                return;

            foreach (string path in spritePaths)
            {
                LILogger.Info($"Loading minigame sprite {type} at '{path}'");
                var spriteObj = minigame.transform.Find(path);
                var spriteRenderer = spriteObj?.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    LILogger.Warn($"Could not find {type} at '{path}'");
                    continue;
                }
                spriteRenderer.sprite = sprite;
            }
        }

        public void OnDestroy()
        {
            minigameDataArr = null;
        }
    }
}
