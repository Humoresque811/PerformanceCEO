using AirportCEOModLoader;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PerformanceCEO.NewUpdateSystem;
using PerformanceCEO.OtherTweaks;
using UnityEngine;

namespace PerformanceCEO;

[BepInPlugin("org.performanceCEO.humoresque", "Performance CEO", "1.1.0")]
[BepInDependency("org.airportceomodloader.humoresque")]
public class PerformanceCEO : BaseUnityPlugin
{
    public static PerformanceCEO Instance { get; private set; }
    internal static Harmony Harmony { get; private set; }
    internal static ManualLogSource PCEOLogger { get; private set; }
    internal static ConfigFile ConfigReference {  get; private set; }

    private void Awake()
    {
        Harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        Harmony.PatchAll(); 

        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        Instance = this;
        PCEOLogger = Logger;
        ConfigReference = Config;

        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is setting up config.");
        PerformanceCEOConfig.SetUpConfig();
        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} finished setting up config.");

        GameObject child = Instantiate(new GameObject());
        child.transform.SetParent(null);
        child.name = "PerformanceCEOActive";

        Logger.LogInfo("Finished Awake");
    }
    
    private void Start()
    {
        AirportCEOModLoader.WatermarkUtils.WatermarkUtils.Register(new AirportCEOModLoader.WatermarkUtils.WatermarkInfo("PCEO", "1.2.1", true));
        AirportCEOModLoader.SaveLoadUtils.EventDispatcher.EndOfLoad += DLCCheckFix.NewCheckDLCOwnage;
        PerformanceTests.DoTests();

        //gameObject.AddComponent<NewUpdateManager>();

        Logger.LogInfo("Finished Start");
    }

    internal static void LogInfo(string message) => PCEOLogger.LogInfo(message);
    internal static void LogError(string message) => PCEOLogger.LogError(message);
}
