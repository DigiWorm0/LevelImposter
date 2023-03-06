using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Core
{
    public class TriggerConsole : MonoBehaviour
    {
        public TriggerConsole(IntPtr intPtr) : base(intPtr)
        {
        }

        public const string TRIGGER_ID = "onUse";

        private float _usableDistance = 1.0f;
        private bool _onlyFromBelow = false;
        private SpriteRenderer? _spriteRenderer;

        public float UsableDistance => _usableDistance;
        public float PercentCool => 0;
        public ImageNames UseIcon => ImageNames.UseButton;

        /// <summary>
        /// Sets the console's metadata
        /// </summary>
        /// <param name="elem">LIElement the console is attatched to</param>
        [HideFromIl2Cpp]
        public void Init(LIElement elem)
        {
            _usableDistance = elem.properties.range ?? 1.0f;
            _onlyFromBelow = elem.properties.onlyFromBelow ?? false;
        }

        /// <summary>
        /// Updates the sprite outline for the consoles
        /// </summary>
        /// <param name="isHighlighted">TRUE iff the console is within vision</param>
        /// <param name="isMainTarget">TRUE iff the console is the main target selected</param>
        public void SetOutline(bool isHighlighted, bool isMainTarget)
        {
            if (_spriteRenderer == null)
                return;
            
            _spriteRenderer.material.SetFloat("_Outline", isHighlighted ? 1 : 0);
            _spriteRenderer.material.SetColor("_OutlineColor", Color.yellow);
            _spriteRenderer.material.SetColor("_AddColor", isMainTarget ? Color.yellow : Color.clear);
        }

        /// <summary>
        /// Checks whether or not the console is usable by a player
        /// </summary>
        /// <param name="playerInfo">Player to check</param>
        /// <param name="canUse">TRUE iff the player can access this console currently</param>
        /// <param name="couldUse">TRUE iff the player could access this console in the future</param>
        /// <returns></returns>
        public float CanUse(GameData.PlayerInfo playerInfo, out bool canUse, out bool couldUse)
        {
            PlayerControl playerControl = playerInfo.Object;
            Vector2 truePosition = playerControl.GetTruePosition();
            Vector3 position = transform.position;

            couldUse = !playerInfo.IsDead &&
                    playerControl.CanMove &&
                    (!_onlyFromBelow || truePosition.y < position.y);
            canUse = couldUse;

            if (couldUse)
            {
                float playerDistance = Vector2.Distance(truePosition, transform.position);
                canUse = couldUse && (playerDistance <= _usableDistance);
                return playerDistance;
            }
            return float.MaxValue;
        }

        /// <summary>
        /// Activates the associated console trigger
        /// </summary>
        public void Use()
        {
            CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool couldUse);
            if (!canUse)
                return;
            LITriggerable.Trigger(gameObject, TRIGGER_ID, PlayerControl.LocalPlayer);
        }

        public void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
