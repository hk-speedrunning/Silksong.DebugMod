using BepInEx.Configuration;
using DebugMod.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod;

public class Settings
{
    private static ConfigEntry<KeyCode> toggleAllUI;
    private static ConfigEntry<float> noclipSpeedModifier;
    private static ConfigEntry<bool> altInfoPanel;
    private static ConfigEntry<bool> expandedInfoPanel;

    private static ConfigEntry<int> maxSavestatePages;
    private static ConfigEntry<bool> numpadForSavestates;
    private static ConfigEntry<bool> savestateGlitchFixes;
    private static ConfigEntry<bool> safeSavestateLoading;

    internal void InitMenu(ConfigFile config)
    {
        // We store all the settings ourselves
        config.SaveOnConfigSet = false;

        toggleAllUI = config.Bind(
            "General",
            "Toggle All UI Keybind",
            KeyCode.F2,
            "Press this key to toggle DebugMod's UI."
        );
        toggleAllUI.Value = binds.GetValueOrDefault("Toggle All UI", KeyCode.None);
        toggleAllUI.SettingChanged += (_, _) =>
        {
            if (toggleAllUI.Value == KeyCode.None)
            {
                DebugMod.UpdateBind("Toggle All UI", null);
            }
            else
            {
                DebugMod.UpdateBind("Toggle All UI", toggleAllUI.Value);
            }
        };
        DebugMod.bindUpdated += (name, key) =>
        {
            if (name == "Toggle All UI")
            {
                toggleAllUI.Value = key ?? KeyCode.None;
            }
        };

        noclipSpeedModifier = config.Bind(
            "General",
            "Noclip Speed Multiplier",
            1f,
            "You can also hold shift in noclip to get an additional 2x multiplier."
        );
        noclipSpeedModifier.Value = NoClipSpeedModifier;
        noclipSpeedModifier.SettingChanged += (_, _) => NoClipSpeedModifier = noclipSpeedModifier.Value;

        altInfoPanel = config.Bind(
            "General",
            "Alternate Info Panel Style",
            false,
            "Adds some decoration to the info panel."
        );
        altInfoPanel.Value = AltInfoPanel;
        altInfoPanel.SettingChanged += (_, _) =>
        {
            if (AltInfoPanel != altInfoPanel.Value)
            {
                AltInfoPanel = altInfoPanel.Value;
                InfoPanel.Instance.Destroy();
                InfoPanel.BuildPanel();
            }
        };

        expandedInfoPanel = config.Bind(
            "General",
            "Expanded Info Panel",
            false,
            "Shows additional niche info on the info panel."
        );
        expandedInfoPanel.Value = ExpandedInfoPanel;
        expandedInfoPanel.SettingChanged += (_, _) =>
        {
            if (ExpandedInfoPanel != expandedInfoPanel.Value)
            {
                ExpandedInfoPanel = expandedInfoPanel.Value;
                InfoPanel.Instance.Destroy();
                InfoPanel.BuildPanel();
            }
        };

        maxSavestatePages = config.Bind(
            "Savestates",
            "Number of Savestate Pages",
            10,
            "The number of pages of available savestates. Takes effect on restart."
        );
        maxSavestatePages.Value = MaxSavePages;
        maxSavestatePages.SettingChanged += (_, _) =>
        {
            if (maxSavestatePages.Value > 0)
            {
                MaxSavePages = maxSavestatePages.Value;
            }
        };

        numpadForSavestates = config.Bind(
            "Savestates",
            "Numpad Hotkeys",
            false,
            "Use the numpad keys instead of the regular number keys to select file states in the savestate panel. Takes effect on restart."
        );
        numpadForSavestates.Value = NumPadForSaveStates;
        numpadForSavestates.SettingChanged += (_, _) => NumPadForSaveStates = numpadForSavestates.Value;

        savestateGlitchFixes = config.Bind(
            "Savestates",
            "Savestate Glitch Fixes",
            true,
            "Automatically clear glitched state (such as forms of storage) when loading a savestate."
        );
        savestateGlitchFixes.Value = SaveStateGlitchFixes;
        savestateGlitchFixes.SettingChanged += (_, _) => SaveStateGlitchFixes = savestateGlitchFixes.Value;

        safeSavestateLoading = config.Bind(
            "Savestates",
            "Safe Savestate Loading",
            false,
            "Fixes some obscure issues when using savestates, but makes loading take longer."
        );
        safeSavestateLoading.Value = SafeSaveStateLoading;
        safeSavestateLoading.SettingChanged += (_, _) => SafeSaveStateLoading = safeSavestateLoading.Value;
    }

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public Dictionary<string, KeyCode> binds = new();

    public bool FirstRun = true;

    public bool MainPanelVisible = true;

    public string MainPanelCurrentTab;

    public bool EnemiesPanelVisible = true;

    public bool ConsoleVisible = true;

    public bool InfoPanelVisible = true;

    public bool SaveStatePanelVisible = true;

    public bool SaveStatePanelExpanded = false;

    public bool NumPadForSaveStates = false;

    public int ShowHitBoxes;

    public int MaxSavePages = 10;

    public float NoClipSpeedModifier = 1f;

    public bool ShowCursorWhileUnpaused = false;

    public bool SaveStateGlitchFixes = true;

    public bool SafeSaveStateLoading = false;

    public bool LogUnityExceptions = true;

    public bool AltInfoPanel = false;

    public bool ExpandedInfoPanel = false;
}
