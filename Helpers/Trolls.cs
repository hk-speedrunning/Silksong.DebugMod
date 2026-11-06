using DebugMod.UI;
using HarmonyLib;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DebugMod.Helpers;

[HarmonyPatch]
internal static class Trolls
{
    private static bool? rosaryRocksTroll;

    private static bool RosaryRocksActive()
    {
        if (GameManager.instance.sceneName != "Bone_01c" || GameManager.instance.entryGateName != "left1")
        {
            return false;
        }

        if (!rosaryRocksTroll.HasValue)
        {
            float chance = 1f / 200f;

            try
            {
                string username = SteamFriends.GetPersonaName();
                if (username == "Jamie<3")
                {
                    chance = 1f / 10f;
                }
            }
            catch { }

            rosaryRocksTroll = Random.value < chance;

            if (rosaryRocksTroll.Value)
            {
                DebugMod.Log("https://discord.com/channels/772964112908156938/1365522289143054427/1486446585737904440");
                DebugMod.Log(";)");
            }
        }

        return rosaryRocksTroll.Value;
    }

    private static int extraRosaryCount;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.FinishedEnteringScene))]
    [HarmonyPrefix]
    private static void GameManager_FinishedEnteringScene()
    {
        extraRosaryCount = 0;
    }

    [HarmonyPatch(typeof(CurrencyCounter), nameof(CurrencyCounter.Add))]
    [HarmonyPrefix]
    private static void CurrencyCounter_Add(ref int amount, CurrencyType type)
    {
        if (type == CurrencyType.Money && RosaryRocksActive())
        {
            extraRosaryCount += amount;
            amount *= 2;
        }
    }

    [HarmonyPatch(typeof(CurrencyCounter), nameof(CurrencyCounter.Count), MethodType.Getter)]
    [HarmonyPostfix]
    private static void CurrencyCounter_Count_Get(CurrencyCounter __instance, ref int __result)
    {
        if (__instance.CounterType == CurrencyType.Money && RosaryRocksActive())
        {
            __result += extraRosaryCount;
        }
    }

    public static void OnResourcesLoaded()
    {
        DateTime time = DateTime.Now;

        if (time.Month == 4 && time.Day == 1)
        {
            string fontName = Font.GetOSInstalledFontNames().FirstOrDefault(x => x.Contains("Comic Sans"));

            if (fontName != null)
            {
                UICommon.arial = Font.CreateDynamicFontFromOSFont(fontName, UICommon.FontSize);
            }
        }
    }
}

