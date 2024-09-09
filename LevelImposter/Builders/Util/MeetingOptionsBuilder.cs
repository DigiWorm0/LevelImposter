using LevelImposter.Core;
using UnityEngine;

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
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

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

            // Load Sprite
            SpriteLoader.Instance?.LoadSpriteAsync(
                mapAsset?.OpenStream(),
                spriteData => { LoadMeetingBackground(elem, spriteData); },
                elem.properties.meetingBackgroundID?.ToString(),
                null
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
            meetingOverlay.Stinger = WAVFile.LoadSound(buttonSound) ?? meetingOverlay.Stinger;
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
            reportOverlay.Stinger = WAVFile.LoadSound(reportSound) ?? reportOverlay.Stinger;
            reportOverlay.StingerVolume = reportSound?.volume ?? 1;
        }
    }

    private void LoadMeetingBackground(LIElement elem, SpriteLoader.SpriteData? spriteData)
    {
        // Handle Error
        if (spriteData == null)
        {
            LILogger.Warn($"Error loading sprite for {elem}");
            return;
        }

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Set Background
        shipStatus.MeetingBackground = spriteData.Sprite;
    }
}