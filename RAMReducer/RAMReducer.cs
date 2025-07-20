using UnityEngine;
using HarmonyLib;
using System;
using AirportCEOModLoader.Core;
using System.IO;
using PerformanceCEO.General;
using Newtonsoft.Json;

namespace PerformanceCEO;

[HarmonyPatch(typeof(LiveryImporter))]
[HarmonyPatch("LoadCustomLivery")]
static class Patch_RAMReducerChecker
{
    public static bool Prefix(ref string filePath, string airlineName)
    {
        filePath = filePath.Replace("\\", "/");
        string[] directories = Directory.GetDirectories(filePath);
        for (int i = 0; i < directories.Length; i++)
        {
            string[] JSONFiles = Directory.GetFiles(directories[i], "*.json");
            if (JSONFiles.Length == 0) { continue; }

            string[] PNGFiles = Directory.GetFiles(directories[i], "*.png");
            if (PNGFiles.Length == 0) { continue; }

            if (PerformanceCEOConfig.LoadOnDemandModuleEnabled.Value)
            {

                LiveryData liveryData = JsonConvert.DeserializeObject<LiveryData>(Utils.ReadFile(JSONFiles[0]));
                VRAMReducerManager.AddInfoToDicts(liveryData.aircraftType, directories[i], airlineName, liveryData.isSpecial);
            }
            if (PerformanceCEOConfig.RAMReductionModuleEnabled.Value)
            {
                GameObject planeLivery = LiveryImporterUniversal.LoadLivery(directories[i], airlineName, out LiveryData liveryData);
                if (planeLivery == null)
                {
                    continue;
                }
                SingletonNonDestroy<LiveryController>.Instance.AddLivery(liveryData.aircraftType, planeLivery);
            }
        }

        return false;
    }
}
