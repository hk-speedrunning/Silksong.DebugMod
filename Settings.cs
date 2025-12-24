using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace DebugMod;

public class Settings
{
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public Dictionary<string, KeyCode> binds = new();

    public bool FirstRun = true;

    public bool MainPanelVisible = true;

    public bool EnemiesPanelVisible = true;

    public bool ConsoleVisible = true;

    public bool InfoPanelVisible = true;

    public bool SaveStatePanelVisible = true;

    public bool NumPadForSaveStates = false;
    
    public int ShowHitBoxes;

    public int MaxSavePages = 10;

    public float NoClipSpeedModifier = 1f;

    public bool ShowCursorWhileUnpaused = false;

    public bool SaveStateGlitchFixes = true;

    public bool LogUnityExceptions = true;
}
