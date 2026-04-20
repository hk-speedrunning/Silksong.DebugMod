using DebugMod.UI;
using DebugMod.UI.Canvas;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace DebugMod.MonoBehaviours;

[HarmonyPatch]
public class EnemyHandle : MonoBehaviour
{
    private static int BarWidth => UICommon.ScaleWidth(150);
    private static int BarHeight => UICommon.ScaleHeight(40);

    private HealthManager hm;
    private tk2dSprite sprite;
    private BoxCollider2D collider;
    private CanvasPanel hpBar;
    private Texture2D barTexture;
    private int maxHP;

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
    }

    public void Update()
    {
        if (!EnemiesPanel.ActivelyUpdating())
        {
            hpBar?.ActiveSelf = false;
            return;
        }

        if (maxHP <= 0 && hm.hp > 0)
        {
            maxHP = hm.hp;
        }

        if (EnemiesPanel.hpBars)
        {
            if (hpBar == null)
            {
                barTexture = new Texture2D(BarWidth, 1);
                Color[] colors = new Color[BarWidth];
                Array.Fill(colors, Color.red.SetAlpha(0.5f));
                barTexture.SetPixels(colors);
                barTexture.Apply();

                hpBar = new CanvasPanel($"{gameObject.name} HP Bar");
                hpBar.Size = new Vector2(BarWidth, BarHeight);

                CanvasImage background = UICommon.AddBackground(hpBar);
                background.SetImage(barTexture);
                background.Border.Size = hpBar.Size;
                background.Border.Thickness = 2;

                CanvasText text = hpBar.Add(new CanvasText("HP"));
                text.Size = hpBar.Size;
                text.FontSize = UICommon.ScaleHeight(20);
                text.Alignment = TextAnchor.MiddleCenter;

                hpBar.Build();

                // Move HP bar behind UI
                foreach (CanvasNode node in hpBar.Subtree().Reverse())
                {
                    node.GameObject.transform.SetAsFirstSibling();
                }
            }

            Vector2 barPos = transform.position;

            Bounds bounds = sprite?.GetBounds() ?? collider?.bounds ?? new(transform.position, new Vector3(1, 1, 0));
            barPos.y += (bounds.max.y - bounds.min.y) / 2f;

            barPos = Camera.main.WorldToScreenPoint(barPos);
            barPos.x -= BarWidth / 2f;
            barPos.y = Screen.height - barPos.y - hpBar.Size.y;

            hpBar.LocalPosition = barPos;
            hpBar.Get<CanvasImage>("Background").Size = new Vector2(BarWidth * Mathf.Clamp01(HP / (float)MaxHP), BarHeight);
            hpBar.Get<CanvasText>("HP").LocalPosition = Vector2.zero;
            hpBar.Get<CanvasText>("HP").Text = $"{HP}/{MaxHP}";
        }

        hpBar?.ActiveSelf = EnemiesPanel.hpBars;
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