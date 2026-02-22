using DebugMod.Helpers;
using DebugMod.Hitbox;
using DebugMod.MonoBehaviours;
using GlobalEnums;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod.SaveStates;

/// <summary>
/// Handles struct SaveStateData and individual SaveState operations
/// </summary>
[HarmonyPatch]
public class SaveState
{
    // Some mods (ItemChanger) check type to detect vanilla scene loads.
    private class DebugModSaveStateSceneLoadInfo : GameManager.SceneLoadInfo { }

    //used to stop double loads
    public static SaveState loadingSavestate { get; private set; }

    [Serializable]
    public class SaveStateData
    {
        public string saveStateIdentifier;
        public string saveScene;
        public PlayerData savedPd;
        public SceneData savedSd;
        public SceneData.SerializableBoolData[] semiPersistentBools;
        public SceneData.SerializableIntData[] semiPersistentInts;
        public Vector3 savePos;
        public FieldInfo cameraLockArea;
        public object lockArea;
        public bool isKinematized;
        public HeroController.HunterUpgCrestStateInfo evoState;
        public bool isMaggoted;
        public string[] loadedScenes;
        public string[] loadedSceneActiveScenes;
        public string roomSpecificOptions;


        internal SaveStateData() { }

        internal SaveStateData(SaveStateData _data)
        {
            saveStateIdentifier = _data.saveStateIdentifier;
            saveScene = _data.saveScene;

            cameraLockArea = _data.cameraLockArea;
            savedPd = _data.savedPd;
            savedSd = _data.savedSd;
            savePos = _data.savePos;
            lockArea = _data.lockArea;
            isKinematized = _data.isKinematized;
            evoState = _data.evoState;
            isMaggoted = _data.isMaggoted;
            roomSpecificOptions = _data.roomSpecificOptions;

            if (_data.semiPersistentBools is not null)
            {
                semiPersistentBools = new SceneData.SerializableBoolData[_data.semiPersistentBools.Length];
                Array.Copy(_data.semiPersistentBools, semiPersistentBools, _data.semiPersistentBools.Length);
            }

            if (_data.semiPersistentInts is not null)
            {
                semiPersistentInts = new SceneData.SerializableIntData[_data.semiPersistentInts.Length];
                Array.Copy(_data.semiPersistentInts, semiPersistentInts, _data.semiPersistentInts.Length);
            }

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
        }

        public SaveStateData DeepCopy()
        {
            return new SaveStateData(this);
        }
    }

    [SerializeField]
    public SaveStateData data;

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
        data.semiPersistentBools = SaveSemiPersistent(SceneData.instance.persistentBools);
        data.semiPersistentInts = SaveSemiPersistent(SceneData.instance.persistentInts);
        data.savePos = HeroController.instance.gameObject.transform.position;
        data.cameraLockArea = (data.cameraLockArea ?? typeof(CameraController).GetField("currentLockArea", BindingFlags.Instance | BindingFlags.NonPublic));
        data.lockArea = data.cameraLockArea.GetValue(GameManager.instance.cameraCtrl);
        data.isKinematized = HeroController.instance.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic;
        data.evoState = HeroController.instance.hunterUpgState;
        data.isMaggoted = HeroController.instance.cState.isMaggoted;

        var scenes = SceneWatcher.LoadedScenes;
        data.loadedScenes = scenes.Select(s => s.name).ToArray();
        data.loadedSceneActiveScenes = scenes.Select(s => s.activeSceneWhenLoaded).ToArray();

        DebugMod.LogConsole("Saved temp state");
        return true;
    }

    private static TContainer[] SaveSemiPersistent<TValue, TContainer>(SceneData.PersistentItemDataCollection<TValue, TContainer> collection)
        where TContainer : SceneData.SerializableItemData<TValue>, new()
    {
        List<TContainer> list = [];

        foreach (Dictionary<string, PersistentItemData<TValue>> scene in collection.scenes.Values)
        {
            foreach (PersistentItemData<TValue> item in scene.Values)
            {
                if (item.IsSemiPersistent)
                {
                    TContainer container = new()
                    {
                        SceneName = item.SceneName,
                        ID = item.ID,
                        Value = item.Value,
                        Mutator = item.Mutator,
                    };
                    list.Add(container);
                }
            }
        }

        return list.ToArray();
    }
    #endregion

    #region loading
    //loadDuped is used by external mods
    public IEnumerator Load(bool loadDuped = false)
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

        System.Diagnostics.Stopwatch loadingStateTimer = new();
        loadingStateTimer.Start();

        loadingSavestate = this;

        IEnumerator enumerator = LoadImpl(loadDuped);
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
    }

    private IEnumerator LoadImpl(bool loadDuped)
    {
        bool stateondeath = DebugMod.stateOnDeath;
        DebugMod.stateOnDeath = false;

        //prevents silly things from happening
        TimeScale.Frozen = true;

        //called here because this needs to be done here
        if (DebugMod.savestateFixes)
        {
            //TODO: Cleaner way to do this? Also get it to actually work
            //prevent hazard respawning
            if (DebugMod.CurrentHazardCoro != null)
                HeroController.instance.StopCoroutine(DebugMod.CurrentHazardCoro);
            if (DebugMod.CurrentInvulnCoro != null)
                HeroController.instance.StopCoroutine(DebugMod.CurrentInvulnCoro);
            DebugMod.CurrentHazardCoro = null;
            DebugMod.CurrentInvulnCoro = null;
            HeroController.instance.hazardInvulnRoutine = null;

            //fixes knockback storage
            HeroController.instance.CancelDamageRecoil();

            //ends hazard respawn animation
            var invPulse = HeroController.instance.GetComponent<InvulnerablePulse>();
            invPulse.StopInvulnerablePulse();
        }

        if (data.savedPd == null || string.IsNullOrEmpty(data.saveScene)) yield break;

        // Close inventory and dialogue
        EventRegister.SendEvent("INVENTORY CANCEL");
        DialogueBox.EndConversation();
        DialogueBox.HideInstant();
        DialogueYesNoBox.ForceClose();
        QuestYesNoBox.ForceClose();

        EventRegister.SendEvent("REST AREA MUSIC STOP");
        ToolItemManager.SetIsInCutscene(false);
        CameraBlurPlane.Spacing = 0f;
        CameraBlurPlane.Vibrancy = 0f;
        CameraBlurPlane.MaskLerp = 0f;
        ScreenFaderUtils.Fade(ScreenFaderUtils.GetColour(), Color.clear, 0f);

        // Fix slopes
        foreach (SlideSurface surface in Object.FindObjectsByType<SlideSurface>(FindObjectsSortMode.None))
        {
            if (surface.isHeroAttached)
            {
                surface.Detach(false);
            }
        }

        // Prevent silk spool regen from continuing after (or during!) the load.
        HeroController.instance.ResetSilkRegen();

        if (HeroController.instance.transform.parent)
        {
            HeroController.instance.transform.SetParent(null, true);
            Object.DontDestroyOnLoad(HeroController.instance);
        }

        // If another scene load operation is in progress, loading the dummy scene will hang
        yield return ScenePreloader.ForceEndPendingOperations();

        string previousScene = GameManager.instance.GetSceneNameString();

        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.savedSd), SceneData.instance);
        GameManager.instance.ResetSemiPersistentItems();
        RestoreSemiPersistent(data.semiPersistentBools, SceneData.instance.persistentBools);
        RestoreSemiPersistent(data.semiPersistentInts, SceneData.instance.persistentInts);

        StaticVariableList.ClearSceneTransitions(); // Clears cached data like quest board contents
        GameManager.ReportUnload(previousScene); // Clears object pools in case the same scene is reloaded

        yield return null;

        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.savedPd), PlayerData.instance);

        SceneWatcher.LoadedSceneInfo[] sceneData = data
            .loadedScenes
            .Zip(data.loadedSceneActiveScenes, (name, gameplay) => new SceneWatcher.LoadedSceneInfo(name, gameplay))
            .ToArray();

        sceneData[0].LoadHook();

        GameManager.instance.entryGateName = "dreamGate";
        GameManager.instance.startedOnThisScene = true;

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
                AlwaysUnloadUnusedAssets = false
            }
        );

        yield return new WaitUntil(() => USceneManager.GetActiveScene().name == data.saveScene);

        if (loadDuped)
        {
            yield return new WaitUntil(() => GameManager.instance.IsInSceneTransition == false);
            for (int i = 1; i < sceneData.Length; i++)
            {
                SceneWatcher.LoadedSceneInfo.activeInfo = sceneData[i];
                var loadop = Addressables.LoadSceneAsync($"Scenes/{sceneData[i].name}", LoadSceneMode.Additive);
                yield return loadop;
                SceneWatcher.LoadedSceneInfo.activeInfo = null;
                GameManager.instance.RefreshTilemapInfo(sceneData[i].name);
                GameManager.instance.cameraCtrl.SceneInit();
            }

            GameManager.instance.BeginScene();
        }

        if (data.lockArea != null)
        {
            GameManager.instance.cameraCtrl.LockToArea(data.lockArea as CameraLockArea);
        }

        HeroController.instance.CharmUpdate();

        // invalidates caches
        QuestManager.IncrementVersion();
        CollectableItemManager.IncrementVersion();

        PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
        PlayMakerFSM.BroadcastEvent("TOOL EQUIPS CHANGED");
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");


        HeroController.instance.gameObject.transform.position = data.savePos;
        HeroController.instance.transitionState = HeroTransitionState.WAITING_TO_TRANSITION;
        if (data.isKinematized) HeroController.instance.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        DebugMod.noclipPos = data.savePos;

        GameManager.instance.cameraCtrl.PositionToHeroInstant(false);
        GameManager.instance.cameraCtrl.isGameplayScene = true;
        GameManager.instance.UpdateUIStateFromGameState();

        if (loadDuped && DebugMod.settings.ShowHitBoxes > 0)
        {
            int cs = DebugMod.settings.ShowHitBoxes;
            DebugMod.settings.ShowHitBoxes = 0;
            yield return new WaitUntil(() => HitboxViewer.State == 0);
            DebugMod.settings.ShowHitBoxes = cs;
        }

        HeroController.instance.FinishedEnteringScene(true, false);
        // Fixes invisible player when loading out of certain boss attacks
        HeroController.instance.GetComponent<MeshRenderer>().enabled = true;
        // Fixes falling out of the map when loading out of some animations (i.e. bell eater entrance)
        DebugMod.RefHeroCollider.enabled = !DebugMod.heroColliderDisabled;
        HeroBox.Inactive = DebugMod.heroColliderDisabled;

        RoomSpecific.BackwardsCompat(data.saveScene, ref data.roomSpecificOptions);

        if (!string.IsNullOrEmpty(data.roomSpecificOptions))
        {
            DebugMod.LogConsole("Performing Room Specific Option " + data.roomSpecificOptions);
            yield return RoomSpecific.DoRoomSpecific(data.saveScene, data.roomSpecificOptions);
        }

        yield return RoomSpecific.DoGenericFixes(data.saveScene);

        //removes things like bench storage no clip float etc
        if (DebugMod.settings.SaveStateGlitchFixes) SaveStateGlitchFixes();

        yield return new WaitUntil(() => !GameManager.instance.isLoading);

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
        DebugMod.stateOnDeath = stateondeath;
    }

    private static void RestoreSemiPersistent<TValue, TContainer>(TContainer[] list, SceneData.PersistentItemDataCollection<TValue, TContainer> collection)
        where TContainer : SceneData.SerializableItemData<TValue>, new()
    {
        if (list != null)
        {
            foreach (TContainer container in list)
            {
                if (!collection.scenes.ContainsKey(container.SceneName))
                {
                    collection.scenes.Add(container.SceneName, []);
                }

                collection.scenes[container.SceneName][container.ID] = new PersistentItemData<TValue>
                {
                    SceneName = container.SceneName,
                    ID = container.ID,
                    Value = container.Value,
                    Mutator = container.Mutator,
                    IsSemiPersistent = true,
                };
            }
        }
    }

    //these are toggleable, as they will prevent glitches from persisting
    private void SaveStateGlitchFixes()
    {
        var rb2d = HeroController.instance.GetComponent<Rigidbody2D>();

        //float
        HeroController.instance.AffectedByGravity(true);
        rb2d.gravityScale = 0.79f;

        //invuln
        HeroController.instance.gameObject.LocateMyFSM("Roar and Wound States").FsmVariables.FindFsmBool("Force Roar Lock").Value = false;
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

        // Pogo storage
        if (HeroController.instance.currentDownspike)
        {
            HeroController.instance.currentDownspike.CancelAttack();
        }
    }

    //Moving all HUD related code to here for clarity
    private void HUDFixes()
    {
        Object.FindAnyObjectByType<InventoryPaneList>()?.gameObject?.LocateMyFSM("Inventory Control")?.SetState("Close");

        if (CurrencyCounter._currencyCounters.TryGetValue(CurrencyType.Money, out List<CurrencyCounter> list))
        {
            foreach (CurrencyCounter counter in list)
            {
                counter.geoTextMesh.Text = data.savedPd.geo.ToString();
            }
        }
        if (CurrencyCounter._currencyCounters.TryGetValue(CurrencyType.Shard, out list))
        {
            foreach (CurrencyCounter counter in list)
            {
                counter.geoTextMesh.Text = data.savedPd.ShellShards.ToString();
            }
        }

        PlayerData.instance.health = data.savedPd.health;

        // Resets maggots, lifeblood, buff tools, etc.
        HeroController.instance.ClearEffects();

        // Need to be done after ClearEffects()
        if (data.isMaggoted)
        {
            HeroController.instance.cState.isMaggoted = true; // Avoids sound effect
            HeroController.instance.SetIsMaggoted(true);
            Utils.FindFSM("Maggots", "Maggot Effect").SetState("Is Maggoted?");
        }
        HeroController.instance.hunterUpgState = data.evoState;

        int healthBlue = data.savedPd.healthBlue;
        for (int i = 0; i < healthBlue; i++)
        {
            EventRegister.SendEvent("ADD BLUE HEALTH");
        }

        HudHelper.RefreshMasks();
        HudHelper.RefreshSpool();

        // Spawns tool icons if they weren't already visible
        EventRegister.SendEvent("LAST HP ADDED");

        // Update active crest behind health display
        BindOrbHudFrame bindOrb = Object.FindAnyObjectByType<BindOrbHudFrame>();
        if (bindOrb)
        {
            bindOrb.isActive = true;
            bindOrb.currentFrameCrest = null;
            bindOrb.Refresh(true, false);
        }
    }
    #endregion

    #region helper functionality
    public bool IsSet() => !string.IsNullOrEmpty(data.saveStateIdentifier);

    public override string ToString() => IsSet() ? data.saveStateIdentifier : "Empty";
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
    #endregion
}