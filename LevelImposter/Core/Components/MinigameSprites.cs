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

        private LIMinigame[]? minigameDataArr = null;

        [HideFromIl2Cpp]
        public void Init(LIElement elem)
        {
            minigameDataArr = elem.properties.minigames ?? new LIMinigame[0];
        }

        public void LoadMinigame(Minigame minigame)
        {
            if (minigameDataArr == null)
                return;
            foreach (LIMinigame minigameData in minigameDataArr)
            {
                SpriteLoader.Instance?.LoadSpriteAsync(minigameData.spriteData, (spriteData) => {
                    LoadMinigameSprite(minigame, minigameData.type, spriteData?.Sprite);
                }, minigameData.id.ToString());
            }
        }

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
