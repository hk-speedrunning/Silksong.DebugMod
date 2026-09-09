using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace DebugMod;

[JsonConverter(typeof(BindingJsonConverter))]
public readonly struct Binding : IEquatable<Binding>
{
    public Modifier Modifiers { get; }
    public KeyCode Key { get; }

    public Binding(Modifier modifiers, KeyCode key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    public Binding(KeyCode key) : this(Modifier.None, key) {}

    public static implicit operator Binding(KeyCode key) => new(key);
    
    #region Boilerplate

    public bool Equals(Binding other) => Modifiers == other.Modifiers && Key == other.Key;
    public override bool Equals(object obj) => obj is Binding other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Modifiers, Key);
    public static bool operator ==(Binding left, Binding right) => left.Equals(right);
    public static bool operator !=(Binding left, Binding right) => !(left == right);

    public bool IsDown() => IsDown(ModifierExtensions.Held());
    public bool IsDown(Modifier modifiers) => Key != KeyCode.None && Input.GetKeyDown(Key) 
                                                                  && (modifiers & Modifiers) == Modifiers;

    public override string ToString()
    {
        if (Modifiers == Modifier.None) return Key.ToString();
        return string.Join("+", Modifiers.Active()) + "+" + Key;
    }
    
    #endregion
    
    #region Parsing

    private static Binding Parse(string value)
    {
        return TryParse(value, out Binding binding) ? binding : throw new ArgumentException($"Invalid binding '{value}'");
    }
    
    private static bool TryParse(string value, out Binding binding)
    {
        binding = default;
        if (string.IsNullOrEmpty(value)) return false;

        string[] parts = value.Split('+');
        if (!Enum.TryParse(parts[^1], ignoreCase: true, out KeyCode key)) return false;

        Modifier modifiers = Modifier.None;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!Enum.TryParse(parts[i], ignoreCase: true, out Modifier flag)) return false;
            modifiers |= flag;
        }

        binding = new Binding(modifiers, key);
        return true;
    }
    
    internal static void RegisterTomlConverter()
    {
        if (TomlTypeConverter.CanConvert(typeof(Binding))) return;
        TomlTypeConverter.AddConverter(typeof(Binding), new TypeConverter
        {
            ConvertToString = (value, _) => value.ToString(),
            ConvertToObject = (value, _) => Parse(value),
        });
    }

    private sealed class BindingJsonConverter : JsonConverter<Binding>
    {
        public override void WriteJson(JsonWriter writer, Binding value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Binding ReadJson(JsonReader reader, Type objectType, Binding existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value switch
            {
                string name => Parse(name),
                null => default,
                _ => throw new ArgumentException($"Invalid binding '{reader.Value}'")
            };
        }
    }
    
    #endregion
}
