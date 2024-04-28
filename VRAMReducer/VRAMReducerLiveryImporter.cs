using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceCEO;

[HarmonyPatch(typeof(LiveryImporter))]
[HarmonyPatch("LoadCustomLivery")]
static class VRAMReducerLiveryImporter
{
    public static bool Prefix(string filePath, string airlineName)
    {
        if (PerformanceCEOConfig.VRAMReductionModuleEnabled.Value == false)
        {
            return true;
        } 

        filePath = filePath.Replace("\\", "/");
        string[] directories = Directory.GetDirectories(filePath);
        for (int i = 0; i < directories.Length; i++)
        {
            string[] JSONFiles = Directory.GetFiles(directories[i], "*.json");
            if (JSONFiles.Length == 0) { continue; }

            string[] PNGFiles = Directory.GetFiles(directories[i], "*.png");
            if (PNGFiles.Length == 0) { continue; }

            LiveryData liveryData = Utils.CreateFromJSON<LiveryData>(Utils.ReadFile(JSONFiles[0]));
            VRAMReducerManager.AddInfoToDicts(liveryData.aircraftType, directories[i], airlineName, liveryData.isSpecial);
        }
        return false;
    }
}
