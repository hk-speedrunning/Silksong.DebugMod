using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Kill All", category = "Cheats")]
    public static void KillAll()
    {
        int ctr = 0;

        foreach (HealthManager hm in Object.FindObjectsOfType<HealthManager>())
        {
            if (!hm.isDead)
            {
                hm.Die(null, AttackTypes.Generic, true);
                ctr++;
            }
        }
        Console.AddLine($"Killing {ctr} HealthManagers in scene!");
    }

    [BindableMethod(name = "Infinite Jump", category = "Cheats")]
    public static void ToggleInfiniteJump()
    {
        PlayerData.instance.infiniteAirJump = !PlayerData.instance.infiniteAirJump;
        Console.AddLine("Infinite Jump set to " + PlayerData.instance.infiniteAirJump.ToString().ToUpper());
    }

    [BindableMethod(name = "Infinite Silk", category = "Cheats")]
    public static void ToggleInfiniteSilk()
    {
        DebugMod.infiniteSilk = !DebugMod.infiniteSilk;
        Console.AddLine("Infinite Silk set to " + DebugMod.infiniteSilk.ToString().ToUpper());
    }

    [BindableMethod(name = "Infinite HP", category = "Cheats")]
    public static void ToggleInfiniteHP()
    {
        DebugMod.infiniteHP = !DebugMod.infiniteHP;
        Console.AddLine("Infinite HP set to " + DebugMod.infiniteHP.ToString().ToUpper());
    }

    [BindableMethod(name = "Invincibility", category = "Cheats")]
    public static void ToggleInvincibility()
    {
        DebugMod.playerInvincible = !DebugMod.playerInvincible;
        Console.AddLine("Invincibility set to " + DebugMod.playerInvincible.ToString().ToUpper());

        PlayerData.instance.isInvincible = DebugMod.playerInvincible;
    }

    [BindableMethod(name = "Noclip", category = "Cheats")]
    public static void ToggleNoclip()
    {
        DebugMod.noclip = !DebugMod.noclip;

        if (DebugMod.noclip)
        {
            Console.AddLine("Enabled noclip");
            DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
        }
        else
        {
            Console.AddLine("Disabled noclip");
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
            Console.AddLine("Enabled hero collider" + (DebugMod.noclip ? " and disabled noclip" : ""));
            DebugMod.noclip = false;
        }
        else
        {
            DebugMod.RefHeroCollider.enabled = false;
            DebugMod.RefHeroBox.enabled = false;
            Console.AddLine("Disabled hero collider" + (DebugMod.noclip ? "" : " and enabled noclip"));
            DebugMod.noclip = true;
            DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
        }
    }

    [BindableMethod(name = "Kill Self", category = "Cheats")]
    public static void KillSelf()
    {
        if (!HeroController.instance.cState.dead && !HeroController.instance.cState.transitioning)
        {
            HeroController.instance.StartCoroutine(HeroController.instance.Die(false, false));
            Console.AddLine("Killed player");
        }
    }
}