using DebugMod.Helpers;
using DebugMod.SaveStates;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using UnityEngine;

namespace DebugMod;

[HarmonyPatch]
public partial class DebugMod
{
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.nailDamage), MethodType.Getter)]
    [HarmonyPostfix]
    private static int Get_NailDamage(int nailDamage)
    {
        return nailDamage + extraNailDamage;
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.TakeDamage))]
    [HarmonyPrefix]
    private static void TakeDamage(HealthManager __instance, HitInstance hitInstance)
    {
        HitInstance scaled = __instance.ApplyDamageScaling(hitInstance);
        lastHit = scaled;
        lastDamage = __instance.damageOverride ? 1 : Mathf.RoundToInt(scaled.DamageDealt * scaled.Multiplier);

        lastScaling = __instance.damageScaling;

        int scaleLevel = hitInstance.DamageScalingLevel - 1;
        if (hitInstance.IsUsingNeedleDamageMult)
        {
            scaleLevel = PlayerData.instance.nailUpgrades;
        }
        else if (hitInstance.RepresentingTool && hitInstance.RepresentingTool.Type != ToolItemType.Skill)
        {
            scaleLevel = PlayerData.instance.ToolKitUpgrades;
        }
        lastScaleLevel = scaleLevel;
    }

    // Prevents clipping through water when invincible
    [HarmonyPatch(typeof(SurfaceWaterRegion), nameof(SurfaceWaterRegion.OnTriggerEnter2D))]
    [HarmonyPrefix]
    private static void OnTriggerEnter2D_Prefix(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<HeroController>() && playerInvincible)
        {
            PlayerData.instance.isInvincible = false;
        }
    }

    [HarmonyPatch(typeof(SurfaceWaterRegion), nameof(SurfaceWaterRegion.OnTriggerEnter2D))]
    [HarmonyPostfix]
    private static void OnTriggerEnter2D_Postfix()
    {
        if (playerInvincible)
        {
            PlayerData.instance.isInvincible = true;
        }
    }

    // Prevents savestates loaded while in water from warping to the wrong point.
    [HarmonyPatch(typeof(HeroWaterController), nameof(HeroWaterController.TumbleOut))]
    [HarmonyPrefix]
    private static bool HeroWaterController_TumbleOut_Prefix(HeroWaterController __instance)
    {
        if (SaveState.loadingSavestate == null) return true;

        __instance.ExitedWater();
        return false;
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeSilk), typeof(int), typeof(SilkSpool.SilkTakeSource))]
    [HarmonyPrefix]
    private static void TakeSilk(ref int amount)
    {
        if (infiniteSilk)
        {
            amount = 0;
        }
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.DoSpecialDamage))]
    [HarmonyPrefix]
    private static bool HeroController_DoSpecialDamage()
    {
        return !playerInvincible;
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanBeBarnacleGrabbed))]
    [HarmonyPrefix]
    private static bool HeroController_CanBeBarnacleGrabbed(ref bool __result)
    {
        // This function does not check for invincibility, unlike other types of grabs
        if (playerInvincible)
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
    [HarmonyPrefix]
    private static void PlayMakerFSM_Start(PlayMakerFSM __instance)
    {
        // The Underworks saw block FSM is bugged and damages you even if you are invincible
        if (__instance.gameObject.name == "Hero Damager" && __instance.FsmName == "Multihitter")
        {
            CanHeroTakeDamage action = __instance.GetState("Start Hit?")?.Actions.OfType<CanHeroTakeDamage>().FirstOrDefault();
            if (action != null && action.canTakeDmgEvent.Name == "HIT" && action.cannotTakeDmgEvent.Name == "HIT")
            {
                action.cannotTakeDmgEvent = new FsmEvent("CANCEL");
            }
        }
    }

    [HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.CanChangeEquips), [])]
    [HarmonyPrefix]
    private static bool InventoryItemToolManager_CanChangeEquips(ref bool __result)
    {
        if (freeEquip)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetIsInventoryOpen))]
    [HarmonyPrefix]
    private static void GameManager_SetIsInventoryOpen(bool value)
    {
        if (freeEquip && !value)
        {
            // Would normally be called by the inventory FSM,
            // but the action is skipped if atBench is false
            ToolItemManager.SendEquippedChangedEvent();
        }
    }
}
