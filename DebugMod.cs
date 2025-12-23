using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using DebugMod.Helpers;
using DebugMod.SaveStates;
using DebugMod.UI;
using GlobalEnums;
using HarmonyLib;
using JetBrains.Annotations;
using MonoMod.ModInterop;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMod;

[BepInAutoPlugin("io.github.hk-speedrunning.debugmod")]
[BepInDependency(DependencyGUID: "org.silksong-modding.modlist", MinimumDependencyVersion: "0.2.0")]
[HarmonyPatch]
public partial class DebugMod : BaseUnityPlugin
{
    private static GameManager _gm;
    private static InputHandler _ih;
    private static HeroController _hc;
    private static GameObject _refKnight;
    private static PlayMakerFSM _refKnightSlash;
    private static CameraController _refCamera;
    private static PlayMakerFSM _refDreamNail;
    private static Collider2D _refHeroCollider;
    private static Collider2D _refHeroBox;

    internal static GameManager GM => _gm != null ? _gm : (_gm = GameManager.instance);
    internal static InputHandler IH => _ih != null ? _ih : (_ih = GM.inputHandler);
    internal static HeroController HC => _hc != null ? _hc : (_hc = HeroController.instance);
    internal static GameObject RefKnight => _refKnight != null ? _refKnight : (_refKnight = HC.gameObject);
    internal static CameraController RefCamera => _refCamera != null ? _refCamera : (_refCamera = GM.cameraCtrl);
    internal static Collider2D RefHeroCollider => _refHeroCollider != null ? _refHeroCollider : (_refHeroCollider = RefKnight.GetComponent<Collider2D>());
    internal static Collider2D RefHeroBox => _refHeroBox != null ? _refHeroBox : (_refHeroBox = RefKnight.transform.Find("HeroBox").GetComponent<Collider2D>());

    //used to stop hazard coros
    internal static IEnumerator CurrentHazardCoro;

    internal static IEnumerator CurrentInvulnCoro;


    internal static DebugMod instance;
    
    public static Settings settings { get; set; } = new Settings();
    public static readonly string ModBaseDirectory = Path.Combine(Application.persistentDataPath, "DebugModData");

    private static float _loadTime;
    private static float _unloadTime;
    private static bool _loadingChar;

    internal static HitInstance? lastHit;
    internal static int lastDamage;
    internal static float lastScaling;

    internal static bool stateOnDeath;
    internal static bool infiniteHP;
    internal static bool infiniteSilk;
    internal static bool infiniteTools;
    internal static bool playerInvincible;
    internal static bool noclip;
    internal static Vector3 noclipPos;
    internal static bool cameraFollow;
    internal static SaveStateManager saveStateManager;
    public static bool KeyBindLock;
    internal static bool TimeScaleActive;
    internal static float CurrentTimeScale = 1f;
    internal static bool PauseGameNoUIActive = false;
    internal static bool savestateFixes = true;
    public static bool overrideLoadLockout = false;
    internal static int extraNailDamage;

    internal static readonly Dictionary<string, BindAction> bindActions = new();
    internal static readonly Dictionary<KeyCode, int> alphaKeyDict = new();
    
    public void Awake()
    {
        LoadSettings();

        if (settings.LogUnityExceptions)
        {
            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                if (type is LogType.Error or LogType.Exception && condition.Contains("Exception"))
                {
                    string message = $"[UNITY] {condition}\n{stackTrace}";
                    LogError(message.Trim());
                }
            };
        }
        
        bindActions.Clear();
        foreach (MethodInfo method in typeof(BindableFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            object[] attributes = method.GetCustomAttributes(typeof(BindableMethod), false);

            if (attributes.Any())
            {
                BindableMethod attr = (BindableMethod)attributes[0];
                bindActions.Add(attr.name, new BindAction(attr, method));
            }
        }

        if (settings.FirstRun || settings.binds == null)
        {
            Log("First run detected, setting default binds");

            settings.FirstRun = false;

            settings.binds = [];
            settings.binds.Add("Toggle All UI", KeyCode.F2);
        }

        int alphaStart = (int)(settings.NumPadForSaveStates ? KeyCode.Keypad0 : KeyCode.Alpha0);

        alphaKeyDict.Clear();
        for (int i = 0; i < 10; i++)
        {
            alphaKeyDict.Add((KeyCode)(alphaStart + i), i);
        }
        
        saveStateManager = new SaveStateManager();

        Harmony harmony = new(Id);
        harmony.PatchAll();

        SceneManager.activeSceneChanged += LevelActivated;
        ModHooks.AfterSavegameLoadHook += LoadCharacter;
        ModHooks.NewGameHook += NewCharacter;
        ModHooks.BeforeSceneLoadHook += OnLevelUnload;
        ModHooks.TakeHealthHook += PlayerDamaged;
        ModHooks.ApplicationQuitHook += SaveSettings;

        if (settings.ShowCursorWhileUnpaused)
        {
            BindableFunctions.SetAlwaysShowCursor();
        }

        ModHooks.FinishedLoadingModsHook += () =>
        {
            BossHandler.PopulateBossLists();
            GUIController.Instance.BuildMenus();
            SceneWatcher.Init();

            LogConsole("New session started " + DateTime.Now);
        };

        KeyBindLock = false;
        TimeScaleActive = false;

        Log("Initialized");
    }

    public DebugMod()
    {
        instance = this;
        // Register exports early so other mods can use them when initializing
        typeof(DebugExport).ModInterop();
    }

    private void LoadSettings()
    {
        try
        {
            if (!Directory.Exists(ModBaseDirectory))
            {
                Directory.CreateDirectory(ModBaseDirectory);
            }

            string path = Path.Combine(ModBaseDirectory, "Settings.json");

            if (File.Exists(path))
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
                Log("Loaded settings");
            }
        }
        catch (Exception e)
        {
            LogError($"Error loading settings: {e}");
        }
    }

    private void SaveSettings()
    {
        try
        {
            string path = Path.Combine(ModBaseDirectory, "Settings.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
            Log("Saved settings");
        }
        catch (Exception e)
        {
            LogError($"Error saving settings: {e}");
        }
    }

    private int PlayerDamaged(int damageAmount)
    {

        int damage = infiniteHP ? 0 : damageAmount;
        if (stateOnDeath && (PlayerData.instance.health - damage <= 0))
        {
            saveStateManager.LoadSaveState(SaveStateType.Memory);
            LogConsole("Lethal damage prevented, savestate loading");
            return 0;
        }
        return damage;
    }

    //save coros so they can be forcibly stopped
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.HazardRespawn))]
    [HarmonyPostfix]
    public static void OnHazardRespawn(HeroController __instance, IEnumerator __result)
    {
        CurrentHazardCoro = __result;
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Invulnerable))]
    [HarmonyPostfix]
    public static void OnInvulnerable(HeroController __instance, IEnumerator __result)
    {
        CurrentInvulnCoro = __result;
    }

    private void NewCharacter() => LoadCharacter(null);

    private void LoadCharacter(SaveGameData saveGameData)
    {
        ConsolePanel.Reset();

        playerInvincible = false;
        infiniteHP = false;
        infiniteSilk = false;
        noclip = false;
        extraNailDamage = 0;

        lastHit = null;
        lastDamage = 0;
        lastScaling = 0f;

        _loadingChar = true;
    }

    private void LevelActivated(Scene sceneFrom, Scene sceneTo)
    {
        string sceneName = sceneTo.name;
        
        if (_loadingChar)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(PlayerData.instance.playTime);
            string text = string.Format("{0:00}.{1:00}", Math.Floor(timeSpan.TotalHours), timeSpan.Minutes);
            int profileID = PlayerData.instance.profileID;
            LogConsole("New savegame loaded. Profile playtime " + text + " Completion: " + PlayerData.instance.completionPercentage + " Save slot: " + profileID + " Game Version: " + PlayerData.instance.version);
            _loadingChar = false;
        }

        if (GM && GM.IsGameplayScene())
        {
            _loadTime = Time.realtimeSinceStartup;
            LogConsole("New scene loaded: " + sceneName);
            PlayerDeathWatcher.Reset();
            BossHandler.LookForBoss(sceneName);
            VisualMaskHelper.OnSceneChange(sceneTo);
        }
    }

    private string OnLevelUnload(string toScene)
    {
        _unloadTime = Time.realtimeSinceStartup;

        return toScene;
    }

    public static string GetSceneName()
    {
        if (GM == null)
        {
           LogWarn("GameManager reference is null in GetSceneName");
            return "";
        }

        string sceneName = GM.GetSceneNameString();
        return sceneName;
    }

    public static float GetLoadTime()
    {
        return (float)Math.Round(_loadTime - _unloadTime, 2);
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.nailDamage), MethodType.Getter)]
    [HarmonyPostfix]
    private static int Get_NailDamage(int nailDamage)
    {
        return nailDamage + extraNailDamage;
    }


    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.TakeDamage))]
    [HarmonyPrefix]
    private static void TakeDamage(HealthManager __instance, HitInstance hitInstance)
    {
        HitInstance scaled = __instance.ApplyDamageScaling(hitInstance);
        lastHit = scaled;
        lastDamage = __instance.damageOverride ? 1 : Mathf.RoundToInt(scaled.DamageDealt * scaled.Multiplier);

        int scaleLevel = hitInstance.DamageScalingLevel - 1;
        if (hitInstance.IsUsingNeedleDamageMult)
        {
            scaleLevel = PlayerData.instance.nailUpgrades;
        }
        else if (hitInstance.RepresentingTool && hitInstance.RepresentingTool.Type != ToolItemType.Skill)
        {
            scaleLevel = PlayerData.instance.ToolKitUpgrades;
        }
        lastScaling = __instance.damageScaling.GetMultFromLevel(scaleLevel);
    }

    // Prevents clipping through water when invincible
    [HarmonyPatch(typeof(SurfaceWaterRegion), nameof(SurfaceWaterRegion.OnTriggerEnter2D))]
    [HarmonyPrefix]
    private static void OnTriggerEnter2D_Prefix(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<HeroController>() && playerInvincible)
        {
            PlayerData.instance.isInvincible = false;
        }
    }

    [HarmonyPatch(typeof(SurfaceWaterRegion), nameof(SurfaceWaterRegion.OnTriggerEnter2D))]
    [HarmonyPostfix]
    private static void OnTriggerEnter2D_Postfix()
    {
        if (playerInvincible)
        {
            PlayerData.instance.isInvincible = true;
        }
    }

    // Prevents savestates loaded while in water from warping to the wrong point.
    [HarmonyPatch(typeof(HeroWaterController), nameof(HeroWaterController.TumbleOut))]
    [HarmonyPrefix]
    private static bool HeroWaterController_TumbleOut_Prefix(HeroWaterController __instance)
    {
        if (SaveState.loadingSavestate == null) return true;
        
        __instance.ExitedWater();
        return false;
    }

    // Bounce off lava when invincible so the player doesn't just fall through the map
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
    [HarmonyPrefix]
    private static bool HeroController_TakeDamage(GameObject go, HazardType hazardType)
    {
        if (playerInvincible && !noclip && hazardType == HazardType.LAVA && go.name.Contains("Lava Box"))
        {
            HeroController.instance.ShroomBounce();
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.DoSpecialDamage))]
    [HarmonyPrefix]
    private static bool HeroController_DoSpecialDamage()
    {
        return !playerInvincible;
    }
    
    /// <summary>
    /// Add all public static methods on a type to the keybinds list. Methods must be decorated with the BindableMethod attribute.
    /// </summary>
    [PublicAPI]
    public static void AddToKeyBindList(Type BindableFunctionsClass)
    {
        foreach (MethodInfo method in BindableFunctionsClass.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.GetCustomAttribute<BindableMethod>(false) is BindableMethod attr)
            {
                Log($"Recieved Action: {attr.name} (from {BindableFunctionsClass.Name})");
                bindActions.Add(attr.name, new BindAction(attr, method));
            } 
        }
    }

    /// <summary>
    /// Add an action to the keybinds list.
    /// </summary>
    [PublicAPI]
    public static void AddActionToKeyBindList(Action method, string name, string category)
    {
        AddActionToKeyBindList(method, name, category, true);   
    }

    /// <summary>
    /// Add an action to the keybinds list.
    /// </summary>
    [PublicAPI]
    public static void AddActionToKeyBindList(Action method, string name, string category, bool allowLock)
    {
        Log($"Received Action: {name}");
        bindActions.Add(name, new BindAction(name, category, allowLock, method));
    }

    public static void LogDebug(string message)
    {
        instance.Logger.LogDebug(message);
    }

    public static void Log(string message)
    {
        instance.Logger.LogInfo(message);
    }

    public static void LogWarn(string message)
    {
        instance.Logger.LogWarning(message);
    }

    public static void LogError(string message)
    {
        instance.Logger.LogError(message);
    }

    public static void LogConsole(string message)
    {
        ConsolePanel.AddLine(message);
    }
}