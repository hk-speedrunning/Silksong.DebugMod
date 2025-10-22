using System.Collections.Generic;
using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
using GlobalEnums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod.UI;

public static class EnemiesPanel
{
    private static CanvasPanel panel;
    public static readonly List<EnemyHandle> enemyPool = [];

    public static GameObject parent { get; private set; }
    public static bool hpBars;

    public static void BuildMenu(GameObject canvas)
    {
        parent = canvas;

        panel = new CanvasPanel(nameof(EnemiesPanel), null, new Vector2(1920f - GUIController.Instance.images["EnemiesPBg"].width, 481f), Vector2.zero);

        panel.AddText("Panel Label", "Enemies", new Vector2(125f, -25f), Vector2.zero, GUIController.Instance.trajanBold, 30);

        panel.AddText("Enemy Names", "", new Vector2(90f, 20f), Vector2.zero, GUIController.Instance.arial);
        panel.AddText("Enemy HP", "", new Vector2(300f, 20f), Vector2.zero, GUIController.Instance.arial);

        panel.AddPanel("Pause", GUIController.Instance.images["EnemiesPBg"], Vector2.zero, Vector2.zero, new Rect(0, 0, GUIController.Instance.images["EnemiesPBg"].width, GUIController.Instance.images["EnemiesPBg"].height));
        panel.AddPanel("Play", GUIController.Instance.images["EnemiesBg"], new Vector2(57f, 0f), Vector2.zero, new Rect(0f, 0f, GUIController.Instance.images["EnemiesBg"].width, GUIController.Instance.images["EnemiesBg"].height));

        for (int i = 1; i <= 14; i++)
        {
            panel.GetPanel("Pause").AddButton("Del" + i, GUIController.Instance.images["ButtonDel"], new Vector2(20f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), () => DelClicked(i), new Rect(0, 0, GUIController.Instance.images["ButtonDel"].width, GUIController.Instance.images["ButtonDel"].height));
            panel.GetPanel("Pause").AddButton("Clone" + i, GUIController.Instance.images["ButtonPlus"], new Vector2(40f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), () => CloneClicked(i), new Rect(0, 0, GUIController.Instance.images["ButtonPlus"].width, GUIController.Instance.images["ButtonPlus"].height));
            panel.GetPanel("Pause").AddButton("Inf" + i, GUIController.Instance.images["ButtonInf"], new Vector2(60f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), () => InfClicked(i), new Rect(0, 0, GUIController.Instance.images["ButtonInf"].width, GUIController.Instance.images["ButtonInf"].height));
        }

        panel.GetPanel("Pause").AddButton("HP Bars", GUIController.Instance.images["ButtonRect"], new Vector2(30f, 250f), Vector2.zero, HPBarsClicked, new Rect(0, 0, GUIController.Instance.images["ButtonRect"].width, GUIController.Instance.images["ButtonRect"].height), GUIController.Instance.trajanBold, "HP Bars");

        panel.FixRenderOrder();
    }

    private static void DelClicked(int index)
    {
        if (index <= enemyPool.Count)
        {
            EnemyHandle handle = enemyPool[index - 1];
            Object.DestroyImmediate(handle.gameObject);

            Console.AddLine($"Destroying enemy: {handle.gameObject.name}");
        }
    }

    private static void CloneClicked(int index)
    {
        if (index <= enemyPool.Count)
        {
            EnemyHandle handle = enemyPool[index - 1];
            GameObject gameObject2 = Object.Instantiate(handle.gameObject, handle.transform.position, handle.transform.rotation);

            Console.AddLine($"Cloning enemy as: {gameObject2.name}");
        }
    }

    private static void InfClicked(int index)
    {
        if (index <= enemyPool.Count)
        {
            EnemyHandle handle = enemyPool[index - 1];
            handle.HP = 9999;

            Console.AddLine($"HP for enemy: {handle.gameObject.name} is now 9999");
        }
    }

    private static void HPBarsClicked() => BindableFunctions.ToggleEnemyHPBars();
    
    public static void Update()
    {
        if (panel == null)
        {
            return;
        }

        if (GUIController.ForceHideUI())
        {
            panel.ActiveSelf = false;
        }
        else
        {
            panel.ActiveSelf = DebugMod.settings.EnemiesPanelVisible;

            if (DebugMod.settings.EnemiesPanelVisible && UIManager.instance.uiState == UIState.PLAYING &&
                (panel.GetPanel("Pause").ActiveInHierarchy || !panel.GetPanel("Play").ActiveInHierarchy))
            {
                panel.GetPanel("Pause").ActiveSelf = false;
                panel.GetPanel("Play").ActiveSelf = true;
            }
            else if (DebugMod.settings.EnemiesPanelVisible && UIManager.instance.uiState == UIState.PAUSED &&
                     (!panel.GetPanel("Pause").ActiveInHierarchy || panel.GetPanel("Play").ActiveInHierarchy))
            {
                panel.GetPanel("Pause").ActiveSelf = true;
                panel.GetPanel("Play").ActiveSelf = false;
            }
        }

        enemyPool.RemoveAll(handle => !handle && !handle.gameObject.activeSelf);

        if (panel.ActiveInHierarchy)
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

            if (panel.GetPanel("Pause").ActiveInHierarchy)
            {
                for (int i = 1; i <= 14; i++)
                {
                    if (i <= enemyCount)
                    {
                        panel.GetPanel("Pause").GetButton("Del" + i).ActiveSelf = true;
                        panel.GetPanel("Pause").GetButton("Clone" + i).ActiveSelf = true;
                        panel.GetPanel("Pause").GetButton("Inf" + i).ActiveSelf = true;
                    }
                    else
                    {
                        panel.GetPanel("Pause").GetButton("Del" + i).ActiveSelf = false;
                        panel.GetPanel("Pause").GetButton("Clone" + i).ActiveSelf = false;
                        panel.GetPanel("Pause").GetButton("Inf" + i).ActiveSelf = false;
                    }
                }

                panel.GetPanel("Pause").GetButton("HP Bars")
                    .SetTextColor(hpBars ? new Color(244f / 255f, 127f / 255f, 32f / 255f) : Color.white);
            }

            if (enemyCount > 14)
            {
                enemyNames += "And " + (enemyCount - 14) + " more";
            }

            panel.GetText("Enemy Names").UpdateText(enemyNames);
            panel.GetText("Enemy HP").UpdateText(enemyHP);
        }
    }

    public static bool IsActive()
    {
        return HeroController.instance && !HeroController.instance.cState.transitioning && GameManager.instance.IsGameplayScene();
    }
}
