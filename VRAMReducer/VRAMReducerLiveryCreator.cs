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
	// Most of this is from the game, specific portions are marked as not, but not all modifications are noted.
    public static GameObject GetLivery(string filePath, string airlineName)
    {
		return LiveryImporterUniversal.LoadLivery(filePath, airlineName, out LiveryData _);
    }
}

