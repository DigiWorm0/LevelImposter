using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Builders
{
    class SabotageOptionsBuilder : IElemBuilder
    {
        public static GameObject? TriggerObject { get; private set; }

        public const string SABOTAGE_SOUND_NAME = "sabotageSound";


        public SabotageOptionsBuilder()
        {
            TriggerObject = null;
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-sabotages")
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Singleton
            if (TriggerObject != null)
            {
                LILogger.Warn("Only 1 util-sabotages object can be placed per map");
                return;
            }
            TriggerObject = obj;
            
            // Sabotage Sound
            LISound? sabotageSound = MapUtils.FindSound(elem.properties.sounds, SABOTAGE_SOUND_NAME);
            if (sabotageSound != null)
                shipStatus.SabotageSound = WAVFile.Load(sabotageSound?.data) ?? shipStatus.SabotageSound;
        }

        public void PostBuild() { }
    }
}