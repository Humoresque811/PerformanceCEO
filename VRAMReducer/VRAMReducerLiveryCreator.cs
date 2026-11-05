using PerformanceCEO;
using PerformanceCEO.General;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerformanceCEO;
static class VRAMReducerLiveryCreator
{
    private static readonly Dictionary<string, GameObject> liveryCache = new Dictionary<string, GameObject>();

	// Most of this is from the game, specific portions are marked as not, but not all modifications are noted.
    public static GameObject GetLivery(string filePath, string airlineName)
    {
        string key = $"{filePath}|{airlineName}|{PerformanceCEOConfig.DownscaleLevel.Value}|{PerformanceCEOConfig.CompressTextures.Value}";
        if (liveryCache.TryGetValue(key, out GameObject cachedLivery) && cachedLivery != null)
        {
            return cachedLivery;
        }

        GameObject livery = LiveryImporterUniversal.LoadLivery(filePath, airlineName, out LiveryData _);
        if (livery != null)
        {
            liveryCache[key] = livery;
        }
        return livery;
    }
}

