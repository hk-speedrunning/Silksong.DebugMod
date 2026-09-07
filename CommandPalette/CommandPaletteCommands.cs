using DebugMod.Helpers;
using DebugMod.SaveStates;
using System.Collections.Generic;
using System.Linq;

namespace DebugMod.CommandPalette;

public static class CommandPaletteCommands
{
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
}
