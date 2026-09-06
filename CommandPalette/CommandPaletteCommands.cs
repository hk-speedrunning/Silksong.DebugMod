using DebugMod.Helpers;
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

        return registry;
    }
}
