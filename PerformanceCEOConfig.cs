using System;
using BepInEx.Configuration;
using Epic.OnlineServices;

namespace PerformanceCEO;

internal class PerformanceCEOConfig
{
    internal static ConfigEntry<bool> RAMReductionModuleEnabled { get; set; }
    internal static ConfigEntry<bool> VRAMReductionModuleEnabled { get; set; }
    internal static ConfigEntry<bool> RemoveMainMenuAircraft { get; set;}
    internal static ConfigEntry<bool> UseTests { get; set; }
    internal static ConfigEntry<bool> DebugLogs { get; set; }

    internal static void SetUpConfig()
    {
        RAMReductionModuleEnabled = PerformanceCEO.ConfigReference.Bind<bool>("General", "Use Ram Reducer", true, "Try to reduce ram usage of game");
        VRAMReductionModuleEnabled = PerformanceCEO.ConfigReference.Bind<bool>("General", "Use VRam Reducer", true, "Try to reduce VRam usage of game");
        DebugLogs = PerformanceCEO.ConfigReference.Bind<bool>("General", "Addition Debug Logs", false, "Log additional debug information");
        RemoveMainMenuAircraft = PerformanceCEO.ConfigReference.Bind<bool>("General", "Remove Main Menu Aircraft", false, "Removes the decorative aircraft " +
            "in the main menu, considerably increases main menu performance");
        UseTests = PerformanceCEO.ConfigReference.Bind<bool>("General", "Run performance tests", false, "Runs performance tests if they exist. Only enable if " +
            "asked to");
    }
}