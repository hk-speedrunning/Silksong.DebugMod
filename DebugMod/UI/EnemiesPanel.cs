using System;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod.UI
{
    public static class EnemiesPanel
    {
        private static CanvasPanel panel;
        private static float lastTime;
        public static List<EnemyData> enemyPool = new List<EnemyData>();

        public static GameObject parent { get; private set; }
        public static bool hpBars;

        public static void BuildMenu(GameObject canvas)
        {
            parent = canvas;

            panel = new CanvasPanel(canvas, new Texture2D(1, 1), new Vector2(1920f - GUIController.Instance.images["EnemiesPBg"].width, 481f), Vector2.zero, new Rect(0, 0, 1, 1));

            panel.AddText("Panel Label", "Enemies", new Vector2(125f, -25f), Vector2.zero, GUIController.Instance.trajanBold, 30);

            panel.AddText("Enemy Names", "", new Vector2(90f, 20f), Vector2.zero, GUIController.Instance.arial);
            panel.AddText("Enemy HP", "", new Vector2(300f, 20f), Vector2.zero, GUIController.Instance.arial);

            panel.AddPanel("Pause", GUIController.Instance.images["EnemiesPBg"], Vector2.zero, Vector2.zero, new Rect(0, 0, GUIController.Instance.images["EnemiesPBg"].width, GUIController.Instance.images["EnemiesPBg"].height));
            panel.AddPanel("Play", GUIController.Instance.images["EnemiesBg"], new Vector2(57f, 0f), Vector2.zero, new Rect(0f, 0f, GUIController.Instance.images["EnemiesBg"].width, GUIController.Instance.images["EnemiesBg"].height));

            for (int i = 1; i <= 14; i++)
            {
                panel.GetPanel("Pause").AddButton("Del" + i, GUIController.Instance.images["ButtonDel"], new Vector2(20f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), DelClicked, new Rect(0, 0, GUIController.Instance.images["ButtonDel"].width, GUIController.Instance.images["ButtonDel"].height));
                panel.GetPanel("Pause").AddButton("Clone" + i, GUIController.Instance.images["ButtonPlus"], new Vector2(40f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), CloneClicked, new Rect(0, 0, GUIController.Instance.images["ButtonPlus"].width, GUIController.Instance.images["ButtonPlus"].height));
                panel.GetPanel("Pause").AddButton("Inf" + i, GUIController.Instance.images["ButtonInf"], new Vector2(60f, 20f + (i - 1) * 15f), new Vector2(12f, 12f), InfClicked, new Rect(0, 0, GUIController.Instance.images["ButtonInf"].width, GUIController.Instance.images["ButtonInf"].height));
            }

            panel.GetPanel("Pause").AddButton("HP Bars", GUIController.Instance.images["ButtonRect"], new Vector2(30f, 250f), Vector2.zero, HPBarsClicked, new Rect(0, 0, GUIController.Instance.images["ButtonRect"].width, GUIController.Instance.images["ButtonRect"].height), GUIController.Instance.trajanBold, "HP Bars");

            panel.FixRenderOrder();
        }

        private static void DelClicked(string buttonName)
        {
            int num = Convert.ToInt32(buttonName.Substring(3));
            EnemyData dat = enemyPool.FindAll(ed => ed.gameObject?.activeSelf == true)[num - 1];

            Console.AddLine("Destroying enemy: " + dat.gameObject.name);
            Object.DestroyImmediate(dat.gameObject);
        }

        private static void CloneClicked(string buttonName)
        {
            int num = Convert.ToInt32(buttonName.Substring(5));
            EnemyData dat = enemyPool.FindAll(ed => ed.gameObject?.activeSelf == true)[num - 1];

            GameObject gameObject2 = Object.Instantiate(dat.gameObject, dat.gameObject.transform.position, dat.gameObject.transform.rotation);
            enemyPool.Add(new EnemyData(gameObject2, parent));
            Console.AddLine("Cloning enemy as: " + gameObject2.name);
        }

        private static void InfClicked(string buttonName)
        {
            int num = Convert.ToInt32(buttonName.Substring(3));
            EnemyData dat = enemyPool.FindAll(ed => ed.gameObject != null && ed.gameObject.activeSelf)[num - 1];

            dat.SetHP(9999);
            Console.AddLine("HP for enemy: " + dat.gameObject.name + " is now 9999");
        }

        private static void HPBarsClicked(string buttonName) => BindableFunctions.ToggleEnemyHPBars();
        
        public static void Update()
        {
            if (panel == null)
            {
                return;
            }

            if (GUIController.ForceHideUI())
            {
                if (panel.active)
                {
                    panel.SetActive(false, true);
                }
            }
            else
            {
                if (DebugMod.settings.EnemiesPanelVisible && !panel.active)
                {
                    panel.SetActive(true, false);
                }
                else if (!DebugMod.settings.EnemiesPanelVisible && panel.active)
                {
                    panel.SetActive(false, true);
                }

                if (DebugMod.settings.EnemiesPanelVisible && UIManager.instance.uiState == UIState.PLAYING &&
                    (panel.GetPanel("Pause").active || !panel.GetPanel("Play").active))
                {
                    panel.GetPanel("Pause").SetActive(false, true);
                    panel.GetPanel("Play").SetActive(true, false);
                }
                else if (DebugMod.settings.EnemiesPanelVisible && UIManager.instance.uiState == UIState.PAUSED &&
                         (!panel.GetPanel("Pause").active || panel.GetPanel("Play").active))
                {
                    panel.GetPanel("Pause").SetActive(true, false);
                    panel.GetPanel("Play").SetActive(false, true);
                }
            }

            if (panel.active || hpBars)
            {
                CheckForAutoUpdate();

                string enemyNames = "";
                string enemyHP = "";
                int enemyCount = 0;

                for (int i = 0; i < enemyPool.Count; i++)
                {
                    EnemyData dat = enemyPool[i];
                    GameObject obj = dat.gameObject;

                    if (obj == null || !obj.activeSelf)
                    {
                        if (obj == null)
                        {
                            dat.hpBar.Destroy();
                            dat.hitbox.Destroy();
                            enemyPool.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            dat.hpBar.SetPosition(new Vector2(-1000f, -1000f));
                        }
                    }
                    else
                    {
                        int hp = dat.HM.hp;

                        if (hp != dat.HP)
                        {
                            dat.SetHP(hp);

                            if (hpBars)
                            {
                                Texture2D tex = new Texture2D(120, 40);

                                for (int x = 0; x < 120; x++)
                                {
                                    for (int y = 0; y < 40; y++)
                                    {
                                        if (x < 3 || x > 116 || y < 3 || y > 36)
                                        {
                                            tex.SetPixel(x, y, Color.black);
                                        }
                                        else
                                        {
                                            if ( hp / (float) dat.MaxHP >= (x - 2f) / 117f)
                                            {
                                                tex.SetPixel(x, y, Color.red);
                                            }
                                            else
                                            {
                                                tex.SetPixel(x, y, new Color(255f, 255f, 255f, 0f));
                                            }
                                        }
                                    }
                                }

                                tex.Apply();

                                dat.hpBar.UpdateBackground(tex, new Rect(0, 0, 120, 40));
                            }
                        }

                        if (hpBars)
                        {
                            Vector2 enemyPos = Camera.main.WorldToScreenPoint(obj.transform.position);
                            enemyPos.x *= 1920f / Screen.width;
                            enemyPos.y *= 1080f / Screen.height;

                            enemyPos.y = 1080f - enemyPos.y;

                            Bounds bounds = dat.GetBounds();
                            enemyPos.y -= (Camera.main.WorldToScreenPoint(bounds.max).y * (1080f / Screen.height) -
                                           Camera.main.WorldToScreenPoint(bounds.min).y * (1080f / Screen.height)) / 2f;
                            enemyPos.x -= 60;

                            dat.hpBar.SetPosition(enemyPos);
                            dat.hpBar.GetText("HP").UpdateText(dat.HM.hp + "/" + dat.MaxHP);

                            if (!dat.hpBar.active)
                            {
                                dat.hpBar.SetActive(true, true);
                            }
                        }

                        if (!hpBars && dat.hpBar.active)
                        {
                            dat.hpBar.SetActive(false, true);
                        }

                        if (++enemyCount <= 14)
                        {
                            enemyNames += obj.name + "\n";
                            enemyHP += dat.HM.hp + "/" + dat.MaxHP + "\n";
                        }
                    }
                }

                if (panel.GetPanel("Pause").active)
                {
                    for (int i = 1; i <= 14; i++)
                    {
                        if (i <= enemyCount)
                        {
                            panel.GetPanel("Pause").GetButton("Del" + i).SetActive(true);
                            panel.GetPanel("Pause").GetButton("Clone" + i).SetActive(true);
                            panel.GetPanel("Pause").GetButton("Inf" + i).SetActive(true);
                        }
                        else
                        {
                            panel.GetPanel("Pause").GetButton("Del" + i).SetActive(false);
                            panel.GetPanel("Pause").GetButton("Clone" + i).SetActive(false);
                            panel.GetPanel("Pause").GetButton("Inf" + i).SetActive(false);
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
            else
            {
                Reset();
            }
        }

        public static void Reset()
        {
            foreach(EnemyData dat in enemyPool)
            {
                dat.hitbox.Destroy();
                dat.hpBar.Destroy();
            }
            enemyPool.Clear();
        }

        public static bool Ignore(string name)
        {
            if (name.IndexOf("Hornet Barb", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Needle Tink", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("worm", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Laser Turret", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (name.IndexOf("Deep Spikes", StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return false;
        }

        private static void CheckForAutoUpdate()
        {
            float deltaTime = Time.realtimeSinceStartup - lastTime;

            if (deltaTime >= 2f)
            {
                EnemyUpdate();
            }
        }

        public static void EnemyUpdate()
        {
            lastTime = Time.realtimeSinceStartup;

            if (HeroController.instance != null && !HeroController.instance.cState.transitioning && DebugMod.GM.IsGameplayScene())
            {
                int count = enemyPool.Count;
                
                foreach (HealthManager healthManager in Object.FindObjectsByType<HealthManager>(FindObjectsSortMode.None))
                {
                    if (healthManager.gameObject.layer == (int)PhysLayers.ENEMIES &&
                        enemyPool.All(ed => ed.gameObject != healthManager.gameObject) && !Ignore(healthManager.gameObject.name))
                    {
                        enemyPool.Add(new EnemyData(healthManager.gameObject, parent));
                    }
                }

                if (enemyPool.Count > count)
                {
                    Console.AddLine("EnemyList updated: +" + (enemyPool.Count - count));
                }
            }
        }
    }
}
