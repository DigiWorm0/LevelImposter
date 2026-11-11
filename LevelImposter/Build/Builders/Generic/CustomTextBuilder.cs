using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;

namespace LevelImposter.Build.Builders.Generic;

/// <summary>
///     Replaces String in the Translation Controller with Custom Text
/// </summary>
internal static class CustomTextBuilder
{
    private static readonly Dictionary<string, StringNames> TextIDToStringName = new()
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
        { "WifiPleaseReturnIn", StringNames.WifiPleaseReturnIn },
        { "WifiRebootRequired", StringNames.WifiRebootRequired },
        { "WifiPleaseWait", StringNames.WifiPleaseWait },
        { "WifiRebootComplete", StringNames.WifiRebootComplete },
        { "WifiPleasePowerOn", StringNames.WifiPleasePowerOn },
        { "WeatherComplete", StringNames.WeatherComplete },
        { "WeatherEta", StringNames.WeatherEta },
        { "AstDestroyed", StringNames.AstDestroyed },
        { "WaterPlantsGetCan", StringNames.WaterPlantsGetCan },
        { "Vending", StringNames.Vending },
        { "Download", StringNames.Download },
        { "MyTablet", StringNames.MyTablet },
        { "Headquarters", StringNames.Headquarters },
        { "DownloadTestEstTimeDHMS", StringNames.DownloadTestEstTimeDHMS },
        { "DownloadTestEstTimeHMS", StringNames.DownloadTestEstTimeHMS },
        { "DownloadTestEstTimeMS", StringNames.DownloadTestEstTimeMS },
        { "DownloadTestEstTimeS", StringNames.DownloadTestEstTimeS },
        { "DownloadComplete", StringNames.DownloadComplete },
        { "MedID", StringNames.MedID },
        { "MedHT", StringNames.MedHT },
        { "MedWT", StringNames.MedWT },
        { "MedC", StringNames.MedC },
        { "MedBT", StringNames.MedBT },
        { "MedscanRequested", StringNames.MedscanRequested },
        { "MedscanWaitingFor", StringNames.MedscanWaitingFor },
        { "MedscanCompleted", StringNames.MedscanCompleted },
        { "MedscanCompleteIn", StringNames.MedscanCompleteIn },
        { "SecondsAbbv", StringNames.SecondsAbbv },
        { "EmergencyNotReady", StringNames.EmergencyNotReady },
        { "EmergencyCount", StringNames.EmergencyCount },
        { "EmergencyDuringCrisis", StringNames.EmergencyDuringCrisis },
        { "EmergencyRequested", StringNames.EmergencyRequested },
        { "Fine", StringNames.Fine },
        { "SwipeCardPleaseInsert", StringNames.SwipeCardPleaseInsert },
        { "SwipeCardPleaseSwipe", StringNames.SwipeCardPleaseSwipe },
        { "SwipeCardAccepted", StringNames.SwipeCardAccepted },
        { "SwipeCardTooFast", StringNames.SwipeCardTooFast },
        { "SwipeCardBadRead", StringNames.SwipeCardBadRead },
        { "SwipeCardTooSlow", StringNames.SwipeCardTooSlow },
        { "BeginDiagnostics", StringNames.BeginDiagnostics },
        { "PickAnomaly", StringNames.PickAnomaly }
    };

    [ElementBuilder]
    public static void AddCustomText(LIElement element, LIBaseShip baseShip)
    {
        // Get Custom Text
        var customText = element.properties.customText;
        if (customText == null)
            return;

        // Replace Custom Text
        foreach (var (textID, text) in customText)
        {
            // Skip Empty Text
            if (string.IsNullOrEmpty(textID) ||
                string.IsNullOrEmpty(text))
                continue;

            // Find String Name
            var hasTextID = TextIDToStringName.TryGetValue(textID, out var stringName);
            if (!hasTextID)
            {
                LILogger.Warn($"Unknown custom text '{textID}'");
                continue;
            }

            // Replace Text
            baseShip.Renames.Add(stringName, text);
            LILogger.Debug($"Custom Text '{stringName}' >>> '{text}'");
        }
    }
}