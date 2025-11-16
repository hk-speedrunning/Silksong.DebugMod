using System;
using System.Collections.Generic;
using System.Linq;
using DebugMod.Hitbox;
using DebugMod.SaveStates;
using DebugMod.UI;
using DebugMod.UI.Canvas;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod;

public class GUIController : MonoBehaviour
{
    public Vector3 hazardLocation;
    public string respawnSceneWatch;
    private static readonly HitboxViewer hitboxes = new();
    private KeyCode keyWarning;
    private Resolution resolution;

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
                DebugMod.Log("Creating new GUIController");

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
        if (canvas)
        {
            Destroy(canvas);
        }

        canvas = new GameObject("DebugModCanvas");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<GraphicRaycaster>();

        DontDestroyOnLoad(canvas);

        MainPanel.BuildPanel();
        EnemiesPanel.BuildPanel();
        ConsolePanel.BuildPanel();
        InfoPanel.BuildPanel();
        SaveStatesPanel.BuildPanel();
        KeybindContextPanel.BuildPanel();

        resolution = Screen.currentResolution;
    }

    public void Update()
    {
        if (DebugMod.GM == null) return;

        if (resolution.width != Screen.currentResolution.width || resolution.height != Screen.currentResolution.height)
        {
            resolution = Screen.currentResolution;
            BuildMenus();
        }

        MainPanel.Instance.ActiveSelf = DebugMod.settings.MainPanelVisible;
        EnemiesPanel.Instance.ActiveSelf = DebugMod.settings.EnemiesPanelVisible;
        ConsolePanel.Instance.ActiveSelf = DebugMod.settings.ConsoleVisible;
        InfoPanel.Instance.ActiveSelf = DebugMod.settings.InfoPanelVisible;
        SaveStatesPanel.Instance.ActiveSelf = SaveStatesPanel.ShouldBeVisible;

        foreach (CanvasNode root in CanvasNode.rootNodes)
        {
            if (ForceHideUI())
            {
                root.ActiveSelf = false;
            }
            else if (root.ActiveSelf)
            {
                root.Update();
            }
        }

        if (DebugMod.GetSceneName() == "Menu_Title") return;
        
        // If the mouse is visible, then make sure it can be used.
        // Normally, allowMouseInput is false until first pause and then true from then on (even when not paused)
        if (DebugMod.settings.ShowCursorWhileUnpaused && !ForceHideUI() && !UIManager.instance.inputModule.allowMouseInput)
        {
            InputHandler.Instance.StartUIInput();
        }

        if (!CanvasTextField.AnyFieldFocused)
        {
            HandleKeybinds();
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
            DebugMod.RefCamera.isGameplayScene = false;
            DebugMod.RefCamera.SnapTo(DebugMod.RefKnight.transform.position.x, DebugMod.RefKnight.transform.position.y);
        }

        if (PlayerData.instance.hazardRespawnLocation != hazardLocation)
        {
            hazardLocation = PlayerData.instance.hazardRespawnLocation;
            DebugMod.LogConsole("Hazard Respawn location updated: " + hazardLocation.ToString());
        }
        if (!string.IsNullOrEmpty(respawnSceneWatch) && respawnSceneWatch != PlayerData.instance.respawnScene)
        {
            respawnSceneWatch = PlayerData.instance.respawnScene;
            DebugMod.LogConsole(string.Concat(new string[]
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

    private void HandleKeybinds()
    {
        for (int i = 0; i < DebugMod.settings.binds.Count; i++)
        {
            var bind = DebugMod.settings.binds.ElementAt(i);
            string bindName = bind.Key;
            KeyCode bindKeyCode = bind.Value;

            if (DebugMod.bindActions.ContainsKey(bindName))
            {
                //check for keys that are waiting to be bound
                if (bindKeyCode == KeyCode.None)
                {
                    foreach (KeyCode kc in allKeyCodes)
                    {
                        if (Input.GetKeyDown(kc) && !UnbindableKeys.Contains(kc))
                        {
                            if (keyWarning != kc)
                            {
                                foreach (string method in DebugMod.bindActions.Keys)
                                {
                                    if (DebugMod.settings.binds.TryGetValue(method, out KeyCode key) && key == kc)
                                    {
                                        DebugMod.LogConsole($"{kc} already bound to {method}, press again to confirm");
                                        keyWarning = kc;
                                    }
                                }

                                if (keyWarning == kc) break;
                            }

                            keyWarning = KeyCode.None;

                            //remove bind
                            if (kc == KeyCode.Escape)
                            {
                                DebugMod.settings.binds.Remove(bindName);
                                i--;
                                DebugMod.LogWarn($"The key {Enum.GetName(typeof(KeyCode), kc)} has been unbound from {bindName}");
                            }
                            else if (kc != KeyCode.Escape)
                            {
                                DebugMod.settings.binds[bindName] = kc;
                            }

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
                        BindAction action;

                        if (DebugMod.bindActions.TryGetValue(bindName, out action))
                        {
                            //run if not locked or locked but bind doesnt allow locks
                            if (!DebugMod.KeyBindLock || DebugMod.KeyBindLock && !action.AllowLock)
                            {
                                action.Action.Invoke();
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        DebugMod.LogError("Error running keybind method " + bindName + ":\n" +
                                                   e.ToString());
                    }

                }
            }
        }
    }
}
