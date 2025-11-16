using System;
using DebugMod.UI.Canvas;
using GlobalSettings;
using UnityEngine;

namespace DebugMod.UI;

public class InfoPanel : CanvasPanel
{
    public static int ListingHeight => UICommon.ScaleHeight(20);

    public static InfoPanel Instance { get; private set; }

    private float x;
    private float y;
    private float labelWidth;
    private float infoWidth;
    private int counter;

    public static void BuildPanel()
    {
        Instance = new InfoPanel();
        Instance.Build();
    }

    public InfoPanel() : base(nameof(InfoPanel))
    {
        LocalPosition = new Vector2(UICommon.ScreenMargin, Screen.height - UICommon.ConsoleHeight - UICommon.InfoPanelHeight - UICommon.ScreenMargin * 2);
        Size = new Vector2(UICommon.LeftSideWidth, UICommon.InfoPanelHeight);

        x = UICommon.Margin;
        y = UICommon.Margin;
        labelWidth = 140f;
        infoWidth = 150f;

        AppendInfo("Scene Name", DebugMod.GetSceneName);
        AppendInfo("Position", GetHeroPos);
        AppendInfo("Velocity", () => HeroController.instance.current_velocity);

        y += 15f;

        AppendInfo("Move Vector", () => DebugMod.IH.inputActions.MoveVector.Vector);
        AppendInfo("Hero State", () => HeroController.instance.hero_state);
        AppendInfo("Damage State", () => HeroController.instance.damageMode);

        y += 15f;

        AppendInfo("Health", () => $"{PlayerData.instance.health}/{PlayerData.instance.maxHealth}");
        AppendInfo("Silk", () => $"{PlayerData.instance.silk}/{PlayerData.instance.CurrentSilkMaxBasic}");

        y += 15f;

        AppendInfo("Needle Base", () => PlayerData.instance.nailDamage);
        AppendInfo("Last Damage", () => DebugMod.lastHit != null ?
            $"{DebugMod.lastDamage} ({DebugMod.lastHit?.DamageDealt} x {DebugMod.lastHit?.Multiplier})" : "None");
        AppendInfo("Last Type", () => DebugMod.lastHit?.AttackType.ToString() ?? "None");
        AppendInfo("Last Scaling", () => DebugMod.lastHit != null ? DebugMod.lastScaling.ToString() : "None");

        y += 15f;

        AppendInfo("Completion", () => $"{PlayerData.instance.completionPercentage}%");
        AppendInfo("Fleas", () => $"{Gameplay.FleasCollectedCount} / 30");
        AppendInfo("Quest Points", GetQuestPoints);

        x += labelWidth + infoWidth;
        y = UICommon.Margin;

        AppendInfo("Dashing", () => HeroController.instance.cState.dashing);
        AppendInfo("Sprinting", () => HeroController.instance.cState.isSprinting);
        AppendInfo("Jumping", () => HeroController.instance.cState.jumping || HeroController.instance.cState.doubleJumping);
        AppendInfo("Super Jumping", () => HeroController.instance.cState.superDashing);
        AppendInfo("Falling", () => HeroController.instance.cState.falling);
        AppendInfo("Hardland", () => HeroController.instance.cState.willHardLand);
        AppendInfo("Swimming", () => HeroController.instance.cState.swimming);
        AppendInfo("Recoiling", () => HeroController.instance.cState.recoiling);

        y += 30f;

        AppendInfo("Invulnerable", () => HeroController.instance.cState.Invulnerable);
        AppendInfo("Invincible", () => PlayerData.instance.isInvincible);

        y += 30f;

        AppendInfo("Attacking", () => HeroController.instance.cState.attacking);
        AppendInfo("Can Cast", () => HeroController.instance.CanCast());
        AppendInfo("Can Super Jump", () => HeroController.instance.CanSuperJump());
        AppendInfo("Can Quickmap", () => HeroController.instance.CanQuickMap());
        AppendInfo("Can Inventory", () => HeroController.instance.CanOpenInventory());
    }

    private void AppendInfo(string label, Func<string> info)
    {
        CanvasText labelText = Add(new CanvasText($"Label{counter}"));
        labelText.LocalPosition = new Vector2(x, y);
        labelText.Size = new Vector2(labelWidth, ListingHeight);
        labelText.Alignment = TextAnchor.MiddleLeft;
        labelText.Text = label;

        CanvasText infoText = Add(new CanvasText($"Info{counter}"));
        infoText.LocalPosition = new Vector2(x + labelWidth, y);
        infoText.Size = new Vector2(infoWidth, ListingHeight);
        infoText.Font = UICommon.trajanNormal;
        infoText.Alignment = TextAnchor.MiddleLeft;
        infoText.OnUpdate += () => infoText.Text = info();

        counter++;
        y += ListingHeight;
    }

    private void AppendInfo(string label, Func<bool> info) => AppendInfo(label, () => GetStringForBool(info()));
    private void AppendInfo<T>(string label, Func<T> info) => AppendInfo(label, () => info().ToString());

    private static string GetHeroPos()
    {
        if (DebugMod.RefKnight == null)
        {
            return string.Empty;
        }

        float heroX = DebugMod.RefKnight.transform.position.x;
        float heroY = DebugMod.RefKnight.transform.position.y;

        return $"({heroX}, {heroY})";
    }

    private static string GetQuestPoints()
    {
        QuestCompleteTotalGroup group = QuestManager.GetQuest("Soul Snare Pre").requiredCompleteTotalGroups[0];
        return $"{group.CurrentValueCount}/{group.target}";
    }

    private static string GetStringForBool(bool b)
    {
        return b ? "✓" : "X";
    }
}
