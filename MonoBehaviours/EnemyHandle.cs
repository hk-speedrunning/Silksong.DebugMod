using DebugMod.Helpers;
using DebugMod.UI;
using DebugMod.UI.Canvas;
using GenericVariableExtension;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Linq;
using UnityEngine;
using Bounds = UnityEngine.Bounds;

namespace DebugMod.MonoBehaviours;
#nullable enable

[HarmonyPatch]
public class EnemyHandle : MonoBehaviour
{
    private const int HPBAR_WIDTH = 150;
    private const int HPBAR_HEIGHT = 40;

    private HealthManager hm;
    private tk2dSprite? sprite;
    private BoxCollider2D collider;
    private CanvasPanel? hpBar;
    private Texture2D? barTexture;
    private int maxHP;

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
        hpBar?.Destroy();
    }

    public void OnEnable() => Awake();
    public void OnDisable() => OnDestroy();

    public void Update()
    {
        if (!EnemiesPanel.ActivelyUpdating())
        {
            hpBar?.ActiveSelf = false;
            return;
        }

        if (maxHP < hm.hp && hm.hp > 0)
        {
            maxHP = hm.hp;
        }

        if (EnemiesPanel.hpBars)
        {
            if (hpBar == null)
            {
                barTexture = new Texture2D(HPBAR_WIDTH, 1);
                Color[] colors = new Color[HPBAR_WIDTH];
                Array.Fill(colors, Color.red.SetAlpha(0.5f));
                barTexture.SetPixels(colors);
                barTexture.Apply();

                hpBar = new CanvasPanel($"{gameObject.name} HP Bar");
                hpBar.Size = new Vector2(HPBAR_WIDTH, HPBAR_HEIGHT);

                CanvasImage background = UICommon.AddBackground(hpBar);
                background.SetImage(barTexture);
                background.Border.Size = hpBar.Size;
                background.Border.Thickness = 2;

                CanvasText text = hpBar.Add(new CanvasText("HP"));
                text.Size = hpBar.Size;
                text.FontSize = 20;
                text.Alignment = TextAnchor.MiddleCenter;

                if (staggerFsm != null)
                {
                    CanvasText staggerText = hpBar.Add(new CanvasText("Combo"));
                    staggerText.Text = GetStaggerText();
                    staggerText.FontSize = 18;
                    staggerText.Size = hpBar.Size; // same size as hpbar, but above
                    staggerText.Alignment = TextAnchor.LowerRight;
                }

                hpBar.Build();

                // Move HP bar behind UI
                foreach (CanvasNode node in hpBar.Subtree().Reverse())
                {
                    if (node is CanvasObject obj)
                    {
                        obj.GameObject.transform.SetAsFirstSibling();
                    }
                }
            }

            Vector2 barPos = transform.position;

            Bounds bounds = sprite?.GetBounds() ?? collider?.bounds ?? new(transform.position, new Vector3(1, 1, 0));
            barPos.y += (bounds.max.y - bounds.min.y) / 2f;

            if (Camera.main) barPos = Camera.main.WorldToScreenPoint(barPos);
            barPos.x -= HPBAR_WIDTH / 2f;
            barPos.y = Screen.height - barPos.y - hpBar.Size.y;

            hpBar.LocalPosition = barPos;
            hpBar.Get<CanvasImage>("Background").Size = new Vector2(HPBAR_WIDTH * Mathf.Clamp01(HP / (float)MaxHP), HPBAR_HEIGHT);
            hpBar.Get<CanvasText>("HP").LocalPosition = Vector2.zero;
            hpBar.Get<CanvasText>("HP").Text = $"{HP}/{MaxHP}";
            if (staggerFsm != null)
            {
                hpBar.Get<CanvasText>("Combo").LocalPosition = new Vector2(0, -HPBAR_HEIGHT);
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
            return $"{hits.Value:0.##}/{max.Value + 0.1f}";
        }
        
        // Unsure if this is even used here, but it might make sense for the eventual HK port _shrug_
        // prefixes combos as such: t.t (h.h/m)

        FsmInt comboMax = staggerFsm.FsmVariables.GetFsmInt("Stun Combo");
        FsmFloat comboHits = staggerFsm.FsmVariables.GetFsmFloat("Combo Counter");
        FsmFloat comboTime = staggerFsm.FsmVariables.GetFsmFloat("Combo Time");
        
        string comboCount = staggerFsm.ActiveStateName == "In Combo" ? comboHits.Value.ToString() : "_";
        
        Wait waitAction = inComboState.GetFirstActionOrDefault<Wait>()!;
        float time = comboTime.Value - waitAction.timer;
        return $"{time:.0} ({comboCount}/{comboMax.Value}) {hits?.Value:0.##}/{max?.Value + 0.1f}";
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
}