using DebugMod.CommandPalette;
using DebugMod.Helpers;
using DebugMod.SaveStates;
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
                Localization.Get(category.Key),
                () => category.Select(action => new CommandPaletteItem.ActionItem(Localization.Get(action.Name), action.Action))
            );
        }
        
        registry.RegisterSubmenu("Savestate files", FileSavestates);
        registry.RegisterSubmenu("Teleport", TeleportPoints);
    }
    
    #region Teleport

    private static IEnumerable<CommandPaletteItem> TeleportPoints()
    {
        Dictionary<string, SceneTeleportMap.SceneInfo> teleportMap = SceneTeleportMap.GetTeleportMap();
        if (teleportMap == null) yield break;

        foreach (var scene in teleportMap.Where(s => s.Value.TransitionGates.Count > 0).OrderBy(s => s.Key))
        {
            yield return new CommandPaletteItem.SubmenuItem(
                scene.Key,
                () => scene.Value.TransitionGates.OrderBy(g => g).Select(gate =>
                    new CommandPaletteItem.ActionItem(gate, () => Teleport(scene.Key, gate))),
                searchChildren: false
            );
        }
    }

    private static void Teleport(string scene, string gate)
    {
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
            yield return new CommandPaletteItem.ActionItem(state.ToString(), () => SaveStateManager.LoadState(state));
        }
    }
    
    #endregion
}
