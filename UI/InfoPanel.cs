using DebugMod.Helpers;
using DebugMod.UI.Canvas;
using GlobalEnums;
using GlobalSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugMod.UI;

public class InfoPanel : CanvasPanel
{
    internal record InjectedInfo
    {
        public readonly string Label;
        public readonly Func<string> Generator;

        public InjectedInfo(string label, Func<string> generator)
        {
            Label = label;
            Generator = generator;
        }
    }

    public static int ListingHeight => UICommon.ScaleHeight(20);

    public static InfoPanel Instance { get; private set; }

    internal static readonly List<InjectedInfo> LeftColumnInjects = [];
    internal static readonly List<InjectedInfo> RightColumnInjects = [];

    private float x;
    private float y;
    private float labelWidth;
    private float infoWidth;
    private float lineGap;  // Alt implementation
    private int counter;
    private float injectedHeight;

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

        AppendInfo("INFOPANEL_POSITION", GetHeroPos);
        AppendInfo("INFOPANEL_VELOCITY", () => HeroController.instance.current_velocity);
        AppendInfo("INFOPANEL_INPUTS", GetInputs);
        AppendInfo("INFOPANEL_HEROSTATE", () => HeroController.instance.hero_state);
        AppendInfo("INFOPANEL_DAMAGESTATE", () => HeroController.instance.damageMode);

        y += sectionBreak;

        AppendInfo("INFOPANEL_NEEDLEBASE", () => $"{PlayerData.instance.nailDamage} (n{PlayerData.instance.nailUpgrades})");
        AppendInfo("INFOPANEL_LASTDAMAGEAMOUNT", () => DebugMod.lastHit != null ?
            $"{DebugMod.lastDamage} ({DebugMod.lastHit?.DamageDealt} x {DebugMod.lastHit?.Multiplier})" : Utils.Localize("INFOPANEL_NONE"));
        AppendInfo("INFOPANEL_LASTDAMAGETYPE", () => DebugMod.lastHit?.AttackType.ToString() ?? Utils.Localize("INFOPANEL_NONE"));
        AppendInfo("INFOPANEL_LASTDAMAGESCALING", GetScaling);

        y += sectionBreak;

        AppendInfo("INFOPANEL_HEALTH", () => $"{PlayerData.instance.health} / {PlayerData.instance.maxHealth}");
        AppendInfo("INFOPANEL_SILK", () => $"{PlayerData.instance.silk} / {PlayerData.instance.CurrentSilkMaxBasic}");
        AppendInfo("INFOPANEL_COMPLETION", () => $"{PlayerData.instance.completionPercentage}%");
        AppendInfo("INFOPANEL_FLEAS", () => $"{Gameplay.FleasCollectedCount} / 30");
        AppendInfo("INFOPANEL_QUESTPOINTS", GetQuestPoints);

        if (DebugMod.settings.ExpandedInfoPanel)
        {
            y += sectionBreak;

            AppendInfo("INFOPANEL_SCENENAME", DebugMod.GetSceneName);
            AppendInfo("INFOPANEL_TRANSITIONSTATE", GetTransitionStates);
            AppendInfo("INFOPANEL_GAMESTATE", () => GameManager.instance.GameState);
            AppendInfo("INFOPANEL_UISTATE", () => HeroController.instance.ui.uiState);
            AppendInfo("INFOPANEL_TRANSITIONING", () => HeroController.instance.cState.transitioning);
            AppendInfo("INFOPANEL_ISGAMEPLAY", () => HeroController.instance.isGameplayScene);
        }

        AppendInfo(LeftColumnInjects);

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

        AppendInfo("INFOPANEL_ATTACKING", () => HeroController.instance.cState.attacking);
        AppendInfo("INFOPANEL_SPRINTING", GetSprintFlags);
        AppendInfo("INFOPANEL_JUMPING", GetJumpFlags);
        AppendInfo("INFOPANEL_FALLING", () => HeroController.instance.cState.falling);
        AppendInfo("INFOPANEL_HARDLAND", () => HeroController.instance.cState.willHardLand);  // Could combine into above to make room if needed
        AppendInfo("INFOPANEL_SWIMMING", () => HeroController.instance.cState.swimming);
        AppendInfo("INFOPANEL_RECOILING", () => HeroController.instance.cState.recoiling);
        AppendInfo("INFOPANEL_SOARING", () => HeroController.instance.cState.superDashing);

        y += sectionBreak;

        AppendInfo("INFOPANEL_WALLSTATES", GetWallState);
        AppendInfo("INFOPANEL_CANCAST", () => HeroController.instance.CanCast());
        AppendInfo("INFOPANEL_CANSOAR", () => HeroController.instance.CanSuperJump());
        AppendInfo("INFOPANEL_CANQUICKMAP", () => HeroController.instance.CanQuickMap());
        AppendInfo("INFOPANEL_CANINVENTORY", () => HeroController.instance.CanOpenInventory());

        y += sectionBreak;

        if (DebugMod.settings.ExpandedInfoPanel)
        {
            AppendInfo("INFOPANEL_ACCEPTINPUT", () => HeroController.instance.acceptingInput);
            AppendInfo("INFOPANEL_CONTROLRELINQUISHED", () => HeroController.instance.controlReqlinquished);
            AppendInfo("INFOPANEL_HEROPAUSED", () => HeroController.instance.IsPaused());
            AppendInfo("INFOPANEL_ATBENCH", () => PlayerData.instance.atBench);
            AppendInfo("INFOPANEL_INVULNERABLE", () => HeroController.instance.cState.Invulnerable);
            AppendInfo("INFOPANEL_INVINCIBLE", () => PlayerData.instance.isInvincible);

            y += sectionBreak;

            AppendInfo("INFOPANEL_CAMERAMODE", GetCameraModes);
        }
        else
        {
            // Re-add scene name omitted above
            AppendInfo("INFOPANEL_SCENENAME", DebugMod.GetSceneName);
        }

        AppendInfo(RightColumnInjects);

        if (DebugMod.settings.AltInfoPanel)
        {
            CanvasBorder rightLabelColumn = Add(new CanvasBorder("RightLabelColumn"));
            rightLabelColumn.Sides = BorderSides.RIGHT;
            rightLabelColumn.LocalPosition = new Vector2(x - ContentMargin(), 0);
            rightLabelColumn.Size = new Vector2(labelWidth + lineGap + UICommon.BORDER_THICKNESS, y);
            rightLabelColumn.Thickness = UICommon.BORDER_THICKNESS;
            rightLabelColumn.Color = UICommon.iconColor;
        }


        if (Profiler.enabled)
        {
            // Column 3 start (usually empty)
            x += labelWidth + infoWidth;
            y = ContentMargin();

            CanvasText profilerText = Add(new CanvasText("ProfilerInfo"));
            profilerText.LocalPosition = new Vector2(x, y);
            profilerText.Size = new Vector2(labelWidth + infoWidth, Size.y);
            profilerText.OnUpdate += () =>
            {
                string info = Utils.Localize("INFOPANEL_PROFILER") + "\n";

                Dictionary<string, float> times = Profiler.GetTimes();
                foreach (KeyValuePair<string, float> pair in times)
                {
                    info += $"{pair.Key} {pair.Value * 1000f:F2}ms\n";
                }

                profilerText.Text = info;
            };
        }

        // Move panel up so injected info doesn't overflow into console
        LocalPosition = new Vector2(LocalPosition.x, LocalPosition.y - injectedHeight);
    }

    private void AppendInfo(string label, Func<string> info)
    {
        CanvasText labelText = Add(new CanvasText($"Label{counter}"));
        labelText.LocalPosition = new Vector2(x, y);
        labelText.Size = new Vector2(labelWidth, ListingHeight);
        labelText.Alignment = TextAnchor.MiddleLeft;
        labelText.Text = Utils.Localize(label);

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

    private void AppendInfo(IReadOnlyCollection<InjectedInfo> injectedInfo)
    {
        if (injectedInfo.Count == 0) return;

        float origY = y;

        y += UICommon.ScaleHeight(20);  // sectionBreak
        foreach (var info in injectedInfo.OrderBy(i => i.Label))
        {
            AppendInfo(info.Label, info.Generator);
        }

        injectedHeight = Mathf.Max(injectedHeight, y - origY);
    }

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
        return HeroController.instance.cState.dashing ? "✓" : "X";
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
        + (HeroController.instance.cState.touchingWall ? "T" : "‒") // Touch
        + (HeroController.instance.wallLocked ? "L" : "‒") // Lock
        + (HeroController.instance.cState.wallSliding ? "S" : "‒") // Slide
        + (HeroController.instance.queuedWallJumpInterrupt ? "Q" : "‒") // Queued
        + (HeroController.instance.touchingWallR ? ">" : "‒"); // Right

    private static string GetScaling()
    {
        if (DebugMod.lastScaling == null) return Utils.Localize("INFOPANEL_NONE");

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
            HeroTransitionState.WAITING_TO_TRANSITION => string.Format(
                Utils.Localize("INFOPANEL_TRANSITIONSTATE_DONEFORMAT"), DebugMod.GetLoadTime()),
            HeroTransitionState.EXITING_SCENE => Utils.Localize("INFOPANEL_TRANSITIONSTATE_EXITING"),
            HeroTransitionState.WAITING_TO_ENTER_LEVEL => Utils.Localize("INFOPANEL_TRANSITIONSTATE_WAITINGTOENTER"),
            HeroTransitionState.ENTERING_SCENE => Utils.Localize("INFOPANEL_TRANSITIONSTATE_ENTERING"),
            HeroTransitionState.DROPPING_DOWN => Utils.Localize("INFOPANEL_TRANSITIONSTATE_DROPPINGDOWN"),
            _ => Utils.Localize("INFOPANEL_UNKNOWN")
        };
    }

    private static string GetCameraModes()
    {
        return DebugMod.RefCamera.mode switch
        {
            CameraController.CameraMode.FROZEN => Utils.Localize("INFOPANEL_CAMERAMODE_FROZEN"),
            CameraController.CameraMode.FOLLOWING => Utils.Localize("INFOPANEL_CAMERAMODE_FOLLOWING"),
            CameraController.CameraMode.LOCKED => Utils.Localize("INFOPANEL_CAMERAMODE_LOCKED"),
            CameraController.CameraMode.PANNING => Utils.Localize("INFOPANEL_CAMERAMODE_PANNING"),
            CameraController.CameraMode.FADEOUT => Utils.Localize("INFOPANEL_CAMERAMODE_FADEOUT"),
            CameraController.CameraMode.FADEIN => Utils.Localize("INFOPANEL_CAMERAMODE_FADEIN"),
            CameraController.CameraMode.PREVIOUS => Utils.Localize("INFOPANEL_CAMERAMODE_PREVIOUS"),
            _ => Utils.Localize("INFOPANEL_UNKNOWN")
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
