using DebugMod.UI;
using DebugMod.UI.Canvas;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;

namespace DebugMod.MonoBehaviours;

[HarmonyPatch]
public class EnemyHandle : MonoBehaviour
{
    private const int HPBAR_WIDTH = 120;
    private const int HPBAR_HEIGHT = 40;
    private const int HPBAR_BORDER = 3;

    private HealthManager hm;
    private tk2dSprite sprite;
    private BoxCollider2D collider;
    private CanvasPanel hpBar;
    private int lastHP = -1;
    private int maxHP;

    public int HP
    {
        get => hm.hp;
        set => hm.hp = value;
    }

    public int MaxHP => maxHP;

    public void Awake()
    {
        hm = GetComponent<HealthManager>();
        sprite = GetComponent<tk2dSprite>();
        collider = GetComponent<BoxCollider2D>();

        if (!EnemiesPanel.Instance.enemyPool.Contains(this))
        {
            EnemiesPanel.Instance.enemyPool.Add(this);
        }
    }

    public void OnDestroy()
    {
        EnemiesPanel.Instance.enemyPool.Remove(this);
        hpBar?.Destroy();
    }

    public void OnEnable() => Awake();
    public void OnDisable() => OnDestroy();

    public void Update()
    {
        if (!EnemiesPanel.IsActive())
        {
            if (hpBar != null)
            {
                hpBar.ActiveSelf = false;
            }
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
                hpBar = new CanvasPanel($"{gameObject.name} HP Bar", null, Vector2.zero, Vector2.zero, DrawTexture(), new Rect(0, 0, HPBAR_WIDTH, HPBAR_HEIGHT));
                hpBar.AddText("HP", "", Vector2.zero, new Vector2(HPBAR_WIDTH, HPBAR_HEIGHT), UICommon.arial, 20, FontStyle.Normal, TextAnchor.MiddleCenter);
                hpBar.Build();
            }

            if (HP != lastHP)
            {
                hpBar.GetImage("Background").UpdateImage(DrawTexture(), new Rect(0, 0, HPBAR_WIDTH, HPBAR_HEIGHT));
                lastHP = HP;
            }

            Vector2 barPos = transform.position;

            Bounds bounds = sprite?.GetBounds() ?? collider?.bounds ?? new(transform.position, new Vector3(1, 1, 0));
            barPos.y += (bounds.max.y - bounds.min.y) / 2f;

            barPos = Camera.main.WorldToScreenPoint(barPos);
            barPos.x = barPos.x / Screen.width * 1920f;
            barPos.y = (1 - barPos.y / Screen.height) * 1080f;

            barPos.x -= HPBAR_WIDTH / 2f;

            hpBar.LocalPosition = barPos;
            hpBar.GetText("HP").Text = $"{HP}/{MaxHP}";
        }

        if (hpBar != null)
        {
            hpBar.ActiveSelf = EnemiesPanel.hpBars;
        }
    }

    private Texture2D DrawTexture()
    {
        Texture2D tex = new Texture2D(HPBAR_WIDTH, HPBAR_HEIGHT);

        for (int x = 0; x < HPBAR_WIDTH; x++)
        {
            for (int y = 0; y < HPBAR_HEIGHT; y++)
            {
                Color color;
                if (x < HPBAR_BORDER || x >= HPBAR_WIDTH - HPBAR_BORDER || y < HPBAR_BORDER || y >= HPBAR_HEIGHT - HPBAR_BORDER)
                {
                    color = Color.black;
                }
                else if (HP / (float)MaxHP >= (x - HPBAR_BORDER) / (float)(HPBAR_WIDTH - HPBAR_BORDER))
                {
                    color = Color.red;
                }
                else
                {
                    color = Color.clear;
                }

                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        return tex;
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Start))]
    [HarmonyPostfix]
    private static void HealthManager_Start(HealthManager __instance)
    {
        if (__instance.gameObject.layer == (int)PhysLayers.ENEMIES && !__instance.GetComponent<EnemyHandle>())
        {
            __instance.gameObject.AddComponent<EnemyHandle>();
        }
    }
}