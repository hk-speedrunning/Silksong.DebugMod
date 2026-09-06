using DebugMod.Helpers;
using DebugMod.SaveStates;
using GlobalEnums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DebugMod.CommandPalette;

public static class CommandPaletteCommands
{
    private static readonly Dictionary<string, List<string>> transitionPoints = LoadTransitionPoints();

    public static CommandPaletteRegistry CreateRegistry()
    {
        CommandPaletteRegistry registry = new();
        
        foreach (var category in DebugMod.bindActions.Values.GroupBy(action => action.Category))
        {
            registry.RegisterSubmenu(
                Localization.Get(category.Key),
                () => category.Select(action => new CommandPaletteItem.ActionItem(Localization.Get(action.Name), action.Action))
            );
        }
        
        registry.RegisterSubmenu("Savestate files", FileSavestates);
        registry.RegisterSubmenu("Teleport", TeleportPoints);

        return registry;
    }
    
    #region Savestates

    private static IEnumerable<CommandPaletteItem> FileSavestates()
    {
        foreach (SaveState state in SaveStateManager.AllSavestates)
        {
            yield return new CommandPaletteItem.ActionItem(state.ToString(), () => SaveStateManager.LoadState(state));
        }
    }
    
    #endregion
    
    #region Teleport

    private static IEnumerable<CommandPaletteItem> TeleportPoints()
        => transitionPoints.Select(scene => new CommandPaletteItem.SubmenuItem(
            scene.Key,
            () => scene.Value.Select(gate => new CommandPaletteItem.ActionItem( gate, () => Teleport(scene.Key, gate))),
            searchChildren: false)
        );

    private static void Teleport(string scene, string gate)
    {
        GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
        {
            SceneName = scene,
            EntryGateName = gate,
        });
    }

    private static Dictionary<string, List<string>> LoadTransitionPoints()
    {
        using Stream stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("DebugMod.CommandPalette.TransitionPoints.json")!;
        using StreamReader reader = new(stream);
        return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(reader.ReadToEnd());
    }

    #endregion
}
