using HarmonyLib;
using ObjList = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>;

namespace LevelImposter.Core
{
    /// <summary>
    /// Renames task names and other strings stored as <c>SystemTypes</c>.
    /// </summary>
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new System.Type[] { typeof(SystemTypes) })]
    public static class SystemRenamePatch
    {
        public static bool Prefix([HarmonyArgument(0)] SystemTypes systemType, ref string __result)
        {
            if (LIShipStatus.Instance == null || !LIShipStatus.Instance.Renames.Contains(systemType))
                return true;

            __result = LIShipStatus.Instance.Renames.Get(systemType);
            return false;
        }
    }
    /// <summary>
    /// Renames task names and other strings stored as <c>TaskTypes</c>.
    /// </summary>
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new System.Type[] { typeof(TaskTypes) })]
    public static class TaskRenamePatch
    {
        public static bool Prefix([HarmonyArgument(0)] TaskTypes taskType, ref string __result)
        {
            if (LIShipStatus.Instance == null || !LIShipStatus.Instance.Renames.Contains(taskType))
                return true;

            __result = LIShipStatus.Instance.Renames.Get(taskType);
            return false;
        }
    }
    /// <summary>
    /// Renames task names and other strings stored as <c>StringNames</c>.
    /// </summary>
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new System.Type[] { typeof(StringNames), typeof(ObjList) })]
    public static class StringRenamePatch
    {
        public static bool Prefix([HarmonyArgument(0)] StringNames stringNames,
                                  [HarmonyArgument(1)] ObjList _, // TODO: Format parameters into string
                                  ref string __result)
        {
            if (LIShipStatus.Instance == null || !LIShipStatus.Instance.Renames.Contains(stringNames))
                return true;

            __result = LIShipStatus.Instance.Renames.Get(stringNames);
            return false;
        }
    }
}