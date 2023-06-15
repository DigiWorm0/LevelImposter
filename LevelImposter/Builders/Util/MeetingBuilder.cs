using HarmonyLib;
using LevelImposter.Builders;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using LevelImposter.Core;

namespace LevelImposter.Builders
{
    class MeetingBuilder : IElemBuilder
    {
        public static GameObject? TriggerObject = null;

        private static AudioClip? _defaultReportStinger = null;
        private static AudioClip? _defaultButtonStinger = null;

        public const string REPORT_SOUND_NAME = "meetingReportStinger";
        public const string BUTTON_SOUND_NAME = "meetingButtonStinger";


        public MeetingBuilder()
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
                LILogger.Warn("Only 1 meeting-util object can be placed per map");
                return;
            }
            TriggerObject = obj;

            // Backup Sounds
            if (_defaultReportStinger == null || _defaultButtonStinger == null)
            {
                _defaultReportStinger = shipStatus.ReportOverlay.Stinger;
                _defaultButtonStinger = shipStatus.EmergencyOverlay.Stinger;
            }
            
            // Meeting Background
            if (!string.IsNullOrEmpty(elem.properties.meetingBackground))
            {
                SpriteLoader.Instance?.LoadSpriteAsync(
                    elem.properties.meetingBackground,
                    (spriteData) => {
                        LoadMeetingBackground(elem, spriteData);
                    },
                    elem.id.ToString()
                );
            }
            
            // Meeting Sound
            LISound? buttonSound = MapUtils.FindSound(elem.properties.sounds, BUTTON_SOUND_NAME);
            if (buttonSound != null)
                LoadMeetingSound(
                    elem,
                    WAVFile.Load(buttonSound?.data),
                    buttonSound?.volume ?? 1,
                    false
                );

            // Report Sound
            LISound? reportSound = MapUtils.FindSound(elem.properties.sounds, REPORT_SOUND_NAME);
            if (reportSound != null)
                LoadMeetingSound(
                    elem,
                    WAVFile.Load(reportSound?.data),
                    reportSound?.volume ?? 1,
                    false
                );
        }

        private void LoadMeetingBackground(LIElement elem, SpriteLoader.SpriteData? spriteData) {

            // Handle Error
            if (spriteData == null)
            {
                LILogger.Warn($"Error loading sprite for {elem}");
                return;
            }

            // Sprite is in cache, we can reduce memory usage
            elem.properties.meetingBackground = "";

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Set Background
            shipStatus.MeetingBackground = spriteData.Sprite;
        }

        private void LoadMeetingSound(LIElement elem, AudioClip? clip, float volume, bool isReport)
        {
            // Handle Error
            if (clip == null)
            {
                LILogger.Warn($"Error loading sound for {elem}");
                return;
            }

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Set Stinger
            MeetingCalledAnimation meetingOverlay = isReport ? shipStatus.ReportOverlay : shipStatus.EmergencyOverlay;
            meetingOverlay.Stinger = clip;
            meetingOverlay.StingerVolume = volume;
        }

        public void PostBuild()
        {
            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Report
            var reportOverlay = shipStatus.ReportOverlay;
            if (reportOverlay.Stinger == null)
                reportOverlay.Stinger = _defaultButtonStinger;

            // Emergency
            var emergencyOverlay = shipStatus.EmergencyOverlay;
            if (emergencyOverlay.Stinger == null)
                emergencyOverlay.Stinger = _defaultReportStinger;
        }
    }
}