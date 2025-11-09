using System;
using System.Collections.Generic;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using GlobalSettings;
using UnityEngine;

namespace DebugMod.UI;

public class CustomInfoPanel : InfoPanel
{
    protected List<(float xLabel, float xInfo, float y, string label, Func<string> textFunc)> PanelBuildInfo = new();
    private Dictionary<string, Func<string>> UpdateActions;

    public CustomInfoPanel(string name, CanvasNode parent, Vector2 position, Vector2 size)
        : base(name, parent, position, size) {}

    public override void Build()
    {
        UpdateActions = new();
        int counter = 0;

        foreach ((float xLabel, float xInfo, float y, string label, Func<string> textFunc) in PanelBuildInfo)
        {
            AddText($"Label-{counter++}", label, new Vector2(xLabel, y), Vector2.zero, UICommon.arial, 15);
            AddText($"Info-{counter}", "", new Vector2(xInfo, y + 4f), Vector2.zero, UICommon.trajanNormal);
            UpdateActions.Add($"Info-{counter}", textFunc);
        }

        base.Build();
    }

    public override void Update()
    {
        base.Update();

        if (ActiveInHierarchy)
        {
            foreach (var kvp in UpdateActions)
            {
                GetText(kvp.Key).Text = kvp.Value.Invoke();
            }
        }
    }

    public void AddInfo(float xLabel, float xInfo, float y, string label, Func<string> textFunc)
        => PanelBuildInfo.Add((xLabel, xInfo, y, label, textFunc));

    internal static CustomInfoPanel BuildMainInfoPanel()
    {
        CustomInfoPanel panel = new CustomInfoPanel(MainInfoPanelName, null, new Vector2(0f, 223f), Vector2.zero);
        panel.AddImage("Background", UICommon.images["StatusPanelBG"], Vector2.zero);

        float y = 0f;

        panel.AddInfo(10f, 150f, y += 20, "Hero State", () => HeroController.instance.hero_state.ToString());
        panel.AddInfo(10f, 150f, y += 20, "Velocity", () => HeroController.instance.current_velocity.ToString());
        panel.AddInfo(10f, 150f, y += 20, "HP", () => PlayerData.instance.health + " / " + PlayerData.instance.maxHealth);
        panel.AddInfo(10f, 150f, y += 20, "MP", () => PlayerData.instance.silk + " / " + PlayerData.instance.CurrentSilkMaxBasic);
        y += 25f;
        panel.AddInfo(10f, 150f, y += 20, "Completion", () => PlayerData.instance.completionPercentage + "%");
        panel.AddInfo(10f, 150f, y += 20, "Fleas", () => Gameplay.FleasCollectedCount + " / 30");
        panel.AddInfo(10, 150f, y += 20, "Quest Points", () => QuestManager.GetQuest("Soul Snare Pre").requiredCompleteTotalGroups[0].CurrentValueCount
            + " / " + QuestManager.GetQuest("Soul Snare Pre").requiredCompleteTotalGroups[0].target);
        y += 25f;
        panel.AddInfo(10f, 150f, y += 20, "Needle Base", () => PlayerData.instance.nailDamage.ToString());
        panel.AddInfo(10f, 150f, y += 20, "Last Damage", () => DebugMod.lastHit != null ?
            $"{DebugMod.lastDamage} ({DebugMod.lastHit?.DamageDealt} x {DebugMod.lastHit?.Multiplier})" : "None");
        panel.AddInfo(10f, 150f, y += 20, "Last Type", () => DebugMod.lastHit?.AttackType.ToString() ?? "None");
        panel.AddInfo(10f, 150f, y += 20, "Last Scaling", () => DebugMod.lastHit != null ? DebugMod.lastScaling.ToString() : "None");
        y += 25f;
        panel.AddInfo(10f, 150f, y += 20, "Invulnerable", () => GetStringForBool(HeroController.instance.cState.Invulnerable));
        panel.AddInfo(10f, 150f, y += 20, "Invincible", () => GetStringForBool(PlayerData.instance.isInvincible));
        panel.AddInfo(10f, 150f, y += 20, "Damage State", () => HeroController.instance.damageMode.ToString());
        panel.AddInfo(10f, 150f, y += 20, "Dead State", () => GetStringForBool(HeroController.instance.cState.dead));
        panel.AddInfo(10f, 150f, y += 20, "Hazard Death", () => GetStringForBool(HeroController.instance.cState.hazardDeath));
        panel.AddInfo(10f, 150f, y += 20, "Cocoon Scene", () => string.IsNullOrEmpty(PlayerData.instance.HeroCorpseScene) ? "None" : PlayerData.instance.HeroCorpseScene);
        panel.AddInfo(10f, 150f, y += 20, "Cocoon Position", () => string.IsNullOrEmpty(PlayerData.instance.HeroCorpseScene) ? "None" : PlayerData.instance.HeroDeathScenePos.ToString());
        y += 25f;
        panel.AddInfo(10f, 150f, y += 20, "Scene Name", () => DebugMod.GetSceneName());
        panel.AddInfo(10f, 150f, y += 20, "Transition", () => GetStringForBool(HeroController.instance.cState.transitioning));
        panel.AddInfo(10f, 150f, y += 20, "Trans State", () => GetTransState());
        panel.AddInfo(10f, 150f, y += 20, "Game State", () => GameManager.instance.GameState.ToString());
        panel.AddInfo(10f, 150f, y += 20, "UI State", () => UIManager.instance.uiState.ToString());
        panel.AddInfo(10f, 150f, y += 20, "Camera Mode", () => DebugMod.RefCamera.mode.ToString());

        y = 10f;

        panel.AddInfo(300f, 440f, y += 20, "Accept Input", () => GetStringForBool(HeroController.instance.acceptingInput));
        panel.AddInfo(300f, 440f, y += 20, "Relinquished", () => GetStringForBool(HeroController.instance.controlReqlinquished));
        panel.AddInfo(300f, 440f, y += 20, "At Bench", () => GetStringForBool(PlayerData.instance.atBench));
        y += 20f;
        panel.AddInfo(300f, 440f, y += 20, "Dashing", () => GetStringForBool(HeroController.instance.cState.dashing));
        panel.AddInfo(300f, 440f, y += 20, "Sprinting", () => GetStringForBool(HeroController.instance.cState.isSprinting));
        panel.AddInfo(300f, 440f, y += 20, "Jumping", () => GetStringForBool(HeroController.instance.cState.jumping || HeroController.instance.cState.doubleJumping));
        panel.AddInfo(300f, 440f, y += 20, "Super Jumping", () => GetStringForBool(HeroController.instance.cState.superDashing));
        panel.AddInfo(300f, 440f, y += 20, "Falling", () => GetStringForBool(HeroController.instance.cState.falling));
        panel.AddInfo(300f, 440f, y += 20, "Hardland", () => GetStringForBool(HeroController.instance.cState.willHardLand));
        panel.AddInfo(300f, 440f, y += 20, "Swimming", () => GetStringForBool(HeroController.instance.cState.swimming));
        panel.AddInfo(300f, 440f, y += 20, "Recoiling", () => GetStringForBool(HeroController.instance.cState.recoiling));
        y += 20f;
        panel.AddInfo(300f, 440f, y += 20, "Wall Lock", () => GetStringForBool(HeroController.instance.wallLocked));
        panel.AddInfo(300f, 440f, y += 20, "Wall Jumping", () => GetStringForBool(HeroController.instance.cState.wallJumping));
        panel.AddInfo(300f, 440f, y += 20, "Wall Touching", () => GetStringForBool(HeroController.instance.cState.touchingWall));
        panel.AddInfo(300f, 440f, y += 20, "Wall Sliding", () => GetStringForBool(HeroController.instance.cState.wallSliding));
        panel.AddInfo(300f, 440f, y += 20, "Wall Left", () => GetStringForBool(HeroController.instance.touchingWallL));
        panel.AddInfo(300f, 440f, y += 20, "Wall Right", () => GetStringForBool(HeroController.instance.touchingWallR));
        y += 20f;
        panel.AddInfo(300f, 440f, y += 20, "Attacking", () => GetStringForBool(HeroController.instance.cState.attacking));
        panel.AddInfo(300f, 440f, y += 20, "Can Cast", () => GetStringForBool(HeroController.instance.CanCast()));
        panel.AddInfo(300f, 440f, y += 20, "Can Superjump", () => GetStringForBool(HeroController.instance.CanSuperJump()));
        panel.AddInfo(300f, 440f, y += 20, "Can Quickmap", () => GetStringForBool(HeroController.instance.CanQuickMap()));
        panel.AddInfo(300f, 440f, y += 20, "Can Inventory", () => GetStringForBool(HeroController.instance.CanOpenInventory()));
        y += 20f;
        panel.AddInfo(300f, 440f, y += 20, "Is Gameplay", () => GetStringForBool(DebugMod.GM.IsGameplayScene()));
        panel.AddInfo(300f, 440f, y += 20, "Hero Paused", () => GetStringForBool(HeroController.instance.cState.isPaused));

        return panel;
    }

    internal static CustomInfoPanel BuildMinimalInfoPanel()
    {
        CustomInfoPanel panel = new CustomInfoPanel(MinimalInfoPanelName, null, new Vector2(130f, 230f), Vector2.zero);

        panel.AddInfo(10, 40, 10, "Vel", () => HeroController.instance.current_velocity.ToString());
        panel.AddInfo(110, 140, 10, "Pos", () => GetHeroPos());
        
        panel.AddInfo(10, 40, 30, "MP", () => PlayerData.instance.silk.ToString());
        
        panel.AddInfo(10, 100, 50, "NailDmg", () => PlayerData.instance.nailDamage.ToString());
        
        panel.AddInfo(10, 95, 70, "Completion", () => PlayerData.instance.completionPercentage + "%");
        panel.AddInfo(140, 195, 70, "Fleas", () => Gameplay.FleasCollectedCount + " / 30");
        
        panel.AddInfo(10, 140, 90, "Scene Name", () => DebugMod.GetSceneName());
        panel.AddInfo(10, 140, 110, "Current Savestate", () => SaveStateManager.quickState.IsSet() ? SaveStateManager.quickState.GetSaveStateID() : "No savestate");
        panel.AddInfo(110, 200, 130, "Current slot", GetCurrentSlotString);
        panel.AddInfo(10, 80, 130, "Hardfall", () => GetStringForBool(HeroController.instance.cState.willHardLand));

        return panel;
    }

    private static string GetCurrentSlotString()
    {
        string slotSet = SaveStateManager.GetCurrentSlot().ToString();
        if (slotSet == "-1")
        {
            slotSet = "unset";
        }
        return slotSet;
    }
}
