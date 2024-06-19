using Il2CppInterop.Runtime.Attributes;
using System;
using UnityEngine;

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
        private bool _ghostsEnabled = false;
        private Color _highlightColor = Color.yellow;
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
            _ghostsEnabled = elem.properties.isGhostEnabled ?? false;
            _highlightColor = elem.properties.highlightColor?.ToUnity() ?? Color.yellow;
        }

        /// <summary>
        /// Updates the sprite outline for the consoles
        /// </summary>
        /// <param name="isVisible">TRUE iff the console is within vision</param>
        /// <param name="isTargeted">TRUE iff the console is the main target selected</param>
        public void SetOutline(bool isVisible, bool isTargeted)
        {
            if (_spriteRenderer == null)
                return;

            _spriteRenderer.material.SetFloat("_Outline", isVisible ? 1 : 0);
            _spriteRenderer.material.SetColor("_OutlineColor", _highlightColor);
            _spriteRenderer.material.SetColor("_AddColor", isTargeted ? _highlightColor : Color.clear);
        }

        /// <summary>
        /// Checks whether or not the console is usable by a player
        /// </summary>
        /// <param name="playerInfo">Player to check</param>
        /// <param name="canUse">TRUE iff the player can access this console currently</param>
        /// <param name="couldUse">TRUE iff the player could access this console in the future</param>
        /// <returns>Distance from console</returns>
        public float CanUse(NetworkedPlayerInfo playerInfo, out bool canUse, out bool couldUse)
        {
            PlayerControl playerControl = playerInfo.Object;
            Vector2 truePosition = playerControl.GetTruePosition();
            Vector3 position = transform.position;

            couldUse = (!playerInfo.IsDead || _ghostsEnabled) &&
                    playerControl.CanMove &&
                    (!_onlyFromBelow || truePosition.y < position.y) &&
                    enabled;
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
        public void OnDestroy()
        {
            _spriteRenderer = null;
        }
    }
}
