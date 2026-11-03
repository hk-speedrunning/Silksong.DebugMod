using DebugMod.Helpers;
using DebugMod.SaveStates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DebugMod.UI.CommandPalette;

public static class CommandPaletteCommands
{
    public static void Initialize()
    {
        CommandPaletteRegistry registry = DebugMod.CommandPaletteRegistry;
        
        foreach (var category in DebugMod.bindActions.Values.GroupBy(action => action.Category))
        {
            registry.RegisterSubmenu(
                () => Localization.Get(category.Key),
                () => category.Select(action => new CommandPaletteItem.ActionItem(() => Localization.Get(action.Name), action.Action))
            );
        }
        
        registry.RegisterSubmenu(() => Localization.Get("COMMANDPALETTE_SAVESTATE_FILES"), FileSavestates);
        registry.RegisterSubmenu(() => Localization.Get("COMMANDPALETTE_WARP"), TeleportPoints);
    }
    
    #region Teleport

    private static readonly Lazy<Dictionary<string, List<string>>> scenesByArea =
        new(() => JsonUtil.Deserialize<Dictionary<string, List<string>>>("DebugMod.Resources.scenes_by_area.jsonc"));

    private static IEnumerable<CommandPaletteItem> TeleportPoints()
    {
        Dictionary<string, SceneTeleportMap.SceneInfo> teleportMap = SceneTeleportMap.GetTeleportMap();
        if (teleportMap == null) yield break;

        foreach ((string area, List<string> scenes) in scenesByArea.Value)
        {
            yield return new CommandPaletteItem.SubmenuItem(area, () => scenes.Select(scene =>
                new CommandPaletteItem.SubmenuItem(
                    scene,
                    () => teleportMap[scene].TransitionGates.OrderBy(g => g).Select(gate =>
                        new CommandPaletteItem.ActionItem(gate, () => GameManager.instance.StartCoroutine(Teleport(scene, gate)))),
                    searchChildren: false
                )));
        }
    }

    private static IEnumerator Teleport(string scene, string gate)
    {
        if (UIManager.instance.uiState.ToString() == "PAUSED")
            yield return GameManager.instance.PauseGameToggle(false);
        
        GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
        {
            SceneName = scene,
            EntryGateName = gate,
        });
    }
    
    #endregion
    
    #region Savestates

    private static IEnumerable<CommandPaletteItem> FileSavestates()
    {
        foreach (SaveState state in SaveStateManager.AllSavestates)
        {
            SaveState snapshot = new() { data = state.data.DeepCopy() };
            yield return new CommandPaletteItem.ActionItem(state.ToString(), () =>
            {
                SaveStateManager.SetQuickState(snapshot);
                SaveStateManager.LoadState(SaveStateManager.GetQuickState());
            });
        }
    }
    
    #endregion
}
