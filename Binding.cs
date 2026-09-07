using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace DebugMod;

[JsonConverter(typeof(BindingJsonConverter))]
public readonly struct Binding : IEquatable<Binding>
{
    public KeyCode Key { get; }

    public Binding(KeyCode key)
    {
        Key = key;
    }

    public static implicit operator Binding(KeyCode key) => new(key);
    
    #region Boilerplate

    public bool Equals(Binding other) => Key == other.Key;
    public override bool Equals(object obj) => obj is Binding other && Equals(other);
    public override int GetHashCode() => Key.GetHashCode();
    public static bool operator ==(Binding left, Binding right) => left.Equals(right);
    public static bool operator !=(Binding left, Binding right) => !(left == right);

    public bool IsDown() => Key != KeyCode.None && Input.GetKeyDown(Key);

    public override string ToString() => Key.ToString();
    
    #endregion
    
    #region Parsing

    private static Binding Parse(string value)
    {
        return TryParse(value, out Binding binding) ? binding : throw new ArgumentException($"Invalid binding '{value}'");
    }
    
    private static bool TryParse(string value, out Binding binding)
    {
        if (!Enum.TryParse(value, out KeyCode keyCode))
        {
            binding = default;
            return false;
        }

        binding = new Binding(keyCode);
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
