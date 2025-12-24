using UnityEngine;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Increase Needle Damage", category = "Player")]
    public static void IncreaseNeedleDamage()
    {
        if (PlayerData.instance.nailDamage == 0)
        {
            PlayerData.instance.nailUpgrades = 0;
            DebugMod.extraNailDamage = 0;
            DebugMod.LogConsole("Resetting needle damage to 5");
        }
        else if (PlayerData.instance.nailUpgrades == 4 || DebugMod.extraNailDamage < 0)
        {
            DebugMod.extraNailDamage += 4;
            DebugMod.LogConsole("Adding 4 extra needle damage");
        }
        else
        {
            PlayerData.instance.nailUpgrades++;
            DebugMod.LogConsole("Adding needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    [BindableMethod(name = "Decrease Needle Damage", category = "Player")]
    public static void DecreaseNeedleDamage()
    {
        if (PlayerData.instance.nailUpgrades == 0 || DebugMod.extraNailDamage > 0)
        {
            DebugMod.extraNailDamage -= 4;
            if (DebugMod.extraNailDamage < -5)
            {
                DebugMod.extraNailDamage = -5;
                DebugMod.LogConsole("Setting needle damage to 0");
            }
            else
            {
                DebugMod.LogConsole("Reducing nail damage by 4");
            }
        }
        else
        {
            PlayerData.instance.nailUpgrades--;
            DebugMod.LogConsole("Removing needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    [BindableMethod(name = "Set Hazard Respawn", category = "Player")]
    public static void SetHazardRespawn()
    {
        Vector3 manualRespawn = DebugMod.RefKnight.transform.position;
        HeroController.instance.SetHazardRespawn(manualRespawn, false);
        DebugMod.LogConsole("Manual respawn point on this map set to" + manualRespawn);
    }

    [BindableMethod(name = "Hazard Respawn", category = "Player")]
    public static void Respawn()
    {
        if (GameManager.instance.IsGameplayScene() && !HeroController.instance.cState.dead &&
            PlayerData.instance.health > 0)
        {
            if (UIManager.instance.uiState.ToString() == "PAUSED")
            {
                InputHandler.Instance.StartCoroutine(GameManager.instance.PauseGameToggle(false));
                GameManager.instance.HazardRespawn();
                DebugMod.LogConsole("Closing Pause Menu and respawning...");
                return;
            }

            if (UIManager.instance.uiState.ToString() == "PLAYING")
            {
                HeroController.instance.RelinquishControl();
                GameManager.instance.HazardRespawn();
                HeroController.instance.RegainControl();
                DebugMod.LogConsole("Respawn signal sent");
                return;
            }

            DebugMod.LogConsole("Respawn requested in some weird conditions, abort, ABORT");
        }
    }

    [BindableMethod(name = "Damage Self", category = "Player")]
    public static void SelfDamage()
    {
        if (PlayerData.instance.health <= 0)
        {
            DebugMod.LogConsole("Cannot damage self: health <= 0");
        }
        else if (HeroController.instance.cState.dead)
        {
            DebugMod.LogConsole("Cannot damage self: player is dead");
        }
        else if (!GameManager.instance.IsGameplayScene())
        {
            DebugMod.LogConsole("Cannot damage self: not a gameplay scene");
        }
        else if (HeroController.instance.cState.recoiling)
        {
            DebugMod.LogConsole("Cannot damage self: player is recoiling");
        }
        else if (HeroController.instance.cState.invulnerable)
        {
            DebugMod.LogConsole("Cannot damage self: player is invulnerable");
        }
        else
        {
            HeroController.instance.DamageSelf(1);
            DebugMod.LogConsole("Attempting self damage");
        }
    }

    [BindableMethod(name = "Kill Self", category = "Player")]
    public static void KillSelf()
    {
        if (!HeroController.instance.cState.dead && !HeroController.instance.cState.transitioning)
        {
            HeroController.instance.StartCoroutine(HeroController.instance.Die(false, false));
            DebugMod.LogConsole("Killed player");
        }
    }

    [BindableMethod(name = "Break Cocoon", category = "Player")]
    public static void BreakCocoon()
    {
        HeroController.instance?.CocoonBroken();
        EventRegister.SendEvent("BREAK HERO CORPSE");
    }
}