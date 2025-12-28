using DebugMod.UI;
using DebugMod.UI.Canvas;
using HarmonyLib;
using System;
using TMProOld;
using UnityEngine;

namespace DebugMod.MonoBehaviours;

[HarmonyPatch]
public class EnemyHandle : MonoBehaviour
{
    private const int HPBAR_WIDTH = 120;
    private const int HPBAR_HEIGHT = 40;

    private HealthManager hm;
    private tk2dSprite sprite;
    private BoxCollider2D collider;
    private CanvasPanel hpBar;
    private Texture2D barTexture;
    private int lastHP = -100000;
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

        if (maxHP <= 0 && hm.hp > 0)
        {
            maxHP = hm.hp;
        }

        if (EnemiesPanel.hpBars)
        {
            if (hpBar == null)
            {
                barTexture = new Texture2D(HPBAR_WIDTH, 1);
                Color[] colors = new Color[HPBAR_WIDTH];
                Array.Fill(colors, Color.red);
                barTexture.SetPixels(colors);
                barTexture.Apply();

                hpBar = new CanvasPanel($"{gameObject.name} HP Bar");
                hpBar.Size = new Vector2(HPBAR_WIDTH, HPBAR_HEIGHT);

                CanvasImage background = UICommon.AddBackground(hpBar);
                background.SetImage(barTexture);
                background.Border.Size = hpBar.Size;
                background.Border.Thickness = 3;

                CanvasText text = hpBar.Add(new CanvasText("HP"));
                text.Size = hpBar.Size;
                text.FontSize = 20;
                text.Alignment = TextAlignmentOptions.Center;

                hpBar.Build();
            }

            if (HP != lastHP)
            {
                hpBar.Get<CanvasImage>("Background").Size = new Vector2(HPBAR_WIDTH * (float)HP / MaxHP, HPBAR_HEIGHT);
                lastHP = HP;
            }

            Vector2 barPos = transform.position;

            Bounds bounds = sprite?.GetBounds() ?? collider?.bounds ?? new(transform.position, new Vector3(1, 1, 0));
            barPos.y += (bounds.max.y - bounds.min.y) / 2f;

            barPos = Camera.main.WorldToScreenPoint(barPos);
            barPos.x -= HPBAR_WIDTH / 2f;

            hpBar.LocalPosition = barPos;
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