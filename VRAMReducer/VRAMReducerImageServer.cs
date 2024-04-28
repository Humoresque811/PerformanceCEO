using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AirportCEOModLoader.Core;
using HarmonyLib;
using UnityEngine;

namespace PerformanceCEO;

[HarmonyPatch(typeof(LiveryController), "TryGetLivery")]
class VRAMReducerImageServer
{
    // REMEBER THAT RETURN IS DIFFERENT, USE __RESULT!!!
    public static bool Prefix(LiveryController __instance, string aircraftType, string airlineName, out GameObject livery, ref bool __result)
    {
        try
        {
            if (PerformanceCEOConfig.DebugLogs.Value)
            {
                PerformanceCEO.LogInfo($"Livery Request for {airlineName} aircraft type {aircraftType} being proccessed.");
            }

            if (PerformanceCEOConfig.VRAMReductionModuleEnabled.Value == false || VRAMReducerManager.customAirlineNames.Contains(airlineName) == false)
            {
                if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server Exit 1"); }
                livery = null;
                return true;
            }

            if (!VRAMReducerManager.aircraftTypeLiveriesPathDict.ContainsKey(aircraftType)) // For code cleanliness sake
            {
                if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server Exit 2"); }
                livery = null;
                return true;
            }

            AirlineLivery airlineLivery = null;

            for (int i = 0; i < LiveryController.Instance.allLiveriesList.Count; i++)
            {
                if (!LiveryController.Instance.allLiveriesList[i].aircraftType.Equals(aircraftType))
                {
                    continue;
                }

                airlineLivery = LiveryController.Instance.allLiveriesList[i];
            }

            if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 1"); }

            livery = null;
            if (airlineName.Equals("GA") && airlineLivery != null)
            {
                if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 2"); }

                int liveryIndex = Utils.RandomRangeI(0f, (VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType].Count - 1 + airlineLivery.liveries.Count));

                if (liveryIndex <= VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType].Count - 1)
                {
                    livery = VRAMReducerLiveryCreator.GetLivery(VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType][liveryIndex], airlineName);
                }
                else
                {
                    livery = airlineLivery.liveries[liveryIndex - VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType].Count];
                }

                if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server Exit 3"); }
                __result = true;
                return false;
            }
            else
            {
                if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 3"); }

                List<PossibleLivery> possibleLiveryOutput = new List<PossibleLivery>();
                int specialLiveries = 0;
                List<string> possibleLiveries = VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType];

                if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 4"); }
                foreach (string liveryPath in possibleLiveries)
                {
                    if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CPL 5"); }
                    string airlineNameLivery = Regex.Replace(VRAMReducerManager.LiveryPathToAirlineNameSpecialDict[liveryPath].Item1.ToUpper(), "\\s+", "");
                    string airlineNameLooking = Regex.Replace(airlineName.ToUpper(), "\\s+", "");

                    if (!airlineNameLivery.Equals(airlineNameLooking))
                    {
                        continue;
                    }
                    if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CPL 6"); }
                    if (VRAMReducerManager.LiveryPathToAirlineNameSpecialDict[liveryPath].Item2)
                    {
                        specialLiveries++;
                    }
                    if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CPL 7"); }
                    possibleLiveryOutput.Add(new PossibleLivery()
                    {
                        prefabPath = liveryPath,
                        prefabAirlineName = airlineNameLivery,
                        isSpecial = VRAMReducerManager.LiveryPathToAirlineNameSpecialDict[liveryPath].Item2
                    });
                    if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CPL 8"); }
                }

                // break in decomp

                if (possibleLiveryOutput.Count > 0)
                {
                    if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 9"); }
                    if (possibleLiveryOutput.Count == 1)
                    {
                        livery = VRAMReducerLiveryCreator.GetLivery(possibleLiveryOutput[0].prefabPath, possibleLiveryOutput[0].prefabAirlineName);
                        if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server Exit 4"); }
                        __result = true;
                        return false;
                    }
                    if (specialLiveries == 0)
                    {
                        if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 10"); }
                        int liveryIndex = Utils.RandomRangeI(0f, (possibleLiveryOutput.Count - 1));
                        livery = VRAMReducerLiveryCreator.GetLivery(possibleLiveryOutput[liveryIndex].prefabPath, airlineName);
                        if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 11"); }
                    }
                    else
                    {
                        if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 12"); }
                        bool thirdOccured = Utils.ChanceOccured(0.33f);
                        possibleLiveryOutput.ShuffleList<PossibleLivery>();
                        for (int i = 0; i < possibleLiveryOutput.Count; i++)
                        {
                            PossibleLivery possibleLivery = possibleLiveryOutput[i];
                            if (thirdOccured && possibleLivery.isSpecial)
                            {
                                livery = VRAMReducerLiveryCreator.GetLivery(possibleLivery.prefabPath, airlineName);
                                break;
                            }
                            if (!thirdOccured && !possibleLivery.isSpecial)
                            {
                                livery = VRAMReducerLiveryCreator.GetLivery(possibleLivery.prefabPath, airlineName);
                                break;
                            }
                        }
                        if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CP 13"); }
                    }

                    if (livery != null)
                    {
                        if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server Exit 5"); }
                        __result = true;
                        return false;
                    }
                }
            }

            for (int k = 0; k < VRAMReducerManager.aircraftTypeLiveriesPathDict.Keys.Count; k++)
            {
                if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server CPL 14"); }
                if (VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType].Count > 0)
                {
                    int liveryIndex = Utils.RandomRangeI(0f, (VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType].Count - 1));
                    livery = VRAMReducerLiveryCreator.GetLivery(VRAMReducerManager.aircraftTypeLiveriesPathDict[aircraftType][liveryIndex], airlineName);
                    if (PerformanceCEOConfig.DebugLogs.Value) { PerformanceCEO.LogInfo("Image Server Exit 6"); }
                    __result = true;
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            PerformanceCEO.LogError($"An error occured in the Image Server. The error was {ExceptionUtils.ProccessException(ex)}.");
        }
        Debug.LogError("Could not find a suitable livery for! " + aircraftType);
        PerformanceCEO.LogError("Something went very very very wrong. Error! Probably was bound to happen... :( (Exit 7)");
		    livery = null;
        __result = false;
		    return true;
    }
    
    internal class PossibleLivery
    {
        public string prefabPath;
        public string prefabAirlineName;
        public bool isSpecial;
    }
}
