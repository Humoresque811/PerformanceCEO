using System;
using BepInEx.Configuration;
using PerformanceCEO.DownCompressor;

namespace PerformanceCEO;

internal class PerformanceCEOConfig
{
    internal static ConfigEntry<bool> RAMReductionModuleEnabled { get; set; }
    internal static ConfigEntry<bool> LoadOnDemandModuleEnabled { get; set; }
    internal static ConfigEntry<bool> CompressTextures { get; private set; }
    internal static ConfigEntry<DownCompressionEnums.DownscaleLevel> DownscaleLevel { get; private set; }

    internal static ConfigEntry<bool> RemoveMainMenuAircraft { get; set;}
    internal static ConfigEntry<bool> UseTests { get; set; }
    internal static ConfigEntry<bool> DebugLogs { get; set; }

    internal static void SetUpConfig()
    {
        RAMReductionModuleEnabled = ConfigRef.Bind("Liveries", "Use Ram Reducer Module", true, "Try to reduce RAM usage of game");
        LoadOnDemandModuleEnabled = ConfigRef.Bind("Liveries", "Use Load on Demand Module", true, "Try to reduce VRAM usage of game. " +
            "This is done by only loading textures on demand, thus reducing upfront cost and not loading unnecessary textures.");
        CompressTextures = ConfigRef.Bind("Liveries", "Compress Textures", false, "Overrides image compression settings in graphics settings. " +
            "May result in empty texture, if so, disable. Otherwise, keep enabled for possibly better performance.");
        DownscaleLevel = ConfigRef.Bind("Liveries", "Downscaling Level", DownCompressionEnums.DownscaleLevel.Downscale2X,
            "Amount to downscale the texture by (per axis, so 2x means you get 4x less quality, 2x less VRAM/RAM usage)");

        DebugLogs = ConfigRef.Bind("General", "Addition Debug Logs", false, "Log additional debug information");
        RemoveMainMenuAircraft = ConfigRef.Bind("General", "Remove Main Menu Aircraft", false, "Removes the decorative aircraft " +
            "in the main menu, considerably increases main menu performance");
        UseTests = ConfigRef.Bind("General", "Run performance tests", false, "Runs performance tests if they exist. Only enable if " +
            "asked to.");
    }

    static ConfigFile ConfigRef => PerformanceCEO.ConfigReference;
}