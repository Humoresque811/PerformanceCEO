using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PerformanceCEO;

static class VRAMReducerManager
{
    // Aircraft type -> A List of the paths of aircraft directories
    public static Dictionary<string, List<string>> aircraftTypeLiveriesPathDict = new Dictionary<string, List<string>>();

    // Livery path -> Airline Name, isSpecial bool
    public static Dictionary<string, Tuple<string, bool>> LiveryPathToAirlineNameSpecialDict = new Dictionary<string, Tuple<string, bool>>();

    // General list (DONT USE ORDER HERE)
    public static List<string> customAirlineNames = new List<string>();

    public static void AddInfoToDicts(string aircraftType, string LiveryDirectoryPath, string airlineName, bool isSpecial)
    {
        if (aircraftTypeLiveriesPathDict.ContainsKey(aircraftType))
        {
            List<string> allLiveryPaths = aircraftTypeLiveriesPathDict[aircraftType];
            allLiveryPaths.Add(LiveryDirectoryPath);
            aircraftTypeLiveriesPathDict[aircraftType] = allLiveryPaths;
        }
        else
        {
            aircraftTypeLiveriesPathDict.Add(aircraftType, new List<string> { LiveryDirectoryPath });
        }

        LiveryPathToAirlineNameSpecialDict.Add(LiveryDirectoryPath, new Tuple<string, bool>(airlineName, isSpecial));

        if (!customAirlineNames.Contains(airlineName))
        {
            customAirlineNames.Add(airlineName);
        }
    }

    public static void ClearPathAirlineList()
    {
        aircraftTypeLiveriesPathDict = new Dictionary<string, List<string>>();
        LiveryPathToAirlineNameSpecialDict = new Dictionary<string, Tuple<string, bool>>();
    }
}
