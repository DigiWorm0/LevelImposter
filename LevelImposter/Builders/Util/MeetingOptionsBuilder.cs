using System;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders;

internal class MeetingOptionsBuilder : IElemBuilder
{
    public const string REPORT_SOUND_NAME = "meetingReportStinger";
    public const string BUTTON_SOUND_NAME = "meetingButtonStinger";


    public MeetingOptionsBuilder()
    {
        TriggerObject = null;
    }

    public static GameObject? TriggerObject { get; private set; }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-meeting")
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // Singleton
        if (TriggerObject != null)
        {
            LILogger.Warn("Only 1 util-meeting object can be placed per map");
            return;
        }

        TriggerObject = obj;

        // Meeting Background
        if (elem.properties.meetingBackgroundID != null)
        {
            var mapAssetDB = LIShipStatus.GetInstanceOrNull()?.CurrentMap?.mapAssetDB;
            var mapAsset = mapAssetDB?.Get(elem.properties.meetingBackgroundID);
            if (mapAsset == null)
                throw new Exception("Meeting Background ID not found in MapAssetDB");

            // Load Sprite
            SpriteLoader.LoadAsync(
                elem.properties.meetingBackgroundID?.ToString() ?? "",
                mapAsset,
                spriteData => { LoadMeetingBackground(elem, spriteData); }
            );
        }

        // Meeting Overlay
        shipStatus.EmergencyOverlay.gameObject.SetActive(false);
        var meetingOverlay = Object.Instantiate(shipStatus.EmergencyOverlay, shipStatus.transform);
        meetingOverlay.gameObject.SetActive(false);
        shipStatus.EmergencyOverlay = meetingOverlay;

        var buttonSound = MapUtils.FindSound(elem.properties.sounds, BUTTON_SOUND_NAME);
        if (buttonSound != null)
        {
            meetingOverlay.Stinger = WAVLoader.Load(buttonSound) ?? meetingOverlay.Stinger;
            meetingOverlay.StingerVolume = buttonSound?.volume ?? 1;
        }

        // Report Overlay
        shipStatus.ReportOverlay.gameObject.SetActive(false);
        var reportOverlay = Object.Instantiate(shipStatus.ReportOverlay, shipStatus.transform);
        reportOverlay.gameObject.SetActive(false);
        shipStatus.ReportOverlay = reportOverlay;

        var reportSound = MapUtils.FindSound(elem.properties.sounds, REPORT_SOUND_NAME);
        if (reportSound != null)
        {
            reportOverlay.Stinger = WAVLoader.Load(reportSound) ?? reportOverlay.Stinger;
            reportOverlay.StingerVolume = reportSound?.volume ?? 1;
        }
    }

    private void LoadMeetingBackground(LIElement elem, Sprite sprite)
    {
        var shipStatus = LIShipStatus.GetShip();
        shipStatus.MeetingBackground = sprite;
    }
}