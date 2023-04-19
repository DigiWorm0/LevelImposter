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

            // Singleton
            if (TriggerObject != null)
            {
                LILogger.Warn("Only 1 meeting-util object can be placed per map");
                return;
            }
            TriggerObject = obj;
            
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
                WAVLoader.Instance?.LoadWAV(buttonSound?.data, (AudioClip? clip) => {
                    LoadMeetingSound(elem, clip, false);
                });

            // Report Sound
            LISound? reportSound = MapUtils.FindSound(elem.properties.sounds, REPORT_SOUND_NAME);
            if (reportSound != null)
                WAVLoader.Instance?.LoadWAV(reportSound?.data, (AudioClip? clip) => {
                    LoadMeetingSound(elem, clip, true);
                });
        }

        private void LoadMeetingBackground(LIElement elem, SpriteLoader.SpriteData? nullableSpriteData) {

            // Handle Error
            if (nullableSpriteData == null)
            {
                LILogger.Warn($"Error loading sprite for {elem}");
                return;
            }

            // Sprite is in cache, we can reduce memory usage
            elem.properties.meetingBackground = "";

            // Load Components
            var spriteData = (SpriteLoader.SpriteData)nullableSpriteData;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Set Background
            shipStatus.MeetingBackground = spriteData.Sprite;
        }

        private void LoadMeetingSound(LIElement elem, AudioClip? clip, bool isReport)
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
        }

        public void PostBuild() { }
    }
}