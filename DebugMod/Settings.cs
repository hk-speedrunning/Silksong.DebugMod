using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace DebugMod;

public class Settings
{
    //Save members
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public Dictionary<string, KeyCode> binds = new();

    public bool ConsoleVisible = true;

    public bool EnemiesPanelVisible = true;

    public bool HelpPanelVisible = true;

    public bool InfoPanelVisible = true;

    public string CurrentInfoPanelName = "";

    public bool SaveStatePanelVisible = true;
    
    public bool MainPanelVisible = true;

    public bool FirstRun = true;

    public bool NumPadForSaveStates = false;
    
    public int ShowHitBoxes;

    public int MaxSaveStates = 10;

    public int MaxSavePages = 10;

    public float NoClipSpeedModifier = 1f;

    public bool ShowCursorWhileUnpaused = false;

    public bool SaveStateGlitchFixes = true;

    //TODO Get rid of this variable and implement an actual clear method
    public bool ClearSaveStatePanel = false;

    public bool LogUnityExceptions = true;
}
