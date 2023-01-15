using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.DB
{
    class SoundData : AssetData
    {
        public string GameObjName { get; set; }
        public string SoundGroupName { get; set; }
        public string AudioClipName { get; set; }

        public AudioClip Clip { get; set; }

        public override void ImportMap(GameObject map, ShipStatus shipStatus)
        {
            List<Transform> parents = MapSearchUtil.SearchMultipleChildren(map, GameObjName);
            foreach (Transform parent in parents)
            {
                SkeldShipRoom skeldRoom = parent.gameObject.GetComponent<SkeldShipRoom>();
                FootstepWatcher stepWatcher = parent.gameObject.GetComponent<FootstepWatcher>();
                if (skeldRoom != null)
                {
                    if (SearchSoundGroup(skeldRoom.FootStepSounds))
                        return;
                }
                else if (stepWatcher != null)
                {
                    if (SearchSoundGroup(stepWatcher.Sounds))
                        return;
                }
            }
            LILogger.Warn($"Could not find {GameObjName} in {shipStatus}");
        }

        private bool SearchSoundGroup(SoundGroup soundGroup)
        {
            if (soundGroup == null)
                return false;
            if (soundGroup.name != SoundGroupName)
                return false;
            foreach (AudioClip clip in soundGroup.Clips)
            {
                if (clip.name == AudioClipName)
                {
                    Clip = clip;
                    return true;
                }
            }
            return false;
        }
    }
}
