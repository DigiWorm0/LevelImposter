using HarmonyLib;
using LevelImposter.Builders;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Normally, binoculars (util-cams4) are handled by
///     FungleShipStatus. This bypasses that
///     dependency by supplying it's own data.
/// </summary>
[HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
public static class BinocularsPatch
{
    public static void Prefix(FungleSurveillanceMinigame __instance)
    {
        if (LIShipStatus.IsInstance())
            return;

        // Create a temporary room to prevent System.InvalidOperationException
        var tempRoom = __instance.gameObject.AddComponent<PlainShipRoom>();
        tempRoom.RoomId = SystemTypes.MeetingRoom;
        ShipStatus.Instance.AllRooms = MapUtils.AddToArr(ShipStatus.Instance.AllRooms, tempRoom);
    }

    public static void Postfix(FungleSurveillanceMinigame __instance)
    {
        if (LIShipStatus.IsInstance())
            return;

        // Remove the temporary room
        var arr = ShipStatus.Instance.AllRooms;
        ShipStatus.Instance.AllRooms = MapUtils.RemoveFromArr(arr, arr.Count - 1);

        // Fix Security Camera Position
        var lastPos = BinocularsBuilder.LastBinocularsPos;
        if (lastPos != Vector2.zero)
            __instance.securityCamera.transform.localPosition = new Vector3(lastPos.x, lastPos.y, 50f);
        else
            __instance.securityCamera.transform.position =
                BinocularsBuilder.CameraOffset + __instance.transform.position;

        // Fix Camera Properties
        var camera = __instance.securityCamera.cam;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Camera.main.backgroundColor;
        camera.orthographicSize = BinocularsBuilder.OrthographicSize;
    }
}

[HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Close))]
public static class BinocularsClosePatch
{
    public static void Postfix(FungleSurveillanceMinigame __instance)
    {
        if (LIShipStatus.IsInstance())
            return;

        // Set Last Camera Position
        BinocularsBuilder.LastBinocularsPos = new Vector2(
            __instance.securityCamera.transform.localPosition.x,
            __instance.securityCamera.transform.localPosition.y
        );
    }
}