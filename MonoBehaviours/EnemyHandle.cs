using DebugMod.Helpers;
using DebugMod.UI;
using DebugMod.UI.Canvas;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bounds = UnityEngine.Bounds;

namespace DebugMod.MonoBehaviours;
#nullable enable

[HarmonyPatch]
public class EnemyHandle : MonoBehaviour
{
    private static int BarWidth => UICommon.ScaleWidth(150);
    private static int BarHeight => UICommon.ScaleHeight(40);
    private static int BossBarWidth => UICommon.ScaleWidth(900);
    private static int BossNameHeight => UICommon.ScaleHeight(20);
    private static int BossBarSeparation => UICommon.ScaleHeight(12);

    private static List<EnemyHandle> bosses = [];

    private HealthManager hm;
    private tk2dSprite? sprite;
    private BoxCollider2D collider;
    private CanvasPanel? hpBar;
    private Texture2D? barTexture;
    private int maxHP;
    private bool maxHPSet;
    private bool isBoss;

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
                string normalizedName =  gameObject.name.Replace("(Clone)", "");

                foreach ((string sceneName, string goName) in bossPatterns)
                {
                    if (normalizedName == goName && SceneManager.GetSceneByName(sceneName).isLoaded)
                    {
                        isBoss = true;
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

                CanvasBorder border = hpBar.Add(new CanvasBorder("Border"));
                border.Size = hpBar.Size;
                border.Thickness = 2;

                CanvasText text = hpBar.Add(new CanvasText("HP"));
                text.Size = hpBar.Size;
                text.FontSize = UICommon.ScaleHeight(20);
                text.Alignment = TextAnchor.MiddleCenter;

                if (isBoss)
                {
                    CanvasText bossName = hpBar.Add(new CanvasText("BossName"));
                    bossName.LocalPosition = new Vector2(0, -BossNameHeight);
                    bossName.Size = new Vector2(hpBar.Size.x, BossNameHeight);
                    bossName.FontSize = UICommon.ScaleHeight(18);
                    bossName.Alignment = TextAnchor.MiddleCenter;
                    bossName.Text = gameObject.name;
                }

                if (staggerFsm != null)
                {
                    CanvasText staggerText = hpBar.Add(new CanvasText("Combo"));
                    staggerText.Text = GetStaggerText();
                    staggerText.FontSize = UICommon.ScaleHeight(18);
                    staggerText.Size = hpBar.Size;
                    staggerText.Alignment = TextAnchor.LowerRight;
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
                barPos.y = Screen.height - (hpBar.Size.y + BossBarSeparation) * (bossIndex + 1) - BossNameHeight * bossIndex;
            }
            else
            {
                Bounds bounds = sprite?.GetBounds() ?? collider?.bounds ?? new(transform.position, new Vector3(1, 1, 0));
                barPos.y += (bounds.max.y - bounds.min.y) / 2f;

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
                hpBar.Get<CanvasText>("Combo").LocalPosition = new Vector2(0, -hpBar.Size.y);
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

    private static readonly (string sceneName, string goName)[] bossPatterns =
    [
        // === ACT 1 ===

        // Moss Mother 1
        ("Tut_03", "Mossbone Mother"),
        // Moss Mother 2
        ("Weave_03", "Mossbone Mother A"),
        ("Weave_03", "Mossbone Mother B"),
        // Bell Beast
        ("Bone_05", "Bone Beast"),
        // Lace
        ("Bone_East_12", "Lace Boss1"),
        // Fourth Chorus
        ("Bone_East_08", "SG_head"),
        // Savage Beastfly 1
        ("Ant_19", "Bone Flyer Giant"),
        // Moorwing
        ("Greymoor_08", "Vampire Gnat"),
        ("Greymoor_05", "Vampire Gnat"),
        // Sister Splinter
        ("Shellwood_18", "Splinter Queen"),
        // Skull Tyrant 1
        ("Bone_15", "Skull King"),
        // Skull Tyrant 2
        ("Bonetown", "Skull King"),
        // Great Conchflies
        ("Coral_11", "Driller A"),
        // Widow
        ("Belltown_Shrine", "Spinner Boss"),
        // Last Judge
        ("Coral_Judge_Arena", "Last Judge"),
        // Phantom
        ("Organ_01", "Phantom"),

        // === ACT 2 ===

        // Cogwork Dancers
        ("Cog_Dancers", "Dancer A"),
        ("Cog_Dancers", "Dancer B"),
        // Trobbio
        ("Library_13", "Trobbio"),
        // Savage Beastfly 2
        ("Bone_East_08", "Bone Flyer Giant"),
        // The Unravelled
        ("Ward_02", "Conductor Boss"),
        // Disgraced Chef Lugoli
        ("Dust_Chef", "Roachkeeper Chef (1)"),
        // Voltvyrm
        ("Coral_29", "Zap Core Enemy"),
        // Raging Conchfly
        ("Coral_27", "Coral Conch Driller Giant Solo"),
        // Broodmother
        ("Slab_16b", "Slab Fly Broodmother"),
        // Second Sentinel
        ("Hang_17b", "Song Knight"),
        // Groal the Great
        ("Shadow_18", "Swamp Shaman"),
        // First Sinner
        ("Slab_10b", "First Weaver"),
        // Garmond & Zaza
        ("Library_09", "Garmond Fighter"),
        // Shakra
        ("Greymoor_08", "Mapper Spar NPC"),
        // Lace 2
        ("Song_Tower_01", "Lace Boss2 New"),
        // Grand Mother Silk
        ("Cradle_03", "Silk Boss"),
        // Forebrothers Signis & Gron
        ("Dock_09", "Dock Guard Slasher"),
        ("Dock_09", "Dock Guard Thrower"),
        // Summoned Saviour
        ("Bone_Steel_Servant", "Abyss Mass"),

        // === ACT 3 ===

        // Bell Eater
        ("Bellway_Centipede_Arena", "Giant Centipede Head"),
        ("Bellway_Centipede_Arena", "Giant Centipede Butt"),
        // Pinstress
        ("Peak_07", "Pinstress Boss"),
        // Tormented Trobbio
        ("Library_13", "Tormented Trobbio"),
        // Lost Garmond
        ("Coral_33", "Garmond Black Threaded Fighter"),
        // Plasmified Zango
        ("Crawl_10", "Blue Assistant"),
        // Crawfather
        ("Room_CrowCourt_02", "Crawfather"),
        // Crust King Khann
        ("Memory_Coral_Tower", "Coral King"),
        // Skarrsinger Karmelita
        ("Memory_Ant_Queen", "Hunter Queen Boss"),
        // Gurr the Outcast
        ("Bone_East_18b", "Bone Hunter Trapper"),
        // Shrine Guardian Seth
        ("Shellwood_22", "Seth"),
        // Nyleth
        ("Shellwood_11b_Memory", "Flower Queen Boss"),
        // Palestag
        ("Clover_19", "Cloverstag White Boss"),
        // Clover Dancers
        ("Clover_10", "Dancer A"),
        // Watcher at the Edge
        ("Coral_39", "Coral Warrior Grey"),
        // Lost Lace
        ("Abyss_Cocoon", "Lost Lace Boss"),
    ];
}