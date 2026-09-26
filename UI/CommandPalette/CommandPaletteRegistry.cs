using System;
using System.Collections.Generic;

namespace DebugMod.UI.CommandPalette;

public sealed class CommandPaletteRegistry
{
    private readonly List<CommandPaletteItem> rootItems = [];

    public IEnumerable<CommandPaletteItem> RootItems => rootItems;

    public void Register(CommandPaletteItem item) => rootItems.Add(item);

    public void RegisterSubmenu(Func<string> title, Func<IEnumerable<CommandPaletteItem>> getChildren)
        => Register(new CommandPaletteItem.SubmenuItem(title, getChildren));
}
