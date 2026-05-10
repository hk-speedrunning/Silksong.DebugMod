using DebugMod.Helpers;
using System;
using System.Reflection;

namespace DebugMod;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class BindableMethod : Attribute
{
    public string name;
    public string category;
    public bool allowLock = true;
    public string translationKey = null;
}

public class BindAction
{
    public string Name { get; }
    public string TranslationKey { get; }
    public string Category { get; }
    public bool AllowLock { get; }
    public Action Action { get; }

    public BindAction(string name, string category, bool allowLock, Action action, string translationKey = null)
    {
        // Keybinds are tracked by name in English
        if (translationKey != null)
        {
            Name = name;
            TranslationKey = translationKey;
        }
        else if (Localization.FallbackSheet.TryGetValue(name, out string translated))
        {
            Name = translated;
            TranslationKey = name;
        }
        else
        {
            Name = name;
            TranslationKey = "";
        }

        Category = category;
        AllowLock = allowLock;
        Action = action;
    }

    public BindAction(BindableMethod attribute, MethodInfo method)
        : this(
            attribute.name,
            attribute.category,
            attribute.allowLock,
            (Action)Delegate.CreateDelegate(typeof(Action), method),
            attribute.translationKey
        )
    { }

    public string Localize()
    {
        if (TranslationKey != "")
        {
            return Localization.Get(TranslationKey);
        }

        return Name;
    }
}