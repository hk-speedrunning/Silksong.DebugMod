using System.Collections.Generic;
using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
using GlobalEnums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod.UI;

public class EnemiesPanel : CanvasPanel
{
    public static EnemiesPanel Instance { get; private set; }
    public readonly List<EnemyHandle> enemyPool = [];

    public static bool hpBars;

    public static void BuildPanel()
    {
        Instance = new EnemiesPanel();
        Instance.Build();
    }

    public EnemiesPanel() : base(nameof(EnemiesPanel), null)
    {
        LocalPosition = new Vector2(1920f - UICommon.images["EnemiesPBg"].width, 481f);

        AddText("Panel Label", "Enemies", new Vector2(125f, -25f), Vector2.zero, UICommon.trajanBold, 30);

        AddText("Enemy Names", "", new Vector2(90f, 20f), Vector2.zero, UICommon.arial);
        AddText("Enemy HP", "", new Vector2(300f, 20f), Vector2.zero, UICommon.arial);

        AddPanel("Pause", UICommon.images["EnemiesPBg"], Vector2.zero, Vector2.zero, new Rect(0, 0, UICommon.images["EnemiesPBg"].width, UICommon.images["EnemiesPBg"].height));
        AddPanel("Play", UICommon.images["EnemiesBg"], new Vector2(57f, 0f), Vector2.zero, new Rect(0f, 0f, UICommon.images["EnemiesBg"].width, UICommon.images["EnemiesBg"].height));

        for (int i = 1; i <= 14; i++)
        {
            GetPanel("Pause").AddButton("Del" + i, UICommon.images["ButtonDel"], new Vector2(20f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), () => DelClicked(i), new Rect(0, 0, UICommon.images["ButtonDel"].width, UICommon.images["ButtonDel"].height));
            GetPanel("Pause").AddButton("Clone" + i, UICommon.images["ButtonPlus"], new Vector2(40f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), () => CloneClicked(i), new Rect(0, 0, UICommon.images["ButtonPlus"].width, UICommon.images["ButtonPlus"].height));
            GetPanel("Pause").AddButton("Inf" + i, UICommon.images["ButtonInf"], new Vector2(60f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), () => InfClicked(i), new Rect(0, 0, UICommon.images["ButtonInf"].width, UICommon.images["ButtonInf"].height));
        }

        GetPanel("Pause").AddButton("HP Bars", UICommon.images["ButtonRect"], new Vector2(30f, 250f), Vector2.zero, HPBarsClicked, new Rect(0, 0, UICommon.images["ButtonRect"].width, UICommon.images["ButtonRect"].height), UICommon.trajanBold, "HP Bars");
    }

    private void DelClicked(int index)
    {
        if (index <= enemyPool.Count)
        {
            EnemyHandle handle = enemyPool[index - 1];
            Object.DestroyImmediate(handle.gameObject);

            DebugMod.LogConsole($"Destroying enemy: {handle.gameObject.name}");
        }
    }

    private void CloneClicked(int index)
    {
        if (index <= enemyPool.Count)
        {
            EnemyHandle handle = enemyPool[index - 1];
            GameObject gameObject2 = Object.Instantiate(handle.gameObject, handle.transform.position, handle.transform.rotation);

            DebugMod.LogConsole($"Cloning enemy as: {gameObject2.name}");
        }
    }

    private void InfClicked(int index)
    {
        if (index <= enemyPool.Count)
        {
            EnemyHandle handle = enemyPool[index - 1];
            handle.HP = 9999;

            DebugMod.LogConsole($"HP for enemy: {handle.gameObject.name} is now 9999");
        }
    }

    private void HPBarsClicked() => BindableFunctions.ToggleEnemyHPBars();
    
    public override void Update()
    {
        base.Update();

        ActiveSelf = DebugMod.settings.EnemiesPanelVisible;

        if (DebugMod.settings.EnemiesPanelVisible && UIManager.instance.uiState == UIState.PLAYING)
        {
            GetPanel("Pause").ActiveSelf = false;
            GetPanel("Play").ActiveSelf = true;
        }
        else if (DebugMod.settings.EnemiesPanelVisible && UIManager.instance.uiState == UIState.PAUSED)
        {
            GetPanel("Pause").ActiveSelf = true;
            GetPanel("Play").ActiveSelf = false;
        }
        else
        {
            GetPanel("Pause").ActiveSelf = false;
            GetPanel("Play").ActiveSelf = false;
        }

        enemyPool.RemoveAll(handle => !handle && !handle.gameObject.activeSelf);

        if (ActiveInHierarchy)
        {
            string enemyNames = "";
            string enemyHP = "";
            int enemyCount = 0;

            if (IsActive())
            {
                foreach (EnemyHandle handle in enemyPool)
                {
                    if (++enemyCount <= 14)
                    {
                        enemyNames += $"{handle.gameObject.name}\n";
                        enemyHP += $"{handle.HP}/{handle.MaxHP}\n";
                    }
                }
            }

            if (GetPanel("Pause").ActiveInHierarchy)
            {
                for (int i = 1; i <= 14; i++)
                {
                    if (i <= enemyCount)
                    {
                        GetPanel("Pause").GetButton("Del" + i).ActiveSelf = true;
                        GetPanel("Pause").GetButton("Clone" + i).ActiveSelf = true;
                        GetPanel("Pause").GetButton("Inf" + i).ActiveSelf = true;
                    }
                    else
                    {
                        GetPanel("Pause").GetButton("Del" + i).ActiveSelf = false;
                        GetPanel("Pause").GetButton("Clone" + i).ActiveSelf = false;
                        GetPanel("Pause").GetButton("Inf" + i).ActiveSelf = false;
                    }
                }

                GetPanel("Pause").GetButton("HP Bars").Text.Color =
                    hpBars ? new Color(244f / 255f, 127f / 255f, 32f / 255f) : Color.white;
            }

            if (enemyCount > 14)
            {
                enemyNames += "And " + (enemyCount - 14) + " more";
            }

            GetText("Enemy Names").Text = enemyNames;
            GetText("Enemy HP").Text = enemyHP;
        }
    }

    public static bool IsActive()
    {
        return HeroController.instance && !HeroController.instance.cState.transitioning && GameManager.instance.IsGameplayScene();
    }
}
