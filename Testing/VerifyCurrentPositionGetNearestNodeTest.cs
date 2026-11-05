using System.Diagnostics;
using HarmonyLib;
using Nodes;

namespace PerformanceCEO;

[HarmonyPatch(typeof(GridController), nameof(GridController.GetNearestNode))]
internal class VerifyCurrentPositionGetNearestNodeTest
{
    static bool ShouldTest => PerformanceCEOConfig.UseTests.Value;

    [HarmonyPrefix]
    public static bool Prefix(ref PersonNode __result)
    {
        if (!ShouldTest)
        {
            return true;
        }

        StackTrace stackTrace = new StackTrace();
        if (stackTrace.GetFrame(1)?.GetMethod().Name == nameof(PersonController.VerifyCurrentPosition))
        {
            __result = null;
            return false;
        }

        return true;
    }
}
