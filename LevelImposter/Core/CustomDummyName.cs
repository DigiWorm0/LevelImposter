using HarmonyLib;

namespace LevelImposter.Core
{
    public static class CustomDummyName
    {
        [HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Start))]
        public static void Postfix(DummyBehaviour __instance)
        {
           __instance.myPlayer.SetName(DestroyableSingleton<AccountManager>.Instance.GetRandomName());

           if (LIConstants.SHOULD_HAVE_SPECIFIC_NAMES)
           {
             var random  =  UnityEngine.Random.RandomRangeInt(1, 2);
             if (random == 1)
             {
                __instance.myPlayer.SetName("DigiWorm0");
             }
             else if (random == 2)
             {
               __instance.myPlayer.SetName(PlayerControl.LocalPlayer.cosmetics.nameText.name);
             }
           }
        }
    }
}

 