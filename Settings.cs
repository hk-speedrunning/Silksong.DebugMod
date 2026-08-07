using BepInEx.Configuration;
using DebugMod.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod;

public class Settings
{
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public Dictionary<string, KeyCode> binds = new();

    private string lastLoadedPack = "";
    private string mainPanelCurrentTab;
    private int showHitBoxes;
    private bool showCursorWhileUnpaused;

    private bool mainPanelVisible = true;
    private bool enemiesPanelVisible = true;
    private bool consoleVisible = true;
    private bool infoPanelVisible = true;
    private bool saveStatePanelVisible = true;
    private bool saveStatePanelExpanded = false;

    private bool logUnityExceptions = true;

    private static ConfigEntry<KeyCode> toggleAllUI;
    private static ConfigEntry<float> noclipSpeedModifier;
    private static ConfigEntry<bool> altInfoPanel;
    private static ConfigEntry<bool> expandedInfoPanel;

    private static ConfigEntry<bool> numpadForSavestates;
    private static ConfigEntry<bool> safeSavestateLoading;

    public string LastLoadedPack
    {
        get => lastLoadedPack;
        set
        {
            lastLoadedPack = value;
            DebugMod.SaveSettings();
        }
    }

    public string MainPanelCurrentTab
    {
        get => mainPanelCurrentTab;
        set
        {
            mainPanelCurrentTab = value;
            DebugMod.SaveSettings();
        }
    }

    public int ShowHitBoxes
    {
        get => showHitBoxes;
        set
        {
            showHitBoxes = value;
            DebugMod.SaveSettings();
        }
    }

    public bool ShowCursorWhileUnpaused
    {
        get => showCursorWhileUnpaused;
        set
        {
            showCursorWhileUnpaused = value;
            DebugMod.SaveSettings();
        }
    }

    public bool MainPanelVisible
    {
        get => mainPanelVisible;
        set
        {
            mainPanelVisible = value;
            DebugMod.SaveSettings();
        }
    }

    public bool EnemiesPanelVisible
    {
        get => enemiesPanelVisible;
        set
        {
            enemiesPanelVisible = value;
            DebugMod.SaveSettings();
        }
    }

    public bool ConsoleVisible
    {
        get => consoleVisible;
        set
        {
            consoleVisible = value;
            DebugMod.SaveSettings();
        }
    }

    public bool InfoPanelVisible
    {
        get => infoPanelVisible;
        set
        {
            infoPanelVisible = value;
            DebugMod.SaveSettings();
        }
    }

    public bool SaveStatePanelVisible
    {
        get => saveStatePanelVisible;
        set
        {
            saveStatePanelVisible = value;
            DebugMod.SaveSettings();
        }
    }

    public bool SaveStatePanelExpanded
    {
        get => saveStatePanelExpanded;
        set
        {
            saveStatePanelExpanded = value;
            DebugMod.SaveSettings();
        }
    }

    public bool LogUnityExceptions
    {
        get => logUnityExceptions;
        set
        {
            logUnityExceptions = value;
            DebugMod.SaveSettings();
        }
    }

    public float NoClipSpeedModifier
    {
        get => noclipSpeedModifier.Value;
        set
        {
            noclipSpeedModifier.Value = value;
            DebugMod.SaveSettings();
        }
    }

    public bool AltInfoPanel
    {
        get => altInfoPanel.Value;
        set
        {
            altInfoPanel.Value = value;
            DebugMod.SaveSettings();
        }
    }

    public bool ExpandedInfoPanel
    {
        get => expandedInfoPanel.Value;
        set
        {
            expandedInfoPanel.Value = value;
            DebugMod.SaveSettings();
        }
    }

    public bool NumPadForSaveStates
    {
        get => numpadForSavestates.Value;
        set
        {
            numpadForSavestates.Value = value;
            DebugMod.SaveSettings();
        }
    }

    public bool SafeSaveStateLoading
    {
        get => safeSavestateLoading.Value;
        set
        {
            safeSavestateLoading.Value = value;
            DebugMod.SaveSettings();
        }
    }

    internal void InitMenu(ConfigFile config)
    {
        // We store all the settings ourselves
        config.SaveOnConfigSet = false;

        string toggleAllUIName = "MODUI_TOGGLEALLUI";

        toggleAllUI = config.Bind(
            "General",
            "Toggle All UI Keybind",
            KeyCode.F2,
            "Press this key to toggle DebugMod's UI."
        );
        toggleAllUI.SettingChanged += (_, _) =>
        {
            if (toggleAllUI.Value == KeyCode.None)
            {
                DebugMod.UpdateBind(toggleAllUIName, null);
            }
            else
            {
                DebugMod.UpdateBind(toggleAllUIName, toggleAllUI.Value);
            }
        };
        DebugMod.bindUpdated += (name, key) =>
        {
            if (name == toggleAllUIName)
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

        altInfoPanel = config.Bind(
            "General",
            "Alternate Info Panel Style",
            false,
            "Adds some decoration to the info panel."
        );
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
        expandedInfoPanel.SettingChanged += (_, _) =>
        {
            if (ExpandedInfoPanel != expandedInfoPanel.Value)
            {
                ExpandedInfoPanel = expandedInfoPanel.Value;
                InfoPanel.Instance.Destroy();
                InfoPanel.BuildPanel();
            }
        };

        numpadForSavestates = config.Bind(
            "Savestates",
            "Savestate Numpad Hotkeys",
            false,
            "Use the numpad keys instead of the regular number keys to select file states in the savestate panel. Takes effect on restart."
        );

        safeSavestateLoading = config.Bind(
            "Savestates",
            "Safe Savestate Loading",
            false,
            "Fixes some obscure issues when using savestates, but makes loading take longer."
        );
    }
}
