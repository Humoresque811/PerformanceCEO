using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceCEO.OtherTweaks;

[HarmonyPatch]
internal static class DLCCheckFix
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DLCManager), nameof(DLCManager.CheckDLCOwnage))]
    internal static bool CheckDLCPatch()
    {
		if (!DLCManager.hasCheckedDLC)
		{
			NewCheckDLCOwnage();
		}
		return false;
    }

    internal static void NewCheckDLCOwnage(SaveLoadGameDataController _)
	{
		NewCheckDLCOwnage();
	}

    internal static void NewCheckDLCOwnage()
    {
		if (SteamManager.Initialized)
		{
			DLCManager.ownsSupersonicDLC = DLCManager.OwnsDLC(1525630uL, "steam");
			DLCManager.ownsVintageDLC = DLCManager.OwnsDLC(1649580uL, "steam");
			DLCManager.ownsBeastsOfTheEastDLC = DLCManager.OwnsDLC(1844590uL, "steam");
			DLCManager.ownsHelicopterDLC = DLCManager.OwnsDLC(2555540uL, "steam");
		}
		if (!DLCManager.ownsSupersonicDLC)
		{
			DLCManager.ownsSupersonicDLC = DLCManager.OwnsDLC(1116054340uL, "gog");
		}
		if (!DLCManager.ownsVintageDLC)
		{
			DLCManager.ownsVintageDLC = DLCManager.OwnsDLC(2027337642uL, "gog");
		}
		if (!DLCManager.ownsBeastsOfTheEastDLC)
		{
			DLCManager.ownsBeastsOfTheEastDLC = DLCManager.OwnsDLC(1587865064uL, "gog");
		}
		if (!DLCManager.ownsHelicopterDLC)
		{
			DLCManager.ownsHelicopterDLC = DLCManager.OwnsDLC(1992252315uL, "gog");
		}
		DLCManager.hasCheckedDLC = true;
	}
}
