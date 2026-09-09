using System;
using System.Collections.Generic;

namespace DebugMod.CommandPalette;

public abstract class CommandPaletteItem(string title, string detail = null)
{
    public string Title { get; } = title;
    public string Detail { get; } = detail;

    public sealed class ToggleItem(
        string title,
        Func<bool> isEnabled,
        Action toggle,
        string detail = null)
        : CommandPaletteItem(title, detail)
    {
        public Func<bool> IsEnabled { get; } = isEnabled;
        public Action Toggle { get; } = toggle;
    }

    public sealed class ActionItem(string title, Action execute, string detail = null)
        : CommandPaletteItem(title, detail)
    {
        public Action Execute { get; } = execute;
    }

    public sealed class SubmenuItem(
        string title,
        Func<IEnumerable<CommandPaletteItem>> getChildren,
        string detail = null,
        bool searchChildren = true)
        : CommandPaletteItem(title, detail)
    {
        public Func<IEnumerable<CommandPaletteItem>> GetChildren { get; } = getChildren;
        public bool SearchChildren { get; } = searchChildren;
    }
}
