using LevelImposter.AssetLoader;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Build.Attributes;
using LevelImposter.Build.Builders.Generic;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Build.Builders.Util;

internal static class MeetingOptionsBuilder
{
    private const string REPORT_SOUND_NAME = "meetingReportStinger";
    private const string BUTTON_SOUND_NAME = "meetingButtonStinger";

    public static GameObject? TriggerObject { get; private set; }

    [MapBuilder(Priority = Priority.LAST)]
    public static void Reset()
    {
        TriggerObject = null;
    }

    [ElementBuilder(ElementTypes = ["util-meeting"])]
    public static void Build(LIMap map, LIElement element, GameObject gameObject)
    {
        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();
        var prefabContainer = LIShipStatus.GetInstance().Prefabs.Container;

        // Singleton
        if (TriggerObject != null)
        {
            LILogger.Warn("Only 1 util-meeting object can be placed per map");
            return;
        }

        TriggerObject = gameObject;

        // Meeting Background
        if (element.properties.meetingBackgroundID != null)
        {
            var loadable = SpriteBuilder.GetLoadableFromID(element.properties.meetingBackgroundID, map);
            if (loadable != null)
                SpriteLoader.Instance.AddToQueue(
                    (SpriteInfo)loadable,
                    spriteData => LoadMeetingBackground(spriteData));
        }

        // Meeting Overlay
        var meetingOverlay = Object.Instantiate(shipStatus.EmergencyOverlay, prefabContainer);
        shipStatus.EmergencyOverlay = meetingOverlay;

        var buttonSound = element.properties.sounds.FindSound(BUTTON_SOUND_NAME);
        if (buttonSound != null)
        {
            meetingOverlay.Stinger = WAVLoader.Load(buttonSound) ?? meetingOverlay.Stinger;
            meetingOverlay.StingerVolume = buttonSound?.volume ?? 1;
        }

        // Report Overlay
        var reportOverlay = Object.Instantiate(shipStatus.ReportOverlay, prefabContainer);
        shipStatus.ReportOverlay = reportOverlay;

        var reportSound = element.properties.sounds.FindSound(REPORT_SOUND_NAME);
        if (reportSound != null)
        {
            reportOverlay.Stinger = WAVLoader.Load(reportSound) ?? reportOverlay.Stinger;
            reportOverlay.StingerVolume = reportSound?.volume ?? 1;
        }
    }

    private static void LoadMeetingBackground(Sprite sprite)
    {
        var shipStatus = LIShipStatus.GetShip();
        shipStatus.MeetingBackground = sprite;
    }
}