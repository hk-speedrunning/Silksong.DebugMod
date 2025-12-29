using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod.UI;

public class EnemiesPanel : CanvasPanel
{
    public static int ListingHeight => UICommon.ScaleHeight(16);

    public static EnemiesPanel Instance { get; private set; }
    public static readonly List<EnemyHandle> enemyPool = [];

    public static bool hpBars;

    private readonly List<CanvasPanel> listings = [];

    public static void BuildPanel()
    {
        Instance = new EnemiesPanel();
        Instance.Build();
    }

    public EnemiesPanel() : base(nameof(EnemiesPanel))
    {
        LocalPosition = new Vector2(Screen.width - UICommon.ScreenMargin - UICommon.RightSideWidth, UICommon.MainPanelHeight + UICommon.ScreenMargin * 2);
        Size = new Vector2(UICommon.RightSideWidth, Screen.height - UICommon.MainPanelHeight - UICommon.ScreenMargin * 3);

        UICommon.AddBackground(this);

        using PanelBuilder builder = new(this);
        builder.Padding = UICommon.Margin;

        while (builder.GetCurrentLength() + ListingHeight + UICommon.Margin <= Size.y - UICommon.ControlHeight - UICommon.Margin * 2)
        {
            int index = listings.Count;

            CanvasPanel listing = builder.AppendFixed(new CanvasPanel($"{index + 1}"), ListingHeight);
            listing.CollapseMode = CollapseMode.Deny;
            listings.Add(listing);

            using PanelBuilder listingBuilder = new(listing);
            listingBuilder.Horizontal = true;

            CanvasText enemyName = listingBuilder.AppendFlex(new CanvasText("EnemyName"));
            enemyName.Alignment = TextAnchor.MiddleLeft;
            enemyName.OnUpdate += () => enemyName.Text = enemyPool[index].Name;

            CanvasText enemyHp = listingBuilder.AppendFixed(new CanvasText("EnemyHP"), UICommon.ScaleWidth(80));
            enemyHp.Alignment = TextAnchor.MiddleLeft;
            enemyHp.OnUpdate += () => enemyHp.Text = $"{enemyPool[index].HP}/{enemyPool[index].MaxHP}";

            CanvasButton delete = listingBuilder.AppendSquare(new CanvasButton("Delete"));
            delete.ImageOnly(UICommon.images["IconX"]);
            delete.OnClicked += () =>
            {
                EnemyHandle handle = enemyPool[index];
                Object.DestroyImmediate(handle.gameObject);
                DebugMod.LogConsole($"Destroyed {handle.Name}");
            };

            listingBuilder.AppendPadding(UICommon.Margin);

            CanvasButton clone = listingBuilder.AppendSquare(new CanvasButton("Clone"));
            clone.ImageOnly(UICommon.images["IconPlus"]);
            clone.OnClicked += () =>
            {
                EnemyHandle handle = enemyPool[index];
                Object.Instantiate(handle.gameObject, handle.transform.position, handle.transform.rotation);
                DebugMod.LogConsole($"Cloned {handle.Name}");
            };

            listingBuilder.AppendPadding(UICommon.Margin);

            CanvasButton infHealth = listingBuilder.AppendSquare(new CanvasButton("InfiniteHealth"));
            infHealth.ImageOnly(UICommon.images["IconShield"]);
            infHealth.OnClicked += () =>
            {
                EnemyHandle handle = enemyPool[index];
                handle.HP = 9999;
                DebugMod.LogConsole($"Set {handle.Name} HP to 9999");
            };
        }

        CanvasPanel footer = Add(new CanvasPanel("Footer"));
        footer.LocalPosition = new Vector2(UICommon.Margin, Size.y - UICommon.Margin - UICommon.ControlHeight);
        footer.Size = new Vector2(Size.x - UICommon.Margin * 2, UICommon.ControlHeight);

        using PanelBuilder footerBuilder = new(footer);
        footerBuilder.Horizontal = true;

        footerBuilder.AppendPadding(UICommon.Margin);

        CanvasText overflow = footerBuilder.AppendFlex(new CanvasText("Overflow"));
        overflow.Alignment = TextAnchor.MiddleLeft;
        overflow.OnUpdate += () =>
        {
            if (enemyPool.Count == 0)
            {
                overflow.Text = "No enemies detected";
            }
            else if (enemyPool.Count > listings.Count)
            {
                overflow.Text = $"... and {enemyPool.Count - listings.Count} more";
            }
            else
            {
                overflow.Text = "";
            }
        };

        CanvasButton hpBarsButton = footerBuilder.AppendFixed(new CanvasButton("HPBars"), UICommon.ScaleWidth(100));
        hpBarsButton.Text.Text = "HP Bars";
        hpBarsButton.OnClicked += () =>
        {
            BindableFunctions.ToggleEnemyHPBars();
            hpBarsButton.Toggled = hpBars;
        };
    }

    public override void Update()
    {
        enemyPool.RemoveAll(handle => !handle && !handle.gameObject.activeSelf);

        int enemyCount = ActivelyUpdating() ? enemyPool.Count : 0;

        for (int i = 0; i < listings.Count; i++)
        {
            listings[i].ActiveSelf = i < enemyCount;
        }

        base.Update();
    }

    public static bool ActivelyUpdating()
    {
        return HeroController.instance && !HeroController.instance.cState.transitioning && GameManager.instance.IsGameplayScene();
    }
}
