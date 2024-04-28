using UnityEngine;
using HarmonyLib;
using System;
using AirportCEOModLoader.Core;

namespace PerformanceCEO;
static class RAMReducerManager
{
    public static bool LiveryImporterCall = false;
}

[HarmonyPatch(typeof(LiveryImporter))]
[HarmonyPatch("LoadCustomLivery")]
static class Patch_RAMReducerChecker
{
    public static void Prefix()
    { 
        RAMReducerManager.LiveryImporterCall = true;
    }

    public static void Postfix() { RAMReducerManager.LiveryImporterCall = false; }
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
            PerformanceCEO.LogError($"Error occured while reducing RAM usage (Patch_RAMReducerApplier). {ExceptionUtils.ProccessException(ex)}");
        }
    }
}