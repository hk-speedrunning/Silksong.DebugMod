using System;
using System.Collections.Generic;
using DebugMod.Canvas;
using GlobalSettings;
using UnityEngine;

namespace DebugMod.InfoPanels
{
    public class CustomInfoPanel : TogglableInfoPanel
    {
        public bool ShowSprite;
        public CustomInfoPanel(string Name, bool ShowSprite) : base(Name)
        {
            this.ShowSprite = ShowSprite;
        }

        protected List<(float xLabel, float xInfo, float y, string label, Func<string> textFunc)> PanelBuildInfo = new();
        private Dictionary<string, Func<string>> UpdateActions;
        public override void BuildPanel(GameObject canvas)
        {
            if (ShowSprite)
            {
                panel = new CanvasPanel(
                    canvas,
                    GUIController.Instance.images["StatusPanelBG"],
                    new Vector2(0f, 223f),
                    Vector2.zero,
                    new Rect(
                        0f,
                        0f,
                        GUIController.Instance.images["StatusPanelBG"].width,
                        GUIController.Instance.images["StatusPanelBG"].height));
            }
            else
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadRawTextureData(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                tex.Apply();

                // Puke
                panel = new CanvasPanel(
                    canvas,
                    tex,
                    new Vector2(130f, 230f),
                    Vector2.zero,
                    new Rect(
                        0f,
                        0f,
                        1f,
                        1f));
            }


            UpdateActions = new();
            int counter = 0;

            foreach ((float xLabel, float xInfo, float y, string label, Func<string> textFunc) in PanelBuildInfo)
            {
                panel.AddText($"Label-{counter++}", label, new Vector2(xLabel, y), Vector2.zero, GUIController.Instance.arial, 15);
                panel.AddText($"Info-{counter}", "", new Vector2(xInfo, y + 4f), Vector2.zero, GUIController.Instance.trajanNormal);
                UpdateActions.Add($"Info-{counter}", textFunc);
            }

            panel.FixRenderOrder();
        }
        public override void UpdatePanel()
        {
            if (panel == null) return;

            foreach (var kvp in UpdateActions)
            {
                panel.GetText(kvp.Key).UpdateText(kvp.Value.Invoke());
            }
        }

        public void AddInfo(float xLabel, float xInfo, float y, string label, Func<string> textFunc)
            => PanelBuildInfo.Add((xLabel, xInfo, y, label, textFunc));

        internal static CustomInfoPanel GetMainInfoPanel()
        {
            CustomInfoPanel MainInfoPanel = new CustomInfoPanel(InfoPanel.MainInfoPanelName, true);

            float y = 0f;

            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Hero State", () => HeroController.instance.hero_state.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Velocity", () => HeroController.instance.current_velocity.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "HP", () => PlayerData.instance.health + " / " + PlayerData.instance.maxHealth);
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "MP", () => PlayerData.instance.silk + " / " + PlayerData.instance.CurrentSilkMaxBasic);
            y += 25f;
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Completion", () => PlayerData.instance.completionPercentage + "%");
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Fleas", () => Gameplay.FleasCollectedCount + " / 30");
            MainInfoPanel.AddInfo(10, 150f, y += 20, "Quest Points", () => QuestManager.GetQuest("Soul Snare Pre").requiredCompleteTotalGroups[0].CurrentValueCount + " / 17");
            y += 25f;
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Needle Base", () => PlayerData.instance.nailDamage.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Last Damage", () => DebugMod.lastHit != null ?
                $"{DebugMod.lastDamage} ({DebugMod.lastHit?.DamageDealt} x {DebugMod.lastHit?.Multiplier})" : "");
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Last Type", () => DebugMod.lastHit?.AttackType.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Last Scaling", () => DebugMod.lastHit != null ? DebugMod.lastScaling.ToString() : "");
            y += 25f;
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Invulnerable", () => GetStringForBool(HeroController.instance.cState.Invulnerable));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Invincible", () => GetStringForBool(PlayerData.instance.isInvincible));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Damage State", () => HeroController.instance.damageMode.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Dead State", () => GetStringForBool(HeroController.instance.cState.dead));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Hazard Death", () => GetStringForBool(HeroController.instance.cState.hazardDeath));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Cocoon Scene", () => string.IsNullOrEmpty(PlayerData.instance.HeroCorpseScene) ? "None" : PlayerData.instance.HeroCorpseScene.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Cocoon Position", () => string.IsNullOrEmpty(PlayerData.instance.HeroCorpseScene) ? "None" : PlayerData.instance.HeroDeathScenePos.ToString());
            y += 25f;
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Scene Name", () => DebugMod.GetSceneName());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Transition", () => GetStringForBool(HeroController.instance.cState.transitioning));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Trans State", () => GetTransState());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Game State", () => GameManager.instance.GameState.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "UI State", () => UIManager.instance.uiState.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Camera Mode", () => DebugMod.RefCamera.mode.ToString());

            y = 10f;

            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Accept Input", () => GetStringForBool(HeroController.instance.acceptingInput));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Relinquished", () => GetStringForBool(HeroController.instance.controlReqlinquished));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "At Bench", () => GetStringForBool(PlayerData.instance.atBench));
            y += 20f;
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Dashing", () => GetStringForBool(HeroController.instance.cState.dashing));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Sprinting", () => GetStringForBool(HeroController.instance.cState.isSprinting));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Jumping", () => GetStringForBool((HeroController.instance.cState.jumping || HeroController.instance.cState.doubleJumping)));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Super Jumping", () => GetStringForBool(HeroController.instance.cState.superDashing));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Falling", () => GetStringForBool(HeroController.instance.cState.falling));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Hardland", () => GetStringForBool(HeroController.instance.cState.willHardLand));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Swimming", () => GetStringForBool(HeroController.instance.cState.swimming));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Recoiling", () => GetStringForBool(HeroController.instance.cState.recoiling));
            y += 20f;
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall Lock", () => GetStringForBool(HeroController.instance.wallLocked));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall Jumping", () => GetStringForBool(HeroController.instance.cState.wallJumping));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall Touching", () => GetStringForBool(HeroController.instance.cState.touchingWall));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall Sliding", () => GetStringForBool(HeroController.instance.cState.wallSliding));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall Left", () => GetStringForBool(HeroController.instance.touchingWallL));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall Right", () => GetStringForBool(HeroController.instance.touchingWallR));
            y += 20f;
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Attacking", () => GetStringForBool(HeroController.instance.cState.attacking));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Can Cast", () => GetStringForBool(HeroController.instance.CanCast()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Can Superjump", () => GetStringForBool(HeroController.instance.CanSuperJump()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Can Quickmap", () => GetStringForBool(HeroController.instance.CanQuickMap()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Can Inventory", () => GetStringForBool(HeroController.instance.CanOpenInventory()));
            y += 20f;
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Is Gameplay", () => GetStringForBool(DebugMod.GM.IsGameplayScene()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Hero Paused", () => GetStringForBool(HeroController.instance.cState.isPaused));

            return MainInfoPanel;
        }

        internal static CustomInfoPanel GetMinimalInfoPanel()
        {
            CustomInfoPanel MinimalInfoPanel = new CustomInfoPanel(InfoPanel.MinimalInfoPanelName, false);

            MinimalInfoPanel.AddInfo(10, 40, 10, "Vel", () => HeroController.instance.current_velocity.ToString());
            MinimalInfoPanel.AddInfo(110, 140, 10, "Pos", () => GetHeroPos());
            
            MinimalInfoPanel.AddInfo(10, 40, 30, "MP", () => (PlayerData.instance.silk).ToString());
            
            MinimalInfoPanel.AddInfo(10, 100, 50, "NailDmg", () => PlayerData.instance.nailDamage.ToString());
            
            MinimalInfoPanel.AddInfo(10, 95, 70, "Completion", () => PlayerData.instance.completionPercentage + "%");
            MinimalInfoPanel.AddInfo(140, 195, 70, "Fleas", () => PlayerData.instance.SavedFleasCount + " / 30");
            
            MinimalInfoPanel.AddInfo(10, 140, 90, "Scene Name", () => DebugMod.GetSceneName());
            MinimalInfoPanel.AddInfo(10, 140, 110, "Current SaveState", () => SaveStateManager.quickState.IsSet() ? SaveStateManager.quickState.GetSaveStateID() : "No savestate");
            MinimalInfoPanel.AddInfo(110, 200, 130, "Current slot", GetCurrentSlotString);
            MinimalInfoPanel.AddInfo(10, 80, 130, "Hardfall", () => GetStringForBool(HeroController.instance.cState.willHardLand));

            return MinimalInfoPanel;
        }

        private static string GetCurrentSlotString()
        {
            string slotSet = SaveStateManager.GetCurrentSlot().ToString();
            if (slotSet == "-1")
            {
                slotSet = "unset";
            }
            return slotSet;
        }
    }
}
