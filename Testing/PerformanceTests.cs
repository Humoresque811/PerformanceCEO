using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using AirportCEOModLoader.Core;

namespace PerformanceCEO;

[HarmonyPatch]
internal class PerformanceTests
{
    static Stopwatch stopwatch = new Stopwatch();

    static bool ShouldTest => PerformanceCEOConfig.UseTests.Value;

    internal static void DoTests()
    {

    }

    //internal static Dictionary<PlaceableRoom, List<DeskController>> cafeDesksCahce = new();
    //internal static Dictionary<PlaceableRoom, List<DeskController>> shopDesksCahce = new();

    //[HarmonyPatch(typeof(PersonController), nameof(PersonController.GetPotentialPreferredDesk))]
    //[HarmonyPrefix]
    //public static bool GetPrefferedDesk(PersonController __instance, ref DeskController __result)
    //{
    //    if (PerformanceCEOConfig.UseTests.Value == false)
    //    {
    //        return true;
    //    }

    //    if (__instance.attemptedPreferredDeskType == Enums.DeskType.CafeCounter || __instance.attemptedPreferredDeskType == Enums.DeskType.ShopCounter)
    //    {

    //        __instance.SetCurrentRoom();
    //        if (__instance.currentRoom == null)
    //        {
    //            __result = null;
    //            return false;
    //        }

    //        Enums.ItemType itemType = __instance.attemptedPreferredDeskType == Enums.DeskType.CafeCounter ? Enums.ItemType.CafeCounter : Enums.ItemType.ShopCounter;

    //        List<DeskController> deskControllers;

    //        if (cafeDesksCahce.ContainsKey(__instance.currentRoom))
    //        {
    //            PerformanceCEO.LogInfo($"A {cafeDesksCahce.ContainsKey(__instance.currentRoom)}, B {cafeDesksCahce[__instance.currentRoom].Count} ");
    //        }
    //        if (shopDesksCahce.ContainsKey(__instance.currentRoom))
    //        {
    //            PerformanceCEO.LogInfo($"C {shopDesksCahce.ContainsKey(__instance.currentRoom)}, D {shopDesksCahce[__instance.currentRoom].Count} ");
    //        }


    //        if (cafeDesksCahce.ContainsKey(__instance.currentRoom) && cafeDesksCahce[__instance.currentRoom].Count > 0)
    //        {
    //            deskControllers = cafeDesksCahce[__instance.currentRoom];
    //        }
    //        else if (shopDesksCahce.ContainsKey(__instance.currentRoom) && shopDesksCahce[__instance.currentRoom].Count > 0)
    //        {
    //            deskControllers = shopDesksCahce[__instance.currentRoom];
    //        }
    //        else
    //        {
    //            PerformanceCEO.LogInfo("returned true 2");
    //            return true;
    //        }

    //        __result = AirportController.Instance.GetPreferredDeskByDistanceAndQueueLoad(deskControllers, __instance.FloorPosition);
    //        return false;
    //    }
    //    PerformanceCEO.LogInfo("returned true 1");
    //    return true;
    //}

    //[HarmonyPatch(typeof(DeskController), nameof(DeskController.ToggleActivation))]
    //[HarmonyPostfix]
    //public static void AddDesk(DeskController __instance, bool status)
    //{
    //    AddDeskToLists(__instance, status);
    //}
    //[HarmonyPatch(typeof(DeskController), nameof(DeskController.ChangeToPlaced))]
    //[HarmonyPostfix]
    //public static void AddDesk2(DeskController __instance)
    //{
    //    AddDeskToLists(__instance, true);
    //}

    //private static void AddDeskToLists(DeskController __instance, bool status)
    //{
    //    PerformanceCEO.LogInfo("adding desk");
    //    try
    //    {
    //        if (__instance.ParentRoom == null)
    //        {
    //            return;
    //        }


    //        if (status == true)
    //        {

    //            if (__instance.itemType == Enums.ItemType.CafeCounter)
    //            {
    //                if (cafeDesksCahce[__instance.ParentRoom].Count > 0)
    //                {
    //                    cafeDesksCahce[__instance.ParentRoom].Add(__instance);
    //                    PerformanceCEO.LogInfo("added desk 1");
    //                    return;
    //                }
    //                PerformanceCEO.LogInfo("added desk 2");
    //                cafeDesksCahce[__instance.ParentRoom] = new List<DeskController>(4) { __instance };
    //                return;
    //            }
    //            else if (__instance.itemType == Enums.ItemType.ShopCounter)
    //            {
    //                if (shopDesksCahce[__instance.ParentRoom].Count > 0)
    //                {
    //                    PerformanceCEO.LogInfo("added desk 3");
    //                    shopDesksCahce[__instance.ParentRoom].Add(__instance);
    //                    return;
    //                }
    //                PerformanceCEO.LogInfo("added desk 4");
    //                shopDesksCahce[__instance.ParentRoom] = new List<DeskController>(4) { __instance };
    //                return;
    //            }
    //        }
    //        else
    //        {
    //            if (cafeDesksCahce.ContainsKey(__instance.ParentRoom) && cafeDesksCahce[__instance.ParentRoom].Contains(__instance))
    //            {
    //                cafeDesksCahce[__instance.ParentRoom].Remove(__instance);
    //            }
    //            if (shopDesksCahce.ContainsKey(__instance.ParentRoom) && shopDesksCahce[__instance.ParentRoom].Contains(__instance))
    //            {
    //                shopDesksCahce[__instance.ParentRoom].Remove(__instance);
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        PerformanceCEO.LogError("Error syaeas " + ExceptionUtils.ProccessException(ex));
    //    }
    //}

    //[HarmonyPatch(typeof(DeskController), nameof(DeskController.Remove))]
    //[HarmonyPostfix]
    //public static void RemoveDesk(DeskController __instance)
    //{
    //    if (cafeDesksCahce.ContainsKey(__instance.ParentRoom) && cafeDesksCahce[__instance.ParentRoom].Contains(__instance))
    //    {
    //        cafeDesksCahce[__instance.ParentRoom].Remove(__instance);
    //    }
    //    if (shopDesksCahce.ContainsKey(__instance.ParentRoom) && shopDesksCahce[__instance.ParentRoom].Contains(__instance))
    //    {
    //        shopDesksCahce[__instance.ParentRoom].Remove(__instance);
    //    }
    //}
}
