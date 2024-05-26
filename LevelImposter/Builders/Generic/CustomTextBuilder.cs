using LevelImposter.Core;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Builders
{
    /// <summary>
    /// Replaces String in the Translation Controller with Custom Text 
    /// </summary>
    public class CustomTextBuilder : IElemBuilder
    {
        private readonly Dictionary<string, StringNames> _customTextDB = new Dictionary<string, StringNames>
        {
            { "MedHello", StringNames.MedHello },
            { "SamplesPress", StringNames.SamplesPress },
            { "SamplesSelect", StringNames.SamplesSelect },
            { "MedETA", StringNames.MedETA },
            { "BadResult", StringNames.BadResult },
            { "SamplesThanks", StringNames.SamplesThanks },
            { "SamplesComplete", StringNames.SamplesComplete },
            { "More", StringNames.More },
            { "SamplesAdding", StringNames.SamplesAdding },
            { "TakeBreak", StringNames.TakeBreak },
            { "GrabCoffee", StringNames.GrabCoffee },
            { "DontNeedWait", StringNames.DontNeedWait },
            { "DoSomethingElse", StringNames.DoSomethingElse },
            { "ReactorNominal", StringNames.ReactorNominal },
            { "ReactorHoldToStop", StringNames.ReactorHoldToStop },
            { "ReactorWaiting", StringNames.ReactorWaiting },
        };

        public void Build(LIElement elem, GameObject obj)
        {
            // Get Custom Text
            var customText = elem.properties.customText;
            if (customText == null || customText.Count <= 0)
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance;
            if (shipStatus == null)
                throw new MissingShipException();

            // Replace Custom Text
            foreach (var (textID, text) in customText)
            {
                // Skip Empty Text
                if (string.IsNullOrEmpty(textID) || string.IsNullOrEmpty(text))
                    continue;

                // Find String Name
                bool hasTextID = _customTextDB.TryGetValue(textID, out StringNames stringName);
                if (!hasTextID)
                {
                    LILogger.Warn($"Unknown custom text '{textID}'");
                    continue;
                }

                // Replace Text
                shipStatus.Renames.Add(stringName, text);
                LILogger.Info($"Custom Text '{stringName}' >>> '{text}'");
            }

        }

        public void PostBuild() { }
    }
}
