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
    private const float DURATION_OFFSET = 0.9f; // 0.2f per fade, 0.5f for text

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

        // Get Element Data
        var elementData = EjectBuilder.EjectController?.gameObject.GetLIData();
        if (elementData == null)
            throw new Exception("Failed to get LIElementData from EjectController");

        // Get Element Properties
        _x = elementData.Element.x;
        _y = elementData.Element.y;
        _preTextDuration = elementData.Properties.ejectPreTextDuration ?? 2.0f;
        _textDuration = elementData.Properties.ejectTextDuration ?? 2.0f;
        _postTextDuration = elementData.Properties.ejectPostTextDuration ?? 2.0f;
        _cameraXOffset = elementData.Properties.camXOffset ?? 0.0f;
        _cameraYOffset = elementData.Properties.camYOffset ?? 0.0f;
        _cameraZoom = elementData.Properties.camZoom ?? 3.0f;

        // Set Base Duration (Just in case)
        Duration = _preTextDuration + _textDuration + _postTextDuration + DURATION_OFFSET;
    }

    public override IEnumerator Animate()
    {
        return CoAnimate().WrapToIl2Cpp();
    }

    /// <summary>
    ///     Flexible coroutine for running ejection animation
    /// </summary>
    [HideFromIl2Cpp]
    public System.Collections.IEnumerator CoAnimate()
    {
        // Get Base Controller
        var baseController = EjectBuilder.EjectController?.gameObject;
        if (baseController == null)
            throw new Exception("Failed to get base EjectController");

        // Get Player Control
        var playerControl = initData?.networkedPlayer?.Object;

        // Copy Player Outfit to Eject Dummies
        var isEjectingPlayer = initData?.outfit != null;
        foreach (var ejectDummy in EjectDummyBuilder.PoolablePlayers)
        {
            ejectDummy.gameObject.SetActive(isEjectingPlayer);

            if (!isEjectingPlayer)
                continue;

            ejectDummy.UpdateFromPlayerOutfit(
                initData?.outfit,
                PlayerMaterial.MaskType.Exile,
                false,
                false,
                new Action(() =>
                {
                    // Get Skin Data
                    var skinViewData = ejectDummy.GetSkinView();

                    // Fix Skin Sprite (Idle or Eject)
                    ejectDummy.FixSkinSprite(skinViewData.EjectFrame);
                })
            );
            ejectDummy.ToggleName(false);
            ejectDummy.SetCustomHatPosition(exileHatPosition);
            ejectDummy.SetCustomVisorPosition(exileVisorPosition);
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

        // Trigger Eject
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
        yield return HandleText(_preTextDuration, _textDuration);

        // TODO: Add delay here

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
        WrapUp();
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