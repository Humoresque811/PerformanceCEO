using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace PerformanceCEO;

[HarmonyPatch]
public class MainMenuAircraftImprovements
{
    [HarmonyPatch(typeof(MainMenuWorldEnvironmentController), "LaunchAircraftFormationSimulation")]
    [HarmonyPrefix]
    public static bool RemoveFormationFlights(MainMenuWorldEnvironmentController __instance)
    {
        if (PerformanceCEOConfig.RemoveMainMenuAircraft.Value)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(MainMenuWorldEnvironmentController), "LaunchAircraftSimulation")]
    [HarmonyPrefix]
    public static bool RemoveRandomFlights(MainMenuWorldEnvironmentController __instance)
    {
        if (PerformanceCEOConfig.RemoveMainMenuAircraft.Value)
        {
            return false;
        }

        return true;
    }
}
