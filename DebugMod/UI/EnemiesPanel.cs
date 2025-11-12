using System.Collections.Generic;
using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod.UI;

public class EnemiesPanel : CanvasAutoPanel
{
    public static int ListingHeight => UICommon.ScaleHeight(15);
    public static int ButtonWidth => UICommon.ScaleWidth(100);

    public static EnemiesPanel Instance { get; private set; }
    public static readonly List<EnemyHandle> enemyPool = [];

    public static bool hpBars;

    private int numListings;

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

        CanvasText panelTitle = Append(new CanvasText("PanelTitle"), UICommon.ScaleHeight(30));
        panelTitle.Text = "Enemies";
        panelTitle.Font = UICommon.trajanBold;
        panelTitle.FontSize = UICommon.ScaleHeight(30);
        panelTitle.Alignment = TextAnchor.UpperCenter;

        numListings = 0;

        while (Offset + ListingHeight <= Size.y - UICommon.ControlHeight - UICommon.Margin * 2)
        {
            int index = numListings++;

            CanvasControl control = Append(new CanvasControl($"{index + 1}"), ListingHeight);

            CanvasText enemyName = control.AppendFlex(new CanvasText("EnemyName"));
            enemyName.Alignment = TextAnchor.MiddleLeft;
            enemyName.OnUpdate += () => enemyName.Text = enemyPool[index].Name;

            CanvasText enemyHp = control.Append(new CanvasText("EnemyHP"), UICommon.ScaleWidth(80));
            enemyHp.Alignment = TextAnchor.MiddleLeft;
            enemyHp.OnUpdate += () => enemyHp.Text = $"{enemyPool[index].HP}/{enemyPool[index].MaxHP}";

            CanvasButton delete = control.AppendSquare(new CanvasButton("Delete"));
            delete.ImageOnly(UICommon.images["ButtonDel"]);
            delete.OnClicked += () =>
            {
                EnemyHandle handle = enemyPool[index];
                Object.DestroyImmediate(handle.gameObject);
                DebugMod.LogConsole($"Destroyed {handle.Name}");
            };

            control.AppendPadding(UICommon.Margin);

            CanvasButton clone = control.AppendSquare(new CanvasButton("Clone"));
            clone.ImageOnly(UICommon.images["ButtonPlus"]);
            clone.OnClicked += () =>
            {
                EnemyHandle handle = enemyPool[index];
                Object.Instantiate(handle.gameObject, handle.transform.position, handle.transform.rotation);
                DebugMod.LogConsole($"Cloned {handle.Name}");
            };

            control.AppendPadding(UICommon.Margin);

            CanvasButton infHealth = control.AppendSquare(new CanvasButton("InfiniteHealth"));
            infHealth.ImageOnly(UICommon.images["ButtonInf"]);
            infHealth.OnClicked += () =>
            {
                EnemyHandle handle = enemyPool[index];
                handle.HP = 9999;
                DebugMod.LogConsole($"Set {handle.Name} HP to 9999");
            };
        }

        CanvasText overflow = Append(new CanvasText("Overflow"), ListingHeight);
        overflow.OnUpdate += () => overflow.Text = enemyPool.Count > numListings ? $"... and {enemyPool.Count - numListings} more" : "";

        CanvasButton hpBarsButton = Add(new CanvasButton("HPBars"));
        hpBarsButton.LocalPosition = new Vector2(Size.x - UICommon.Margin - ButtonWidth, Size.y - UICommon.Margin - UICommon.ControlHeight);
        hpBarsButton.Size = new Vector2(ButtonWidth, UICommon.ControlHeight);
        hpBarsButton.Text.Text = "HP Bars";
        hpBarsButton.OnClicked += BindableFunctions.ToggleEnemyHPBars;
        hpBarsButton.OnUpdate += () => hpBarsButton.Text.Color = hpBars ? UICommon.accentColor : UICommon.textColor;
    }
    
    public override void Update()
    {
        enemyPool.RemoveAll(handle => !handle && !handle.gameObject.activeSelf);

        int enemyCount = ActivelyUpdating() ? enemyPool.Count : 0;

        for (int i = 1; i <= numListings; i++)
        {
            Get<CanvasControl>(i.ToString()).ActiveSelf = i <= enemyCount;
        }

        base.Update();
    }

    public static bool ActivelyUpdating()
    {
        return HeroController.instance && !HeroController.instance.cState.transitioning && GameManager.instance.IsGameplayScene();
    }
}
