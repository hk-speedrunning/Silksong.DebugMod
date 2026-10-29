using System;
using System.Collections.Generic;

namespace DebugMod.UI.CommandPalette;

public abstract class CommandPaletteItem(Func<string> title)
{
    public Func<string> Title { get; } = title;

    public sealed class ToggleItem(
        Func<string> title,
        Func<bool> isEnabled,
        Action toggle)
        : CommandPaletteItem(title)
    {
        public ToggleItem(string title, Func<bool> isEnabled, Action toggle)
            : this(() => title, isEnabled, toggle) { }

        public Func<bool> IsEnabled { get; } = isEnabled;
        public Action Toggle { get; } = toggle;
    }

    public sealed class ActionItem(Func<string> title, Action execute)
        : CommandPaletteItem(title)
    {
        public ActionItem(string title, Action execute)
            : this(() => title, execute) { }

        public Action Execute { get; } = execute;
    }

    public sealed class SubmenuItem(
        Func<string> title,
        Func<IEnumerable<CommandPaletteItem>> getChildren,
        bool searchChildren = true)
        : CommandPaletteItem(title)
    {
        public SubmenuItem(string title, Func<IEnumerable<CommandPaletteItem>> getChildren, bool searchChildren = true)
            : this(() => title, getChildren, searchChildren) { }

        public Func<IEnumerable<CommandPaletteItem>> GetChildren { get; } = getChildren;
        public bool SearchChildren { get; } = searchChildren;
    }
}
