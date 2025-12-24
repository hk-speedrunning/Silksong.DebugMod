using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Kill All", category = "Cheats")]
    public static void KillAll()
    {
        int ctr = 0;

        foreach (HealthManager hm in Object.FindObjectsByType<HealthManager>(FindObjectsSortMode.None))
        {
            if (!hm.isDead)
            {
                hm.Die(null, AttackTypes.Generic, true);
                ctr++;
            }
        }
        DebugMod.LogConsole($"Killing {ctr} HealthManagers in scene!");
    }

    [BindableMethod(name = "Infinite Jump", category = "Cheats")]
    public static void ToggleInfiniteJump()
    {
        PlayerData.instance.infiniteAirJump = !PlayerData.instance.infiniteAirJump;
        DebugMod.LogConsole("Infinite Jump set to " + PlayerData.instance.infiniteAirJump.ToString().ToUpper());
    }

    [BindableMethod(name = "Infinite Silk", category = "Cheats")]
    public static void ToggleInfiniteSilk()
    {
        DebugMod.infiniteSilk = !DebugMod.infiniteSilk;
        DebugMod.LogConsole("Infinite Silk set to " + DebugMod.infiniteSilk.ToString().ToUpper());
    }

    [BindableMethod(name = "Infinite HP", category = "Cheats")]
    public static void ToggleInfiniteHP()
    {
        DebugMod.infiniteHP = !DebugMod.infiniteHP;
        DebugMod.LogConsole("Infinite HP set to " + DebugMod.infiniteHP.ToString().ToUpper());
    }

    [BindableMethod(name = "Infinite Tools", category = "Cheats")]
    public static void ToggleInfiniteTools()
    {
        DebugMod.infiniteTools = !DebugMod.infiniteTools;
        DebugMod.LogConsole("Infinite Tools set to " + DebugMod.infiniteTools.ToString().ToUpper());
    }

    [BindableMethod(name = "Invincibility", category = "Cheats")]
    public static void ToggleInvincibility()
    {
        DebugMod.playerInvincible = !DebugMod.playerInvincible;
        DebugMod.LogConsole("Invincibility set to " + DebugMod.playerInvincible.ToString().ToUpper());

        PlayerData.instance.isInvincible = DebugMod.playerInvincible;
    }

    [BindableMethod(name = "Noclip", category = "Cheats")]
    public static void ToggleNoclip()
    {
        DebugMod.noclip = !DebugMod.noclip;

        if (DebugMod.noclip)
        {
            DebugMod.LogConsole("Enabled noclip");
            DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
        }
        else
        {
            DebugMod.LogConsole("Disabled noclip");
            DebugMod.RefKnight.GetComponent<Rigidbody2D>().constraints &= ~RigidbodyConstraints2D.FreezePosition;
        }
    }

    [BindableMethod(name = "Toggle Hero Collider", category = "Cheats")]
    public static void ToggleHeroCollider()
    {
        if (!DebugMod.RefHeroCollider.enabled)
        {
            DebugMod.RefHeroCollider.enabled = true;
            DebugMod.RefHeroBox.enabled = true;
            DebugMod.LogConsole("Enabled hero collider" + (DebugMod.noclip ? " and disabled noclip" : ""));
            DebugMod.noclip = false;
            DebugMod.RefKnight.GetComponent<Rigidbody2D>().constraints &= ~RigidbodyConstraints2D.FreezePosition;
        }
        else
        {
            DebugMod.RefHeroCollider.enabled = false;
            DebugMod.RefHeroBox.enabled = false;
            DebugMod.LogConsole("Disabled hero collider" + (DebugMod.noclip ? "" : " and enabled noclip"));
            DebugMod.noclip = true;
            DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
        }
    }
}