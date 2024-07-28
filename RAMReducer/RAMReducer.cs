using UnityEngine;
using HarmonyLib;
using System;
using AirportCEOModLoader.Core;
using System.IO;
using PerformanceCEO.General;
using Newtonsoft.Json;

namespace PerformanceCEO;
static class RAMReducerManager
{
    public static bool LiveryImporterCall = false;
}

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
            else
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

[HarmonyPatch(typeof(Sprite), new Type[] { typeof(Texture2D), typeof(Rect), typeof(Vector2), typeof(float), typeof(uint), typeof(SpriteMeshType)})]
[HarmonyPatch("Create")]
static class Patch_RAMReducerApplier
{
    public static void Prefix(ref Texture2D texture)
    {
        if (RAMReducerManager.LiveryImporterCall == false || !texture.isReadable || !PerformanceCEOConfig.RAMReductionModuleEnabled.Value)
        {
            return;
        }

        try
        {
            // This does prevent editing of the sprite later on, shouldn't be an issue
            texture.Apply(false, true);
        }
        catch (Exception ex)
        {
            PerformanceCEO.LogError($"Error occurred while reducing RAM usage (Patch_RAMReducerApplier). {ExceptionUtils.ProccessException(ex)}");
        }
    }
}