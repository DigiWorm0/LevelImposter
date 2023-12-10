using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders
{
    class MeetingOptionsBuilder : IElemBuilder
    {
        public static GameObject? TriggerObject { get; private set; }

        public const string REPORT_SOUND_NAME = "meetingReportStinger";
        public const string BUTTON_SOUND_NAME = "meetingButtonStinger";


        public MeetingOptionsBuilder()
        {
            TriggerObject = null;
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-meeting")
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

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
                var mapAssetDB = LIShipStatus.Instance?.CurrentMap?.mapAssetDB;
                var mapAsset = mapAssetDB?.Get(elem.properties.meetingBackgroundID);

                // Load Sprite
                SpriteLoader.Instance?.LoadSpriteAsync(
                    mapAsset?.OpenStream(),
                    (spriteData) =>
                    {
                        LoadMeetingBackground(elem, spriteData);
                    },
                    elem.properties.meetingBackgroundID?.ToString(),
                    null
                );
            }

            // Meeting Overlay
            shipStatus.EmergencyOverlay.gameObject.SetActive(false);
            MeetingCalledAnimation meetingOverlay = UnityEngine.Object.Instantiate(shipStatus.EmergencyOverlay, shipStatus.transform);
            meetingOverlay.gameObject.SetActive(false);
            shipStatus.EmergencyOverlay = meetingOverlay;

            LISound? buttonSound = MapUtils.FindSound(elem.properties.sounds, BUTTON_SOUND_NAME);
            if (buttonSound != null)
            {
                meetingOverlay.Stinger = WAVFile.LoadSound(buttonSound) ?? meetingOverlay.Stinger;
                meetingOverlay.StingerVolume = buttonSound?.volume ?? 1;
            }

            // Report Overlay
            shipStatus.ReportOverlay.gameObject.SetActive(false);
            MeetingCalledAnimation reportOverlay = UnityEngine.Object.Instantiate(shipStatus.ReportOverlay, shipStatus.transform);
            reportOverlay.gameObject.SetActive(false);
            shipStatus.ReportOverlay = reportOverlay;

            LISound? reportSound = MapUtils.FindSound(elem.properties.sounds, REPORT_SOUND_NAME);
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
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Set Background
            shipStatus.MeetingBackground = spriteData.Sprite;
        }

        public void PostBuild() { }
    }
}