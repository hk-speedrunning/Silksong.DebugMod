using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DebugMod.Hitbox;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod
{
    public class GUIController : MonoBehaviour
    {
        public Font trajanBold;
        public Font trajanNormal;
        public Font arial;
        public Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();
        public Vector3 hazardLocation;
        public string respawnSceneWatch;
        public static bool didInput, inputEsc;
        private static readonly HitboxViewer hitboxes = new();

        public GameObject canvas;
        private static GUIController _instance;

        private readonly Array allKeyCodes = Enum.GetValues(typeof(KeyCode));

        private readonly List<KeyCode> UnbindableKeys = new List<KeyCode>()
        {
            KeyCode.Mouse0
        };

        public static GUIController Instance
        {
            get
            {
                if (_instance == null)
                {
                    DebugMod.instance.Log("Creating new GUIController");

                    GameObject GUIObj = new GameObject("GUIController");
                    _instance = GUIObj.AddComponent<GUIController>();
                    DontDestroyOnLoad(GUIObj);
                }
                return _instance;
            }
        }

        /// <summary>
        /// If this returns true, all DebugMod UI elements will be hidden.
        /// </summary>
        public static bool ForceHideUI()
        {
            return DebugMod.GM.IsNonGameplayScene()
                && !DebugMod.GM.IsCinematicScene(); // Show UI in cutscenes
        }

        public void Awake()
        {
            hazardLocation = PlayerData.instance.hazardRespawnLocation;
            respawnSceneWatch = PlayerData.instance.respawnScene;
        }

        public void BuildMenus()
        {
            LoadResources();

            canvas = new GameObject("DebugModCanvas");
            canvas.AddComponent<UnityEngine.Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvas.AddComponent<GraphicRaycaster>();

            SaveStatesPanel.BuildMenu(canvas);
            TopMenu.BuildMenu(canvas);
            EnemiesPanel.BuildMenu(canvas);
            Console.BuildMenu(canvas);

            InfoPanel.BuildInfoPanels(canvas);
            KeyBindPanel.BuildMenu(canvas);

            DontDestroyOnLoad(canvas);
        }

        private void LoadResources()
        {
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == "TrajanPro-Bold")
                {
                    trajanBold = f;
                }

                if (f != null && f.name == "TrajanPro-Regular")
                {
                    trajanNormal = f;
                }

                //Just in case for some reason the computer doesn't have arial
                if (f != null && f.name == "Perpetua")
                {
                    arial = f;
                }

                foreach (string font in Font.GetOSInstalledFontNames())
                {
                    if (font.ToLower().Contains("arial"))
                    {
                        arial = Font.CreateDynamicFontFromOSFont(font, 13);
                        break;
                    }
                }
            }

            if (trajanBold == null || trajanNormal == null || arial == null) DebugMod.instance.LogError("Could not find game fonts");

            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (string res in resourceNames)
            {
                if (res.StartsWith("DebugMod.Images."))
                {
                    try
                    {
                        Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                        byte[] buffer = new byte[imageStream.Length];
                        imageStream.Read(buffer, 0, buffer.Length);

                        Texture2D tex = new Texture2D(1, 1);
                        tex.LoadImage(buffer.ToArray());

                        string[] split = res.Split('.');
                        string internalName = split[split.Length - 2];
                        images.Add(internalName, tex);

                        DebugMod.instance.LogDebug("Loaded image: " + internalName);
                    }
                    catch (Exception e)
                    {
                        DebugMod.instance.LogError("Failed to load image: " + res + "\n" + e.ToString());
                    }
                }
            }
        }

        public void Update()
        {
            if (DebugMod.GM == null) return;

            SaveStatesPanel.Update();
            TopMenu.Update();
            EnemiesPanel.Update();
            Console.Update();
            KeyBindPanel.Update();
            InfoPanel.Update();
            
            if (DebugMod.GetSceneName() == "Menu_Title") return;
            
            // If the mouse is visible, then make sure it can be used.
            // Normally, allowMouseInput is false until first pause and then true from then on (even when not paused)
            if (DebugMod.settings.ShowCursorWhileUnpaused && !ForceHideUI() && !UIManager.instance.inputModule.allowMouseInput)
            {
                InputHandler.Instance.StartUIInput();
            }

            //Handle keybinds
            //foreach (KeyValuePair<string, KeyCode> bind in DebugMod.settings.binds)
            for(int i = 0; i < DebugMod.settings.binds.Count; i++)
            {
                var bind = DebugMod.settings.binds.ElementAt(i);
                string bindName = bind.Key;
                KeyCode bindKeyCode  = bind.Value;
                
                if (DebugMod.bindMethods.ContainsKey(bindName) || DebugMod.AdditionalBindMethods.ContainsKey(bindName))
                {
                    //check for keys that are waiting to be bound
                    if (bindKeyCode == KeyCode.None)
                    {
                        foreach (KeyCode kc in allKeyCodes)
                        {
                            if (Input.GetKeyDown(kc) && !UnbindableKeys.Contains(kc))
                            {
                                if (KeyBindPanel.keyWarning != kc)
                                {
                                    foreach (string method in DebugMod.bindMethods.Keys.Concat(DebugMod.AdditionalBindMethods.Keys))
                                    {
                                        if (DebugMod.settings.binds.TryGetValue(method, out KeyCode key) && key == kc)
                                        {
                                            Console.AddLine($"{kc} already bound to {method}, press again to confirm");
                                            KeyBindPanel.keyWarning = kc;
                                        }
                                    }

                                    if (KeyBindPanel.keyWarning == kc) break;
                                }

                                KeyBindPanel.keyWarning = KeyCode.None;

                                //remove bind
                                if (kc == KeyCode.Escape)
                                {
                                    DebugMod.settings.binds.Remove(bindName);
                                    i--;
                                    DebugMod.instance.LogWarn($"The key {Enum.GetName(typeof(KeyCode),kc)} has been unbound from {bindName}");
                                }
                                else if (kc != KeyCode.Escape)
                                {
                                    DebugMod.settings.binds[bindName] = kc;
                                }

                                KeyBindPanel.UpdateHelpText();
                                break;
                            }
                        }
                    }
                    else if (Input.GetKeyDown(bindKeyCode))
                    {
                        //This makes sure atleast you can close the UI when the KeyBindLock is active.
                        //Im sure theres a better way to do this but idk. 
                        try
                        {
                            //cat, allowLock, the method
                            (string, bool, Action) methodData;
                            
                            if (DebugMod.bindMethods.TryGetValue(bindName, out methodData) 
                                || DebugMod.AdditionalBindMethods.TryGetValue(bindName, out methodData))
                            {
                                //run if not locked or locked but bind doesnt allow locks
                                if (!DebugMod.KeyBindLock || DebugMod.KeyBindLock && !methodData.Item2)
                                {
                                    methodData.Item3.Invoke();
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            DebugMod.instance.LogError("Error running keybind method " + bindName + ":\n" +
                                                       e.ToString());
                        }
                        
                    }
                }
            }
            
            if (SaveStateManager.inSelectSlotState && DebugMod.settings.SaveStatePanelVisible)
            {
                foreach (KeyValuePair<KeyCode, int> entry in DebugMod.alphaKeyDict)
                {
                    
                    if (Input.GetKeyDown(entry.Key))
                    {
                        if (DebugMod.alphaKeyDict.TryGetValue(entry.Key, out int keyInt))
                        {
                            // keyInt should be between 0-9
                            SaveStateManager.currentStateSlot = keyInt;
                            didInput = true;
                            break;
                        }
                        else
                        {
                            didInput = inputEsc = true;
                            break;
                        }
                    }
                }
            }

            if (DebugMod.infiniteSilk
                && PlayerData.instance.silk < PlayerData.instance.silkMax
                && PlayerData.instance.health > 0
                && HeroController.instance != null
                && !HeroController.instance.cState.dead
                && GameManager.instance.IsGameplayScene())
            {
                PlayerData.instance.silk = PlayerData.instance.silkMax;
                HeroController.instance.AddSilk(1, false);
            }

            if (DebugMod.infiniteTools && ToolItemManager.Instance && ToolItemManager.Instance.toolItems)
            {
                foreach (ToolItem tool in ToolItemManager.Instance.toolItems)
                {
                    if (tool)
                    {
                        ToolItemsData.Data data = tool.SavedData;
                        int oldAmount = data.AmountLeft;
                        data.AmountLeft = ToolItemManager.GetToolStorageAmount(tool);
                        tool.SavedData = data;

                        AttackToolBinding? binding = ToolItemManager.GetAttackToolBinding(tool);
                        if (binding.HasValue && oldAmount != data.AmountLeft)
                        {
                            ToolItemManager.ReportBoundAttackToolUpdated(binding.Value);
                        }
                    }
                }
            }

            if (DebugMod.playerInvincible && PlayerData.instance != null)
            {
                PlayerData.instance.isInvincible = true;
            }

            if (DebugMod.noclip)
            {
                Vector3 offset = Vector3.zero;
                float baseSpeed = Input.GetKey(KeyCode.LeftShift) ? 40f : 20f;
                float distance = baseSpeed * DebugMod.settings.NoClipSpeedModifier * Time.deltaTime;

                if (DebugMod.IH.inputActions.Left.IsPressed)
                {
                    offset += Vector3.left * distance;
                }

                if (DebugMod.IH.inputActions.Right.IsPressed)
                {
                    offset += Vector3.right * distance;
                }

                if (DebugMod.IH.inputActions.Up.IsPressed)
                {
                    offset += Vector3.up * distance;
                }

                if (DebugMod.IH.inputActions.Down.IsPressed)
                {
                    offset += Vector3.down * distance;
                }

                DebugMod.noclipPos += offset;

                if (HeroController.instance.transitionState == GlobalEnums.HeroTransitionState.WAITING_TO_TRANSITION && SaveState.loadingSavestate == null)
                {
                    DebugMod.RefKnight.transform.position = DebugMod.noclipPos;
                    DebugMod.RefKnight.GetComponent<Rigidbody2D>().constraints |= RigidbodyConstraints2D.FreezePosition;
                }
                else
                {
                    DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
                    DebugMod.RefKnight.GetComponent<Rigidbody2D>().constraints &= ~RigidbodyConstraints2D.FreezePosition;
                }
            }

            if (DebugMod.cameraFollow)
            {
                BindableFunctions.cameraGameplayScene.SetValue(DebugMod.RefCamera, false);
                DebugMod.RefCamera.SnapTo(DebugMod.RefKnight.transform.position.x, DebugMod.RefKnight.transform.position.y);
            }

            if (PlayerData.instance.hazardRespawnLocation != hazardLocation)
            {
                hazardLocation = PlayerData.instance.hazardRespawnLocation;
                Console.AddLine("Hazard Respawn location updated: " + hazardLocation.ToString());

                if (DebugMod.settings.EnemiesPanelVisible)
                {
                    EnemiesPanel.EnemyUpdate();
                }
            }
            if (!string.IsNullOrEmpty(respawnSceneWatch) && respawnSceneWatch != PlayerData.instance.respawnScene)
            {
                respawnSceneWatch = PlayerData.instance.respawnScene;
                Console.AddLine(string.Concat(new string[]
                {
                    "Save Respawn updated, new scene: ",
                    PlayerData.instance.respawnScene.ToString(),
                    ", Map Zone: ",
                    GameManager.instance.GetCurrentMapZone(),
                    ", Respawn Marker: ",
                    PlayerData.instance.respawnMarkerName.ToString()
                }));
            }
            if (HitboxViewer.State != DebugMod.settings.ShowHitBoxes)
            {
                if (DebugMod.settings.ShowHitBoxes != 0)
                {
                    hitboxes.Load();
                }
                else if (HitboxViewer.State != 0 && DebugMod.settings.ShowHitBoxes == 0)
                {
                    hitboxes.Unload();
                }
            }
        }
    }
}
