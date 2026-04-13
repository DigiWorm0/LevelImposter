using System;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Collections;
using LevelImposter.Builders;
using LevelImposter.Trigger;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Exile Controller for LevelImposter maps.
///     Automatically runs based on element properties and
///     triggers onEject and onFinish triggers.
/// </summary>
public class LIExileController(IntPtr intPtr) : ExileController(intPtr)
{
    private const string ON_EJECT_TRIGGER_ID = "onEject";
    private const string ON_SKIP_TRIGGER_ID = "onSkip";
    private const string ON_FINISH_TRIGGER_ID = "onFinish";
    private const float DURATION_OFFSET = 1.4f; // 0.2f per fade, 0.5f for text, 0.5f for delay

    private Camera? _camera;
    private float _cameraXOffset;
    private float _cameraYOffset;
    private float _cameraZoom;

    private float _postTextDuration;
    private float _preTextDuration;
    private float _textDuration;
    private float _x;
    private float _y;

    public new void Awake()
    {
        base.Awake();

        // Fix Serialized Fields
        exileHatPosition = new Vector3(-0.216f, 0.528f);
        exileVisorPosition = new Vector3(-0.148f, 0.647f, -0.002f);

        // Get Element Data
        var element = MapObjectDB.Get(EjectBuilder.EjectController?.gameObject);
        if (element == null)
            throw new Exception("Failed to get LIElementData from EjectController");

        // Get Element Properties
        _x = element.x;
        _y = element.y;
        _preTextDuration = element.properties.ejectPreTextDuration ?? 2.0f;
        _textDuration = element.properties.ejectTextDuration ?? 2.0f;
        _postTextDuration = element.properties.ejectPostTextDuration ?? 2.0f;
        _cameraXOffset = element.properties.camXOffset ?? 0.0f;
        _cameraYOffset = element.properties.camYOffset ?? 0.0f;
        _cameraZoom = element.properties.camZoom ?? 3.0f;

        // Set Base Duration (Just in case)
        Duration = _preTextDuration + _textDuration + _postTextDuration + DURATION_OFFSET;
    }

    public override IEnumerator Animate()
    {
        return CoAnimate().WrapToIl2Cpp();
    }

    private void UpdateEjectDummies()
    {
        // Copy Player Outfit to Eject Dummies
        var isEjectingPlayer = initData?.outfit != null;
        foreach (var ejectDummy in EjectDummyBuilder.PlayerDummies)
        {
            // Get Player
            var poolablePlayer = ejectDummy.PoolablePlayer;
            var type = ejectDummy.Type;

            // Set Active
            poolablePlayer.gameObject.SetActive(isEjectingPlayer);
            if (!isEjectingPlayer)
                continue;

            // Update Player Outfit
            poolablePlayer.UpdateFromEitherPlayerDataOrCache(
                initData?.networkedPlayer,
                PlayerOutfitType.Default,
                PlayerMaterial.MaskType.Exile,
                false,
                new Action(() =>
                {
                    // Get Skin Data
                    var skinViewData = poolablePlayer.GetSkinView();

                    // Fix Skin Sprite (Idle or Eject)
                    var sprite = type switch
                    {
                        EjectDummyBuilder.PlayerDummyType.Floating => skinViewData.EjectFrame,
                        EjectDummyBuilder.PlayerDummyType.Standing => skinViewData.IdleFrame,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    poolablePlayer.FixSkinSprite(sprite);
                })
            );

            // Hide Name
            poolablePlayer.ToggleName(false);

            // Fix Hat and Visor Position
            if (type == EjectDummyBuilder.PlayerDummyType.Standing)
                continue;
            poolablePlayer.SetCustomHatPosition(exileHatPosition);
            poolablePlayer.SetCustomVisorPosition(exileVisorPosition);
        }

        // Copy Player Outfit to Eject Hands
        var colorID = initData?.outfit?.ColorId;
        foreach (var hand in EjectHandBuilder.AllHands)
        {
            hand.gameObject.SetActive(isEjectingPlayer);
            if (!isEjectingPlayer)
                continue;

            hand.sharedMaterial = CosmeticsLayer.GetBodyMaterial(PlayerMaterial.MaskType.Exile);
            PlayerMaterial.SetColors((int)colorID!, hand);
        }
    }

    /// <summary>
    ///     Flexible coroutine for running ejection animation
    /// </summary>
    [HideFromIl2Cpp]
    private System.Collections.IEnumerator CoAnimate()
    {
        // Get Base Controller
        var baseController = EjectBuilder.EjectController?.gameObject;
        if (baseController == null)
            throw new Exception("Failed to get base EjectController");

        // Get Player Control
        var playerControl = initData?.networkedPlayer?.Object;

        // Update Eject Dummies/Hands
        UpdateEjectDummies();

        // Trigger Eject
        var isEjectingPlayer = initData?.outfit != null;
        var triggerID = isEjectingPlayer ? ON_EJECT_TRIGGER_ID : ON_SKIP_TRIGGER_ID;
        TriggerSignal ejectSignal = new(baseController, triggerID, playerControl);
        TriggerSystem.GetInstance().FireTrigger(ejectSignal);


        // Switch to Eject Camera
        var ejectCamera = _camera ?? CreateCamera();
        ejectCamera.enabled = true;

        // Fade In
        if (DestroyableSingleton<HudManager>.InstanceExists)
            yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);

        // Wait then show text
        yield return CoHandleText();

        // Wait before finishing
        yield return new WaitForSeconds(0.5f);
        // TODO: Add customizable delay here

        // Show "x Impostors Remain" text
        if (initData?.confirmImpostor ?? false)
            ImpostorText.gameObject.SetActive(true);
        yield return Effects.Bloop(0f, ImpostorText.transform);

        // Wait
        yield return Effects.Wait(_postTextDuration);

        // Fade Out
        if (DestroyableSingleton<HudManager>.InstanceExists)
            yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);

        // Switch to Main Camera
        ejectCamera.enabled = false;

        // Trigger Finish
        TriggerSignal finishSignal = new(baseController, ON_FINISH_TRIGGER_ID, playerControl);
        TriggerSystem.GetInstance().FireTrigger(finishSignal);

        // Wrap Up Base
        WrapUpAlt();
    }

    /// <summary>
    ///  Coroutine based on ExileController.HandleText but with customizable timings
    /// </summary>
    [HideFromIl2Cpp]
    private System.Collections.IEnumerator CoHandleText()
    {
        // DO NOT REFERENCE this.completeString
        var exileString = ExileTextPatch.LastExileText;
        
        yield return Effects.Wait(_preTextDuration);
        for (var t = 0f; t <= _textDuration; t += Time.deltaTime)
        {
            var num = (int)(t / _textDuration * exileString.Length);
            if (num > Text.text.Length)
            {
                Text.text = exileString.Substring(0, num);
                Text.gameObject.SetActive(true);
                if (exileString[num - 1] != ' ')
                    SoundManager.Instance.PlaySoundImmediate(TextSound, false, 0.8f);
            }
            yield return null;
        }
        Text.text = exileString;
    }

    /// <summary>
    /// Based on ExileController.WrapUp but modified to avoid bugs with IL2CPP
    /// </summary>
    private void WrapUpAlt()
    {
        // Mark Player as Exiled/Dead
        if (initData.networkedPlayer != null)
        {
            var playerObject = initData.networkedPlayer.Object;
            if (playerObject)
                playerObject.Exiled();
            
            initData.networkedPlayer.IsDead = true;
        }
        
        // Re-enable Gameplay
        if (DestroyableSingleton<TutorialManager>.InstanceExists || 
            (GameManager.Instance != null && !GameManager.Instance.LogicFlow.IsGameOverDueToDeath()))
            ReEnableGameplay();
        
        // Destroy Eject Controller
        GameObject.Destroy(gameObject);
    }

    /// <summary>
    ///     Creates a camera for the ejection sequence
    /// </summary>
    /// <returns>A new camera object</returns>
    private Camera CreateCamera()
    {
        // Get Ship
        var shipStatus = LIShipStatus.GetShip();

        // Add Camera Object
        var cameraObject = new GameObject("EjectCamera");
        cameraObject.transform.SetParent(shipStatus.transform);
        cameraObject.transform.localPosition = new Vector3(
            _x + _cameraXOffset,
            _y + _cameraYOffset,
            Camera.main?.transform.position.z ?? -10.0f
        );
        cameraObject.transform.localRotation = Quaternion.identity;
        cameraObject.transform.localScale = Vector3.one;

        // Add Camera Component
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = _cameraZoom;
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

        // Copy Camera Properties
        camera.cullingMask = Camera.main?.cullingMask ?? 0;
        camera.depth = Camera.main?.depth ?? 0;
        camera.nearClipPlane = Camera.main?.nearClipPlane ?? 0;
        camera.farClipPlane = Camera.main?.farClipPlane ?? 0;
        camera.fieldOfView = Camera.main?.fieldOfView ?? 0;

        // Return Camera
        _camera = camera;
        return camera;
    }
}