using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod;

[Flags]
public enum Modifier
{
    None = 0,
    Control = 1 << 0,
    Shift = 1 << 1,
    Alt = 1 << 2,
    Meta = 1 << 3,
}

internal static class ModifierExtensions
{
    internal static IEnumerable<Modifier> Active(this Modifier modifiers)
    {
        foreach (Modifier flag in Enum.GetValues(typeof(Modifier)))
        {
            if (flag != Modifier.None && modifiers.HasFlag(flag)) yield return flag;
        }
    }

    internal static Modifier Held() =>
        (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? Modifier.Shift : 0) |
        (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? Modifier.Control : 0) |
        (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? Modifier.Alt : 0) |
        (Input.GetKey(KeyCode.LeftMeta) || Input.GetKey(KeyCode.RightMeta) ? Modifier.Meta : 0);

    internal static bool IsModifierKey(KeyCode key) => FromKeyCode(key) != Modifier.None;

    internal static Modifier FromKeyCode(KeyCode key) => key switch
    {
        KeyCode.LeftControl or KeyCode.RightControl => Modifier.Control,
        KeyCode.LeftShift or KeyCode.RightShift => Modifier.Shift,
        KeyCode.LeftAlt or KeyCode.RightAlt => Modifier.Alt,
        KeyCode.LeftMeta or KeyCode.RightMeta => Modifier.Meta,
        _ => Modifier.None
    };

    internal static KeyCode ToKeyCode(this Modifier modifier) => modifier switch
    {
        Modifier.Control => KeyCode.LeftControl,
        Modifier.Shift => KeyCode.LeftShift,
        Modifier.Alt => KeyCode.LeftAlt,
        Modifier.Meta => KeyCode.LeftMeta,
        _ => throw new ArgumentException($"Cannot convert {modifier} to a key code", nameof(modifier))
    };
}
