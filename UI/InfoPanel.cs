using DebugMod.UI.Canvas;
using GlobalEnums;
using GlobalSettings;
using System;
using System.Collections.Generic;
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
    private float lineGap;  // Alt implementation
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

        x = ContentMargin();
        y = ContentMargin();
        labelWidth = UICommon.ScaleWidth(120);
        infoWidth = UICommon.ScaleWidth(160);
        
        // Alt implementation:
        if (DebugMod.settings.AltInfoPanel)
        {
            labelWidth = UICommon.ScaleWidth(100);
            infoWidth = UICommon.ScaleWidth(140);
            lineGap = UICommon.ScaleWidth(10);
        }
        
        int sectionBreak = UICommon.ScaleHeight(20);
        
        AppendInfo("Position", GetHeroPos);
        AppendInfo("Velocity", () => HeroController.instance.current_velocity);
        AppendInfo("Inputs", GetInputs);
        AppendInfo("Hero State", () => HeroController.instance.hero_state);
        AppendInfo("Damage State", () => HeroController.instance.damageMode);

        y += sectionBreak;

        AppendInfo("Needle Base", () => $"{PlayerData.instance.nailDamage} (n{PlayerData.instance.nailUpgrades})");
        AppendInfo("Last Damage", () => DebugMod.lastHit != null ?
            $"{DebugMod.lastDamage} ({DebugMod.lastHit?.DamageDealt} x {DebugMod.lastHit?.Multiplier})" : "None");
        AppendInfo("Last Type", () => DebugMod.lastHit?.AttackType.ToString() ?? "None");
        AppendInfo("Last Scaling", GetScaling);

        y += sectionBreak;
        
        AppendInfo("Health", () => $"{PlayerData.instance.health} / {PlayerData.instance.maxHealth}");
        AppendInfo("Silk", () => $"{PlayerData.instance.silk} / {PlayerData.instance.CurrentSilkMaxBasic}");
        AppendInfo("Completion", () => $"{PlayerData.instance.completionPercentage}%");
        AppendInfo("Fleas", () => $"{Gameplay.FleasCollectedCount} / 30");
        AppendInfo("Quest Points", GetQuestPoints);

        if (DebugMod.settings.ExpandedInfoPanel) {
            y += sectionBreak;

            AppendInfo("Scene Name", DebugMod.GetSceneName);
            AppendInfo("Trans State", GetTransitionStates);
            AppendInfo("Game State", () => GameManager.instance.GameState);
            AppendInfo("UI State", () => HeroController.instance.ui.uiState);
            AppendInfo("Transition", () => HeroController.instance.cState.transitioning);
            AppendInfo("Is Gameplay", () => HeroController.instance.isGameplayScene);
        }

        if (DebugMod.settings.AltInfoPanel)
        {
            CanvasBorder leftLabelColumn = Add(new CanvasBorder("LeftLabelColumn"));
            leftLabelColumn.Sides = BorderSides.RIGHT;
            leftLabelColumn.LocalPosition = new Vector2(x - ContentMargin(), 0);
            leftLabelColumn.Size = new Vector2(x + labelWidth + lineGap + UICommon.BORDER_THICKNESS, y);
            leftLabelColumn.Thickness = UICommon.BORDER_THICKNESS;
            leftLabelColumn.Color = UICommon.iconColor;
        }
        
        // Column 2 Start
        x += labelWidth + infoWidth;
        y = ContentMargin();
        
        AppendInfo("Attacking", () => HeroController.instance.cState.attacking);
        AppendInfo("Sprinting", GetSprintFlags);
        AppendInfo("Jumping", GetJumpFlags);
        AppendInfo("Falling", () => HeroController.instance.cState.falling);
        AppendInfo("Hardland", () => HeroController.instance.cState.willHardLand);  // Could combine into above to make room if needed
        AppendInfo("Swimming", () => HeroController.instance.cState.swimming);
        AppendInfo("Recoiling", () => HeroController.instance.cState.recoiling);
        AppendInfo("Soaring", () => HeroController.instance.cState.superDashing);
        
        y += sectionBreak;
        
        AppendInfo("Wall States", GetWallState);
        AppendInfo("Can Cast", () => HeroController.instance.CanCast());
        AppendInfo("Can Soar", () => HeroController.instance.CanSuperJump());
        AppendInfo("Can Quickmap", () => HeroController.instance.CanQuickMap());
        AppendInfo("Can Inventory", () => HeroController.instance.CanOpenInventory());

        y += sectionBreak;

        if (DebugMod.settings.ExpandedInfoPanel)
        {
            AppendInfo("Accept Input", () => HeroController.instance.acceptingInput);
            AppendInfo("Relinquished", () => HeroController.instance.controlReqlinquished);
            AppendInfo("Hero Paused", () => HeroController.instance.IsPaused());
            AppendInfo("At Bench", () => PlayerData.instance.atBench);
            AppendInfo("Invulnerable", () => HeroController.instance.cState.Invulnerable);
            AppendInfo("Invincible", () => PlayerData.instance.isInvincible);
        
            y += sectionBreak;
        
            AppendInfo("Camera Mode", GetCameraModes);
        }
        else
        {
            // Re-add scene name omitted above
            AppendInfo("Scene Name", DebugMod.GetSceneName);
        }
        

        if (DebugMod.settings.AltInfoPanel)
        {
            CanvasBorder rightLabelColumn = Add(new CanvasBorder("RightLabelColumn"));
            rightLabelColumn.Sides = BorderSides.RIGHT;
            rightLabelColumn.LocalPosition = new Vector2(x - ContentMargin(), 0);
            rightLabelColumn.Size = new Vector2(labelWidth + lineGap + UICommon.BORDER_THICKNESS, y);
            rightLabelColumn.Thickness = UICommon.BORDER_THICKNESS;
            rightLabelColumn.Color = UICommon.iconColor;
        }
    }

    private void AppendInfo(string label, Func<string> info)
    {
        CanvasText labelText = Add(new CanvasText($"Label{counter}"));
        labelText.LocalPosition = new Vector2(x, y);
        labelText.Size = new Vector2(labelWidth, ListingHeight);
        labelText.Alignment = TextAnchor.MiddleLeft;
        labelText.Text = label;

        // Info text is offset slightly downward so the different fonts (approximately) line up
        CanvasText infoText = Add(new CanvasText($"Info{counter}", overflow: HorizontalWrapMode.Overflow));
        infoText.LocalPosition = new Vector2(x + labelWidth, y + ListingHeight / 10f);
        infoText.Size = new Vector2(infoWidth, ListingHeight);
        infoText.Font = UICommon.trajanBold;
        infoText.Alignment = TextAnchor.MiddleLeft;
        infoText.OnUpdate += () => infoText.Text = info();
        
        // Alt implementation:
        if (DebugMod.settings.AltInfoPanel)
        {
            labelText.Alignment = TextAnchor.MiddleRight;
            infoText.LocalPosition = new Vector2(x + labelWidth + 2 * lineGap, y + ListingHeight / 10f);
        }


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

        return $"{heroX:.000000#}, {heroY:.000000#}";
    }

    private static string GetQuestPoints()
    {
        QuestCompleteTotalGroup group = QuestManager.GetQuest("Soul Snare Pre").requiredCompleteTotalGroups[0];
        return $"{group.CurrentValueCount} / {group.target}";
    }

    private static string GetStringForBool(bool b)
    {
        return b ? "✓" : "X";
    }

    private static string GetSprintFlags()
    {
        if (HeroController.instance.cState.isSprinting)
        {
            // isSprinting && dashing shouldn't be possible, but we'll add an indicator for it regardless.
            return HeroController.instance.cState.dashing ? "✓✓" : "✓–";
        }
        return HeroController.instance.cState.dashing ? "✓": "X";
    }

    private static string GetJumpFlags()
    {
        List<string> jumpStates = [];
        if (HeroController.instance.cState.jumping) jumpStates.Add("✓");
        if (HeroController.instance.cState.doubleJumping) jumpStates.Add("◊");
        if (HeroController.instance.isUmbrellaActive.Value) jumpStates.Add("†");
        
        return jumpStates.Count == 0 ? "X" : string.Join("", jumpStates);
    }

    private static string GetWallState() =>
          (HeroController.instance.touchingWallL ? "<" : "‒")  // Left
        + (HeroController.instance.cState.touchingWall ? "T" : "‒") // Touch TODO: necessary with L/R indicators?
        + (HeroController.instance.wallLocked ? "L" : "‒") // Lock TODO: look into all relevant walljump flags
        + (HeroController.instance.cState.wallSliding ? "S" : "‒") // Slide
        + (HeroController.instance.touchingWallR ? ">" : "‒"); // Right

    private static string GetScaling()
    {
        if (DebugMod.lastScaling == null) return "None";
        
        string[] scaleMultipliers =
        [
            $"{DebugMod.lastScaling.Level1Mult:.###}",
            $"{DebugMod.lastScaling.Level2Mult:.###}",
            $"{DebugMod.lastScaling.Level3Mult:.###}",
            $"{DebugMod.lastScaling.Level4Mult:.###}",
            $"{DebugMod.lastScaling.Level5Mult:.###}"
        ];
        
        // Level from HealthManager.ApplyDamageScaling
        int level = DebugMod.lastScaleLevel;
        
        if (level > 4) level = 4; 
        if (level >= 0) // Game behaviour; sub-zero levels scale to 1f so don't highlight any multiplier
            scaleMultipliers[level] = $"({scaleMultipliers[level]})";
        
        return string.Join("∶", scaleMultipliers);
    }

    private static string GetTransitionStates()
    {
        return HeroController.instance.transitionState switch
        {
            HeroTransitionState.WAITING_TO_TRANSITION => $"Done ({DebugMod.GetLoadTime()}s)",
            HeroTransitionState.EXITING_SCENE => "Exiting",
            HeroTransitionState.WAITING_TO_ENTER_LEVEL => "Wait Enter",
            HeroTransitionState.ENTERING_SCENE => "Entering",
            HeroTransitionState.DROPPING_DOWN => "Dropping down",
            _ => "UNKNOWN"
        };
    }

    private static string GetCameraModes()
    {
        return DebugMod.RefCamera.mode switch
        {
            CameraController.CameraMode.FROZEN => "Frz",
            CameraController.CameraMode.FOLLOWING => "Fol",
            CameraController.CameraMode.LOCKED => "Lck",
            CameraController.CameraMode.PANNING => "Pan",
            CameraController.CameraMode.FADEOUT => "Out",
            CameraController.CameraMode.FADEIN => "In",
            CameraController.CameraMode.PREVIOUS => "Prv",
            _ => "UNKNOWN"
        };
    }

    private static string GetInputs()
    {
        // NB: somewhat random spacing to try and work around lack of monospace font availability
        string l = DebugMod.IH.inputActions.Left.State ? "<" : "  ";            // left
        string u = DebugMod.IH.inputActions.Up.State ? "⋀" : "  ";              // up
        string d = DebugMod.IH.inputActions.Down.State ? "⋁" : "  ";            // down
        string r = DebugMod.IH.inputActions.Right.State ? ">" : "  ";           // right
        string j = DebugMod.IH.inputActions.Jump.State ? "j" : " ";             // jump
        string s = DebugMod.IH.inputActions.Dash.State ? "s" : "  ";            // sprint
        string a = DebugMod.IH.inputActions.Attack.State ? "a" : "  ";          // attack
        string h = DebugMod.IH.inputActions.SuperDash.State ? "h" : "   ";      // harpoon
        string n = DebugMod.IH.inputActions.DreamNail.State ? "n" : "   ";      // needolin
        string c = DebugMod.IH.inputActions.QuickCast.State ? "c" : "  ";       // cast
        string b = DebugMod.IH.inputActions.Cast.State ? "b" : "  ";            // bind
        string t = DebugMod.IH.inputActions.Taunt.State ? "t" : "  ";           // taunt
        string m = DebugMod.IH.inputActions.QuickMap.State ? "m" : "   ";       // map
        string i = DebugMod.IH.inputActions.OpenInventory.State ? "i" : "  ";   // inventory
        string p = DebugMod.IH.inputActions.Pause.State ? "p" : "  ";           // pause
        

        return $"{l}{u}{d}{r} {j}{s}{a}{h} {n}{c}{b}{t} {m}{i}{p}";
    }
}
