using DebugMod.Helpers;
using DebugMod.UI;
using DebugMod.UI.Canvas;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bounds = UnityEngine.Bounds;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod.MonoBehaviours;
#nullable enable

[HarmonyPatch]
public class EnemyHandle : MonoBehaviour
{
    private static int BarWidth => UICommon.ScaleWidth(150);
    private static int BarHeight => UICommon.ScaleHeight(32);
    private static int BossBarWidth => UICommon.ScaleWidth(900);
    private static int BossNameHeight => UICommon.ScaleHeight(30);
    private static int BossNameTextSize => UICommon.ScaleHeight(18);
    private static int BossNameOffset => BossNameHeight + 2;

    private static List<EnemyHandle> bosses = [];

    private HealthManager hm;
    private tk2dSprite? sprite;
    private BoxCollider2D collider;
    private CanvasPanel? hpBar;
    private Texture2D? barTexture;
    private Texture2D? yellowBarTexture;
    private int maxHP;
    private bool maxHPSet;
    private bool isBoss;

    private int lastHealth;
    private int damageCounter;
    private int decayingDamageCounter;
    private float lastDamageTime;

    private PlayMakerFSM? staggerFsm;

    public int HP
    {
        get => hm.hp;
        set => hm.hp = value;
    }

    public int MaxHP => maxHP;
    public string Name => gameObject.name;

    public void Awake()
    {
        hm = GetComponent<HealthManager>();
        sprite = GetComponent<tk2dSprite>();
        collider = GetComponent<BoxCollider2D>();

        staggerFsm = gameObject.GetTemplatedFsm("stun_control");

        if (!EnemiesPanel.enemyPool.Contains(this))
        {
            EnemiesPanel.enemyPool.Add(this);
        }
    }

    public void OnDestroy()
    {
        EnemiesPanel.enemyPool.Remove(this);
        DestroyUI();
    }

    public void OnEnable() => Awake();
    public void OnDisable() => OnDestroy();

    public void DestroyUI()
    {
        hpBar?.Destroy();
        hpBar = null;
        bosses.Remove(this);
    }

    public void Update()
    {
        if (!EnemiesPanel.ActivelyUpdating())
        {
            hpBar?.ActiveSelf = false;
            return;
        }

        if (hm.hp < 0 || hm.hp > 9999)
        {
            maxHPSet = false;
            if (hm.hp >= 0)
            {
                maxHP = hm.hp;
            }
        }
        // The third check is to differentiate FSMs that temporarily set
        // the boss HP to 9999 from the debug mod "infinite health" button
        else if (!maxHPSet || maxHP < hm.hp || hm.hp < 5000 && maxHP >= 9000)
        {
            maxHP = hm.hp;
            maxHPSet = true;
        }

        if (EnemiesPanel.hpBars)
        {
            if (hpBar == null)
            {
                string normalizedName = gameObject.name.Replace("(Clone)", "");
                string bossDisplayName = "";

                foreach ((string displayName, string sceneName, string goName) in bossPatterns)
                {
                    if (normalizedName == goName && USceneManager.GetSceneByName(sceneName).isLoaded)
                    {
                        isBoss = !DebugMod.settings.DisableBossHpBars;
                        bossDisplayName = displayName;
                        break;
                    }
                }

                barTexture = new Texture2D(1, 1);
                barTexture.SetPixel(0, 0, Color.red.SetAlpha(0.5f));
                barTexture.Apply();

                hpBar = new CanvasPanel($"{gameObject.name} HP Bar");
                hpBar.Size = new Vector2(isBoss ? BossBarWidth : BarWidth, BarHeight);

                CanvasImage bar = hpBar.Add(new CanvasImage("Bar"));
                bar.SetImage(barTexture);

                if (isBoss)
                {
                    CanvasText bossName = hpBar.Add(new CanvasText("BossName"));
                    bossName.LocalPosition = new Vector2(0, -BossNameOffset);
                    bossName.Size = new Vector2(hpBar.Size.x, BossNameHeight);
                    bossName.FontSize = BossNameTextSize;
                    bossName.Alignment = TextAnchor.LowerCenter;
                    bossName.Text = bossDisplayName;
                    bossName.DropShadow = true;

                    CanvasText damageCounterText = hpBar.Add(new CanvasText("DamageCounter"));
                    damageCounterText.LocalPosition = new Vector2(0, -BossNameOffset);
                    damageCounterText.Size = new Vector2(hpBar.Size.x, BossNameHeight);
                    damageCounterText.FontSize = BossNameTextSize;
                    damageCounterText.Alignment = TextAnchor.LowerRight;
                    damageCounterText.DropShadow = true;

                    yellowBarTexture = new Texture2D(1, 1);
                    yellowBarTexture.SetPixel(0, 0, Color.yellow.SetAlpha(0.5f));
                    yellowBarTexture.Apply();

                    CanvasImage recentDamage = hpBar.Add(new CanvasImage("RecentDamage"));
                    recentDamage.SetImage(yellowBarTexture);
                }

                CanvasText text = hpBar.Add(new CanvasText("HP"));
                text.Size = hpBar.Size;
                text.FontSize = UICommon.ScaleHeight(20);
                text.Alignment = TextAnchor.MiddleCenter;
                text.DropShadow = true;

                CanvasBorder border = hpBar.Add(new CanvasBorder("Border"));
                border.Size = hpBar.Size;
                border.Thickness = 2;

                if (staggerFsm != null)
                {
                    CanvasText staggerText = hpBar.Add(new CanvasText("Combo"));
                    staggerText.LocalPosition = new Vector2(0, -BossNameOffset);
                    staggerText.Size = new Vector2(hpBar.Size.x, BossNameHeight);
                    staggerText.Text = GetStaggerText();
                    staggerText.FontSize = BossNameTextSize;
                    staggerText.Alignment = TextAnchor.LowerLeft;
                    staggerText.DropShadow = true;
                }

                hpBar.Build();

                // Move HP bar behind UI
                hpBar.GameObject.transform.SetAsFirstSibling();

                if (isBoss) bosses.Add(this);
            }

            Vector2 barPos = transform.position;

            if (isBoss)
            {
                int bossIndex = bosses.IndexOf(this);
                if (bossIndex == -1) bossIndex = 0;

                barPos.x = (Screen.width - hpBar.Size.x) / 2f;
                barPos.y = Screen.height - UICommon.ScreenMargin - hpBar.Size.y * (bossIndex + 1) - BossNameHeight * bossIndex;

                // Values were found by scrubbing through Elden Ring boss fight footage frame by frame.
                // The decay percent was originally 2%, but was lowered to adjust for Silksong's combat.
                float recentDamageDecayPercent = 0.005f; // per frame on 60fps
                float recentDamageDecayDelay = 0.7f;
                float damageCounterResetDelay = 2f;

                int damage = lastHealth - HP;
                if (damage > 0 && damage < 5000)
                {
                    damageCounter += damage;
                    lastDamageTime = Time.realtimeSinceStartup;
                    if (damage > MaxHP * recentDamageDecayPercent)
                    {
                        decayingDamageCounter += damage;
                        decayingDamageCounter = Math.Min(decayingDamageCounter, lastHealth);
                        decayingDamageCounter = Math.Min(decayingDamageCounter, MaxHP - HP);
                    }
                }
                else if (damage < 0)
                {
                    // Otherwise it would look like the boss had more health than they actually did
                    decayingDamageCounter = 0;
                }
                lastHealth = HP;

                if (Time.realtimeSinceStartup - lastDamageTime > damageCounterResetDelay)
                {
                    damageCounter = 0;
                }

                if (Time.realtimeSinceStartup - lastDamageTime > recentDamageDecayDelay)
                {
                    decayingDamageCounter -= Math.Max(1, Mathf.RoundToInt(MaxHP * recentDamageDecayPercent * 60 * Time.unscaledDeltaTime));
                    decayingDamageCounter = Math.Max(0, decayingDamageCounter);
                }

                hpBar.Get<CanvasImage>("RecentDamage").LocalPosition = new(hpBar.Size.x * Mathf.Clamp01(HP / (float)MaxHP), 0);
                hpBar.Get<CanvasImage>("RecentDamage").Size = new Vector2(hpBar.Size.x * Mathf.Clamp01(decayingDamageCounter / (float)MaxHP), BarHeight);

                hpBar.Get<CanvasText>("DamageCounter").Text = damageCounter > 0 ? damageCounter.ToString() : "";
            }
            else
            {
                Bounds bounds = sprite?.GetBounds() ?? collider?.bounds ?? new(transform.position, new Vector3(1, 1, 0));
                barPos.y += (bounds.max.y - bounds.min.y) / 2f;

                // Alternative method which is smoother, but looks worse for very short or tall enemies:
                //if (collider) barPos.y += collider.size.y + collider.offset.y;

                if (Camera.main) barPos = Camera.main.WorldToScreenPoint(barPos);

                barPos.x -= BarWidth / 2f;
                barPos.y = Screen.height - barPos.y - hpBar.Size.y;
            }

            hpBar.LocalPosition = barPos;
            hpBar.Get<CanvasImage>("Bar").Size = new Vector2(hpBar.Size.x * Mathf.Clamp01(HP / (float)MaxHP), BarHeight);
            hpBar.Get<CanvasText>("HP").LocalPosition = Vector2.zero;
            hpBar.Get<CanvasText>("HP").Text = $"{HP}/{MaxHP}";

            if (staggerFsm != null)
            {
                hpBar.Get<CanvasText>("Combo").Text = GetStaggerText();
            }
        }

        hpBar?.ActiveSelf = EnemiesPanel.hpBars;
    }

    private string GetStaggerText()
    {
        if (staggerFsm == null) return "Stun disabled"; // shouldn't be called

        FsmInt max = staggerFsm.FsmVariables.GetFsmInt("Stun Hit Max");
        FsmFloat hits = staggerFsm.FsmVariables.GetFsmFloat("Hits Total");

        FsmState inComboState = staggerFsm.GetState("In Combo")!;
        FsmStateAction? comboCheckAction = inComboState.Actions[3];

        if (!comboCheckAction.Active)
        {
            return $"{GetStunControlPrefix()} {hits.Value:0.##}/{max.Value + 0.1f}";
        }

        // Unsure if this is even used here, but it might make sense for the eventual HK port _shrug_
        // prefixes combos as such: t.t (h.h/m)

        FsmInt comboMax = staggerFsm.FsmVariables.GetFsmInt("Stun Combo");
        FsmFloat comboHits = staggerFsm.FsmVariables.GetFsmFloat("Combo Counter");
        FsmFloat comboTime = staggerFsm.FsmVariables.GetFsmFloat("Combo Time");

        string comboCount = staggerFsm.ActiveStateName == "In Combo" ? comboHits.Value.ToString() : "_";

        Wait waitAction = inComboState.GetFirstActionOrDefault<Wait>()!;
        float time = comboTime.Value - waitAction.timer;
        return $"{GetStunControlPrefix()} {time:.0} ({comboCount}/{comboMax.Value}) {hits?.Value:0.##}/{max?.Value + 0.1f}";
    }

    private string GetStunControlPrefix()
    {
        if (staggerFsm == null) return ""; // shouldn't be called

        return staggerFsm.ActiveStateName == "Stop" ? "NoStun" : "";
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Start))]
    [HarmonyPostfix]
    private static void HealthManager_Start(HealthManager __instance)
    {
        if (!__instance.GetComponent<EnemyHandle>())
        {
            __instance.gameObject.AddComponent<EnemyHandle>();
        }
    }

    private static readonly (string displayName, string sceneName, string goName)[] bossPatterns =
    [
        ("Moss Mother", "Tut_03", "Mossbone Mother"),
        ("Moss Mother", "Weave_03", "Mossbone Mother A"),
        ("Moss Mother", "Weave_03", "Mossbone Mother B"),
        ("Bell Beast", "Bone_05", "Bone Beast"),
        ("Lace", "Bone_East_12", "Lace Boss1"),
        ("Fourth Chorus", "Bone_East_08", "SG_head"),
        ("Savage Beastfly", "Ant_19", "Bone Flyer Giant"),
        ("Moorwing", "Greymoor_08", "Vampire Gnat"),
        ("Moorwing", "Greymoor_05", "Vampire Gnat"),
        ("Sister Splinter", "Shellwood_18", "Splinter Queen"),
        ("Skull Tyrant", "Bone_15", "Skull King"),
        ("Skull Tyrant", "Bonetown", "Skull King"),
        ("Great Conchflies", "Coral_11", "Driller A"),
        ("Widow", "Belltown_Shrine", "Spinner Boss"),
        ("Last Judge", "Coral_Judge_Arena", "Last Judge"),
        ("Phantom", "Organ_01", "Phantom"),
        ("Cogwork Dancer", "Cog_Dancers", "Dancer A"),
        ("Cogwork Dancer", "Cog_Dancers", "Dancer B"),
        ("Trobbio", "Library_13", "Trobbio"),
        ("Savage Beastfly", "Bone_East_08", "Bone Flyer Giant"),
        ("The Unravelled", "Ward_02", "Conductor Boss"),
        ("Disgraced Chef Lugoli", "Dust_Chef", "Roachkeeper Chef (1)"),
        ("Voltvyrm", "Coral_29", "Zap Core Enemy"),
        ("Raging Conchfly", "Coral_27", "Coral Conch Driller Giant Solo"),
        ("Broodmother", "Slab_16b", "Slab Fly Broodmother"),
        ("Second Sentinel", "Hang_17b", "Song Knight"),
        ("Groal the Great", "Shadow_18", "Swamp Shaman"),
        ("First Sinner", "Slab_10b", "First Weaver"),
        ("Garmond & Zaza", "Library_09", "Garmond Fighter"),
        ("Shakra", "Greymoor_08", "Mapper Spar NPC"),
        ("Lace", "Song_Tower_01", "Lace Boss2 New"),
        ("Grand Mother Silk", "Cradle_03", "Silk Boss"),
        ("Forebrother Signis", "Dock_09", "Dock Guard Slasher"),
        ("Forebrother Gron", "Dock_09", "Dock Guard Thrower"),
        ("Summoned Saviour", "Bone_Steel_Servant", "Abyss Mass"),
        ("Bell Eater (Head)", "Bellway_Centipede_Arena", "Giant Centipede Head"),
        ("Bell Eater (Rear)", "Bellway_Centipede_Arena", "Giant Centipede Butt"),
        ("Pinstress", "Peak_07", "Pinstress Boss"),
        ("Tormented Trobbio", "Library_13", "Tormented Trobbio"),
        ("Lost Garmond", "Coral_33", "Garmond Black Threaded Fighter"),
        ("Plasmified Zango", "Crawl_10", "Blue Assistant"),
        ("Crawfather", "Room_CrowCourt_02", "Crawfather"),
        ("Crust King Khann", "Memory_Coral_Tower", "Coral King"),
        ("Skarrsinger Karmelita", "Memory_Ant_Queen", "Hunter Queen Boss"),
        ("Gurr the Outcast", "Bone_East_18b", "Bone Hunter Trapper"),
        ("Shrine Guardian Seth", "Shellwood_22", "Seth"),
        ("Nyleth", "Shellwood_11b_Memory", "Flower Queen Boss"),
        ("Palestag", "Clover_19", "Cloverstag White Boss"),
        ("Clover Dancers", "Clover_10", "Dancer A"),
        ("Watcher at the Edge", "Coral_39", "Coral Warrior Grey"),
        ("Lost Lace", "Abyss_Cocoon", "Lost Lace Boss"),
    ];
}