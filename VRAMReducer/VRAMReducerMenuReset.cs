using HarmonyLib;

namespace PerformanceCEO;

[HarmonyPatch(typeof(LiveryController), nameof(LiveryController.ResetForMainMenu))]
static class VRAMReducerMenuReset
{
    public static void Postfix()
    {
        VRAMReducerManager.ClearPathAirlineList();
    }
}
