using DebugMod.Helpers;
using DebugMod.Hitbox;
using DebugMod.MonoBehaviours;
using GlobalEnums;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod.SaveStates;

/// <summary>
/// Handles struct SaveStateData and individual SaveState operations
/// </summary>
[HarmonyPatch]
public partial class SaveState
{
    // Some mods (ItemChanger) check type to detect vanilla scene loads.
    private class DebugModSaveStateSceneLoadInfo : GameManager.SceneLoadInfo { }

    //used to stop double loads
    public static SaveState loadingSavestate { get; private set; }

    public static bool LoadDuped { get; set; }

    public static event Action<SaveState> OnSave;
    public static event Action<SaveState> BeforeLoad;
    public static event Action<SaveState> AfterLoad;

    [Serializable]
    public partial class SaveStateData
    {
        public string saveStateIdentifier;
        public string saveScene;
        public PlayerData savedPd;
        public SceneData savedSd;
        public Vector3 savePos;
        public bool facingLeft; // for backwards compatibility
        public bool facingRight;
        public bool isKinematized;
        public string[] loadedScenes;
        public string[] loadedSceneActiveScenes;
        public string roomSpecificOptions;
        [SerializeField] private List<string> customDataList;
        [NonSerialized] public Dictionary<string, string> customData;

        internal SaveStateData() { }

        partial void CopyGameSpecific(SaveStateData _data);

        internal SaveStateData(SaveStateData _data)
        {
            saveStateIdentifier = _data.saveStateIdentifier;
            saveScene = _data.saveScene;

            savedPd = _data.savedPd;
            savedSd = _data.savedSd;
            savePos = _data.savePos;
            facingLeft = _data.facingLeft;
            facingRight = _data.facingRight;
            isKinematized = _data.isKinematized;
            roomSpecificOptions = _data.roomSpecificOptions;

            CopyGameSpecific(_data);

            if (_data.loadedScenes is not null)
            {
                loadedScenes = new string[_data.loadedScenes.Length];
                Array.Copy(_data.loadedScenes, loadedScenes, _data.loadedScenes.Length);
            }
            else
            {
                loadedScenes = new[] { saveScene };
            }

            loadedSceneActiveScenes = new string[loadedScenes.Length];
            if (_data.loadedSceneActiveScenes is not null)
            {
                Array.Copy(_data.loadedSceneActiveScenes, loadedSceneActiveScenes, loadedSceneActiveScenes.Length);
            }
            else
            {
                for (int i = 0; i < loadedScenes.Length; i++)
                {
                    loadedSceneActiveScenes[i] = loadedScenes[i];
                }
            }

            if (_data.customData != null)
            {
                customData = new Dictionary<string, string>(_data.customData);
            }
        }

        public void BeforeSerialize()
        {
            customData ??= [];
            customDataList = [];

            foreach (var kvp in customData)
            {
                customDataList.Add($"{kvp.Key}:::{kvp.Value}");
            }
        }

        public void AfterDeserialize()
        {
            customDataList ??= [];
            customData = [];

            foreach (string item in customDataList)
            {
                int splitIndex = item.IndexOf(":::", StringComparison.Ordinal);
                if (splitIndex > 0)
                {
                    string key = item[..splitIndex];
                    string value = item[(splitIndex + 3)..];
                    customData[key] = value;
                }
            }
        }

        public SaveStateData DeepCopy()
        {
            return new SaveStateData(this);
        }
    }

    [SerializeField]
    public SaveStateData data;

    private partial IEnumerator PrepareSceneLoad(bool abortDeath);
    private partial IEnumerator LoadDummyScene();
    private partial IEnumerator LoadDupedScene(string sceneName);
    private partial IEnumerator ResetGameSpecificState(bool loadSafely);

    partial void SaveGameSpecific();
    partial void RestoreGameSpecific();

    internal SaveState()
    {
        data = new SaveStateData();
    }

    #region saving
    public bool Save()
    {
        //save level state before savestates so levers and dead enemies persist properly
        GameManager.instance.SaveLevelState();
        data.saveScene = GameManager.instance.GetSceneNameString();
        data.saveStateIdentifier = $"{data.saveScene}-{DateTime.Now:H:mm_d-MMM}";

        //implementation so room specifics can be automatically saved
        try
        {
            data.roomSpecificOptions = RoomSpecific.SaveRoomSpecific(data.saveScene);
        }
        catch (Exception e)
        {
            DebugMod.LogError(e.Message);
        }

        data.savedPd = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(PlayerData.instance));
        data.savedSd = JsonUtility.FromJson<SceneData>(JsonUtility.ToJson(SceneData.instance));
        SaveGameSpecific();
        data.savePos = HeroController.instance.gameObject.transform.position;
        data.facingLeft = !HeroController.instance.cState.facingRight;
        data.facingRight = HeroController.instance.cState.facingRight;
        data.isKinematized = HeroController.instance.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic;

        var scenes = SceneWatcher.LoadedScenes;
        data.loadedScenes = scenes.Select(s => s.name).ToArray();
        data.loadedSceneActiveScenes = scenes.Select(s => s.activeSceneWhenLoaded).ToArray();

        data.customData = [];

        OnSave?.Invoke(this);

        DebugMod.LogConsole("Created savestate");
        return true;
    }

    #endregion

    #region loading
    public IEnumerator Load()
    {
        if (!IsSet())
        {
            DebugMod.LogError("Attempted to load unset savestate");
            yield break;
        }

        // Second check is probably not necessary since it's already checked at the call sites the frame before,
        // but might as well be defensive and rule out any frame-perfect nonsense
        if (loadingSavestate != null && !DebugMod.overrideLoadLockout)
        {
            DebugMod.LogConsole($"Attempted to load savestate in {data.saveScene} while another is already loading, cancelling");
            yield break;
        }

        DebugMod.LogDebug($"Loading savestate: {data.saveStateIdentifier}");

        System.Diagnostics.Stopwatch loadingStateTimer = new();
        loadingStateTimer.Start();

        loadingSavestate = this;

        IEnumerator enumerator = LoadImpl();
        while (true)
        {
            try
            {
                if (!enumerator.MoveNext())
                {
                    break;
                }
            }
            catch (Exception e)
            {
                DebugMod.LogError($"Error loading savestate: {e}");
                DebugMod.LogConsole("Critical error loading savestate, please create a bug report");

                // Hopefully enough to work around most errors and keep the game playable
                TimeScale.Frozen = false;
                loadingSavestate = null;

                yield break;
            }

            yield return enumerator.Current;
        }

        loadingSavestate = null;

        loadingStateTimer.Stop();
        TimeSpan loadingStateTime = loadingStateTimer.Elapsed;
        DebugMod.LogConsole("Loaded savestate in " + loadingStateTime.ToString(@"ss\.fff") + "s");

        yield return new WaitUntil(() => GameCameras.instance.hudCanvasSlideOut.gameObject);
        yield return null; // Not all HUD elements are ready immediately, wait one more frame
        HUDFixes();

        // Fix bench interactions
        foreach (RestBench bench in Object.FindObjectsByType<RestBench>(FindObjectsSortMode.None))
        {
            if (!bench.GetComponent<HeroController>()) // Why, Team Cherry?
            {
                bench.gameObject.SetActive(false);
                bench.gameObject.SetActive(true);
            }
        }

#if SILKSONG
        // Fixes spawning into surface water (can't do this any earlier since you would get forced out)
        foreach (SurfaceWaterRegion water in Object.FindObjectsByType<SurfaceWaterRegion>(FindObjectsSortMode.None))
        {
            if (Physics2D.IsTouching(HeroController.instance.GetComponent<Collider2D>(), water.GetComponent<Collider2D>()))
            {
                yield return new WaitUntil(() => GameManager.instance.sceneLoad == null);
                HeroController.instance.transform.position = data.savePos;
                HeroController.instance.GetComponent<BoxCollider2D>().enabled = false;
                HeroController.instance.GetComponent<BoxCollider2D>().enabled = true;
                break;
            }
        }
#endif
    }

    private IEnumerator LoadImpl()
    {
        //prevents silly things from happening
        TimeScale.Frozen = true;
        Time.fixedDeltaTime = 0.02f;

        BeforeLoad?.Invoke(this);

        // Dupe loading only works with safe loading enabled, so force it on if asked by extension to dupe.
        var loadSafely = DebugMod.settings.SafeSaveStateLoading || LoadDuped
            || data.saveScene == "Bone_East_08"; // Also, this scene is just evil

        //called here because this needs to be done here
        #region glitchfixes
        //TODO: Cleaner way to do this? Also get it to actually work
        //prevent hazard respawning
        if (DebugMod.CurrentHazardCoro != null)
            HeroController.instance.StopCoroutine(DebugMod.CurrentHazardCoro);
        if (DebugMod.CurrentInvulnCoro != null)
            HeroController.instance.StopCoroutine(DebugMod.CurrentInvulnCoro);
#if SILKSONG
        if (HeroController.instance.hazardRespawnRoutine != null)
            HeroController.instance.StopCoroutine(HeroController.instance.hazardRespawnRoutine);
#endif

        if (DebugMod.CurrentDieCoro != null)
            HeroController.instance.StopCoroutine(DebugMod.CurrentDieCoro);
        if (DebugMod.CurrentPlayerDeadCoro != null)
            HeroController.instance.StopCoroutine(DebugMod.CurrentPlayerDeadCoro);
        bool abortDeath = HeroController.instance.cState.dead;
        if (abortDeath)
        {
            HeroController.instance.cState.dead = false;
        }
        DebugMod.CurrentHazardCoro = null;
        DebugMod.CurrentInvulnCoro = null;
        DebugMod.CurrentDieCoro = null;
        DebugMod.CurrentPlayerDeadCoro = null;
#if SILKSONG
        HeroController.instance.hazardRespawnRoutine = null;
#endif
        HeroController.instance.hazardInvulnRoutine = null;

        //fixes knockback storage
        HeroController.instance.CancelDamageRecoil();

        //ends hazard respawn animation
        var invPulse = HeroController.instance.GetComponent<InvulnerablePulse>();
        invPulse.StopInvulnerablePulse();

        // Reset problematic cstates
        HeroController.instance.cState.hazardDeath = false;

        EventRegister.SendEvent("HAZARD RESPAWN RESET");
        #endregion

        // Close inventory and dialogue
        EventRegister.SendEvent("INVENTORY CANCEL");
        yield return ResetGameSpecificState(loadSafely);

        yield return PrepareSceneLoad(abortDeath);

        string previousScene = GameManager.instance.GetSceneNameString();

        GameManager.instance.entryGateName = "dreamGate";
        GameManager.instance.startedOnThisScene = true;

        if (loadSafely)
        {
            yield return LoadDummyScene();
        }

        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.savedSd), SceneData.instance);
        GameManager.instance.ResetSemiPersistentItems();
        RestoreGameSpecific();

#if SILKSONG
        StaticVariableList.ClearSceneTransitions(); // Clears cached data like quest board contents
        GameManager.ReportUnload(previousScene); // Clears object pools in case the same scene is reloaded
#endif

        yield return null;

        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.savedPd), PlayerData.instance);
#if SILKSONG
        PlayerData.instance.ResetCutsceneBools(); // These fields are NonSerialized so aren't overwritten

        if (data.toolAmountsOverride != null)
        {
            PlayerData.instance.toolAmountsOverride = [];
            foreach (string item in data.toolAmountsOverride)
            {
                string[] parts = item.Split(':');
                PlayerData.instance.toolAmountsOverride[parts[0]] = int.Parse(parts[1]);
            }
        }
        else
        {
            PlayerData.instance.toolAmountsOverride = null;
        }
#endif

        SceneWatcher.LoadedSceneInfo[] sceneData = data
            .loadedScenes
            .Zip(data.loadedSceneActiveScenes, (name, gameplay) => new SceneWatcher.LoadedSceneInfo(name, gameplay))
            .ToArray();

        sceneData[0].LoadHook();

        GameManager.instance.BeginSceneTransition
        (
            new DebugModSaveStateSceneLoadInfo
            {
                SceneName = data.saveScene,
                HeroLeaveDirection = GatePosition.unknown,
                EntryGateName = "dreamGate",
                EntryDelay = 0f,
                PreventCameraFadeOut = true,
                WaitForSceneTransitionCameraFade = false,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = loadSafely
            }
        );

        yield return new WaitUntil(() => USceneManager.GetActiveScene().name == data.saveScene);

        if (LoadDuped)
        {
            yield return new WaitUntil(() => GameManager.instance.IsInSceneTransition == false);
            for (int i = 1; i < sceneData.Length; i++)
            {
                while (USceneManager.loadedSceneCount > i)
                {
                    // When we load a scene with a corresponding boss scene, we get more scenes than we expected. Unload the additional scenes.
                    DebugMod.LogDebug($"{USceneManager.loadedSceneCount} scenes loaded, expected {i} ({SceneWatcher.LoadedScenes.Join(lsi => $"{lsi.name}")})");
                    if (SceneWatcher.LoadedScenes[i].name == sceneData[i].name)
                    {
                        DebugMod.LogDebug($"Extra scene matches state, skipping load {i} ({sceneData[i].name})");
                        i++;
                        continue;
                    }
                    var unloadOp = USceneManager.UnloadSceneAsync((Scene)SceneWatcher.LoadedScenes[i].sceneObject!);
                    yield return unloadOp;
                }
                if (i >= sceneData.Length) { continue; } // Ensure we skip final load

                SceneWatcher.LoadedSceneInfo.activeInfo = sceneData[i];
                DebugMod.LogDebug($"Loading  scene {i}: {sceneData[i].name}");
                yield return LoadDupedScene(sceneData[i].name);
                SceneWatcher.LoadedSceneInfo.activeInfo = null;
                GameManager.instance.RefreshTilemapInfo(sceneData[i].name);
                GameManager.instance.cameraCtrl.SceneInit();
            }

            GameManager.instance.BeginScene();
        }

        HeroController.instance.CharmUpdate();

#if SILKSONG
        // invalidates caches
        QuestManager.IncrementVersion();
        CollectableItemManager.IncrementVersion();
#endif

        PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
        PlayMakerFSM.BroadcastEvent("TOOL EQUIPS CHANGED");
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        EventRegister.SendEvent("END FOLLOWERS INSTANT");

        if (loadSafely)
        {
            // Probably just being paranoid?
            yield return null;
        }

        HeroController.instance.transform.position = data.savePos;
        DebugMod.noclipPos = HeroController.instance.transform.position;
        HeroController.instance.transitionState = HeroTransitionState.WAITING_TO_TRANSITION;
        if (data.isKinematized) HeroController.instance.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        if (data.facingLeft) HeroController.instance.FaceLeft();
        else if (data.facingRight) HeroController.instance.FaceRight();

        try
        {
            GameManager.instance.cameraCtrl.PositionToHeroInstant(false);
            GameManager.instance.cameraCtrl.isGameplayScene = true;
            GameManager.instance.UpdateUIStateFromGameState();
        }
        catch (NullReferenceException) { } // This can fail for mysterious reasons if loading out of a cutscene, but it isn't crucial

        if (LoadDuped && DebugMod.settings.ShowHitBoxes > 0)
        {
            int cs = DebugMod.settings.ShowHitBoxes;
            DebugMod.settings.ShowHitBoxes = 0;
            yield return new WaitUntil(() => HitboxViewer.State == 0);
            DebugMod.settings.ShowHitBoxes = cs;
        }

        // Reset various things that could be out of place depending on when the load started
        HeroController.instance.ResetLockStates();
        HeroController.instance.GetComponent<MeshRenderer>().enabled = true;
        DebugMod.RefHeroCollider.enabled = !DebugMod.heroColliderDisabled;
        HeroBox.Inactive = DebugMod.heroColliderDisabled;
        HeroController.instance.gameObject.layer = (int)PhysLayers.PLAYER;
        HeroController.instance.FinishedEnteringScene();

        RoomSpecific.BackwardsCompat(data.saveScene, ref data.roomSpecificOptions);

        if (!string.IsNullOrEmpty(data.roomSpecificOptions))
        {
            DebugMod.LogConsole("Performing room specific option " + data.roomSpecificOptions);
            yield return RoomSpecific.DoRoomSpecific(data.saveScene, data.roomSpecificOptions);
        }

        yield return RoomSpecific.DoGenericFixes(data.saveScene);

        //removes things like bench storage no clip float etc
        SaveStateGlitchFixes();

        AfterLoad?.Invoke(this);

        yield return new WaitUntil(() => !GameManager.instance.isLoading);

        // For redundancy
        HeroController.instance.transform.position = data.savePos;
        DebugMod.noclipPos = HeroController.instance.transform.position;

        //pause fixes from homothety
        if (GameManager.instance.isPaused)
        {
            GameManager.instance.FadeSceneIn();
            GameManager.instance.isPaused = false;
            GameCameras.instance.ResumeCameraShake();
            if (HeroController.SilentInstance != null)
            {
                HeroController.instance.UnPause();
            }

            MenuButtonList.ClearAllLastSelected();
            TimeManager.TimeScale = 1f;
        }

        //set timescale back
        TimeScale.Frozen = false;
    }

    //these are toggleable, as they will prevent glitches from persisting
    private void SaveStateGlitchFixes()
    {
        var rb2d = HeroController.instance.GetComponent<Rigidbody2D>();

        //float
        HeroController.instance.AffectedByGravity(true);
        rb2d.gravityScale = 0.79f;

        //invuln
#if SILKSONG
        HeroController.instance.gameObject.LocateMyFSM("Roar and Wound States").FsmVariables.FindFsmBool("Force Roar Lock").Value = false;
#endif
        HeroController.instance.cState.invulnerable = false;

        //no clip
        rb2d.bodyType = RigidbodyType2D.Dynamic;

        //bench storage
        GameManager.instance.SetPlayerDataBool(nameof(PlayerData.atBench), false);

        if (HeroController.SilentInstance != null)
        {
            if (HeroController.instance.cState.onConveyor || HeroController.instance.cState.onConveyorV || HeroController.instance.cState.inConveyorZone)
            {
                HeroController.instance.GetComponent<ConveyorMovementHero>()?.StopConveyorMove();
                HeroController.instance.cState.inConveyorZone = false;
                HeroController.instance.cState.onConveyor = false;
                HeroController.instance.cState.onConveyorV = false;
            }

            HeroController.instance.cState.nearBench = false;
        }

        GameSpecificSaveStateGlitchFixes();
    }

    private partial void GameSpecificSaveStateGlitchFixes();

    //Moving all HUD related code to here for clarity
    private partial void HUDFixes();
    #endregion

    #region helper functionality
    public bool IsSet() => !string.IsNullOrEmpty(data.saveStateIdentifier);

    public override string ToString() => IsSet() ? data.saveStateIdentifier : Localization.Get("SAVESTATEPANEL_EMPTY");
    #endregion

    #region patches
    // Bring back dream gate transitions >:(
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.FindEntryPoint))]
    [HarmonyPrefix]
    private static bool FindEntryPoint(GameManager __instance, ref Vector2? __result, string entryPointName)
    {
        if (entryPointName == "dreamGate" && !__instance.RespawningHero)
        {
            __result = HeroController.instance.gameObject.transform.position;
            return false;
        }

        return true;
    }

    // Prevent objects in the previous scene overwriting scene data when unloading
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SaveLevelState))]
    [HarmonyPrefix]
    private static bool SaveLevelState()
    {
        return loadingSavestate == null;
    }

    #endregion
}