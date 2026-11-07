using DebugMod.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod.SaveStates;

public partial class SaveState
{
    public partial class SaveStateData
    {
        public int evoState;
        public bool isMaggoted;
        public SceneData.SerializableBoolData[] semiPersistentBools;
        public SceneData.SerializableIntData[] semiPersistentInts;

        partial void CopyGameSpecific(SaveStateData _data)
        {
            evoState = _data.evoState;
            isMaggoted = _data.isMaggoted;

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
        }
    }

    private partial IEnumerator PrepareSceneLoad(bool abortDeath)
    {
        // If another scene load operation is in progress, loading the scene will hang
        yield return ScenePreloader.ForceEndPendingOperations();
        // CustomSceneManager.Start fails to run if the scene is immediately unloaded. Leaves the scene dark on next respawn.
        // Without this wait, on the next respawn the scene stays black.
        if (abortDeath && ScenePreloader._forceEndedOperations.Count > 0)
            yield return null;
        foreach (ScenePreloader.SceneLoadOp op in ScenePreloader._forceEndedOperations)
        {
            yield return Addressables.UnloadSceneAsync(op.Operation);
        }
        ScenePreloader._forceEndedOperations.Clear();
    }

    private partial IEnumerator LoadDummyScene()
    {
        string dummyScene = "Demo Start";
        Addressables.LoadSceneAsync($"Scenes/{dummyScene}");
        yield return new WaitUntil(() => USceneManager.GetActiveScene().name == dummyScene);
    }

    private partial IEnumerator LoadDupedScene(string sceneName)
    {
        yield return Addressables.LoadSceneAsync($"Scenes/{sceneName}", LoadSceneMode.Additive);
    }

    partial void SaveGameSpecific()
    {
        data.evoState = HeroController.instance.hunterUpgState.CurrentMeterHits;
        data.isMaggoted = HeroController.instance.cState.isMaggoted;
        
        data.semiPersistentBools = SaveSemiPersistent(SceneData.instance.persistentBools);
        data.semiPersistentInts = SaveSemiPersistent(SceneData.instance.persistentInts);
    }

    partial void RestoreGameSpecific()
    {
        RestoreSemiPersistent(data.semiPersistentBools, SceneData.instance.persistentBools);
        RestoreSemiPersistent(data.semiPersistentInts, SceneData.instance.persistentInts);
    }

    private partial void HUDFixes()
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

        // Resets maggots, lifeblood overdose, buff tools, etc.
        HeroController.instance.ClearEffects();

        // Need to be done after ClearEffects()
        if (data.isMaggoted)
        {
            HeroController.instance.cState.isMaggoted = true; // Avoids sound effect
            HeroController.instance.SetIsMaggoted(true);
            Utils.FindFSM("Maggots", "Maggot Effect").SetState("Is Maggoted?");
        }

        HeroController.instance.hunterUpgState = new HeroController.HunterUpgCrestStateInfo { CurrentMeterHits = data.evoState };

        // Reset timers
        if (ToolItemManager.IsToolEquipped("Sprintmaster"))
        {
            HeroController.instance.gameObject.LocateMyFSM("Sprint Silk Usage").SetState("Reset Timer");
        }
        HeroController.instance.SetFrostAmount(0f);

        // TODO: restore blue health instead of clearing it
        PlayerData.instance.healthBlue = 0;
        EventRegister.SendEvent("HEALTH UPDATE");

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
}
