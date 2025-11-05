using System;
using System.Collections.Generic;
using DebugMod.UI.Canvas;
using JetBrains.Annotations;
using UnityEngine;

namespace DebugMod.UI;

/// <summary>
/// Represents an info panel in the style of the standard info panel
/// </summary>
public abstract class InfoPanel : CanvasPanel
{
    public const string MainInfoPanelName = "DebugMod.MainInfoPanel";
    public const string MinimalInfoPanelName = "DebugMod.MinimalInfoPanel";

    private static Dictionary<string, InfoPanel> AllPanels = new();
    private static List<string> TogglablePanelNames = new() { MainInfoPanelName, MinimalInfoPanelName };

    public static void ToggleActivePanel()
    {
        int i = TogglablePanelNames.IndexOf(DebugMod.settings.CurrentInfoPanelName);
        i = (i + 1) % TogglablePanelNames.Count;
        DebugMod.settings.CurrentInfoPanelName = TogglablePanelNames[i];
    }

    public static void BuildInfoPanels()
    {
        AllPanels.Add(MainInfoPanelName, CustomInfoPanel.BuildMainInfoPanel());
        AllPanels.Add(MinimalInfoPanelName, CustomInfoPanel.BuildMinimalInfoPanel());
        AllPanels.Add("DebugMod.BottomRightInfoPanel", new BottomRightInfoPanel());

        foreach (InfoPanel panel in AllPanels.Values)
        {
            panel.Build();
        }

        if (!TogglablePanelNames.Contains(DebugMod.settings.CurrentInfoPanelName))
        {
            DebugMod.settings.CurrentInfoPanelName = MainInfoPanelName;
        }
    }

    public static void UpdatePanels()
    {
        foreach (InfoPanel panel in AllPanels.Values)
        {
            panel.ActiveSelf = DebugMod.settings.InfoPanelVisible && DebugMod.settings.CurrentInfoPanelName == panel.Name;
        }
    }

    protected InfoPanel(string name, CanvasNode parent, Vector2 position, Vector2 size)
        : base(name)
    {
        Parent = parent;
        LocalPosition = position;
        Size = size;
    }

    #region Custom Panel API

    /// <summary>
    /// Add an info entry to the specified info panel.
    /// </summary>
    /// <param name="Name">The name of the panel.</param>
    /// <param name="xLabel">The x coordinate of the label.</param>
    /// <param name="xInfo">The x coordinate of the info string.</param>
    /// <param name="y">The y coordinate of the entry.</param>
    /// <param name="label">The text to display on the label.</param>
    /// <param name="textFunc">A function that returns the text to show on the info string; will be called every frame.</param>
    [PublicAPI]
    public static void AddInfoToPanel(string Name, float xLabel, float xInfo, float y, string label, Func<string> textFunc)
    {
        ((CustomInfoPanel)AllPanels[Name]).AddInfo(xLabel, xInfo, y, label, textFunc);
    }

    /// <summary>
    /// Add an info entry to the specified simple info panel.
    /// </summary>
    /// <param name="Name">The name of the panel.</param>
    /// <param name="label">The text to display on the label.</param>
    /// <param name="textFunc">A function that returns the text to show on the info string; will be called every frame.</param>
    [PublicAPI]
    public static void AddInfoToSimplePanel(string Name, string label, Func<string> textFunc)
    {
        ((SimpleInfoPanel)AllPanels[Name]).AddInfo(label, textFunc);
    }

    /// <summary>
    /// Add an info panel to the rotation. Must be done during mod initialization.
    /// </summary>
    /// <param name="Name">The name of the panel.</param>
    /// <param name="p">The panel to add.</param>
    /// <exception cref="InvalidOperationException">A panel with this name already exists.</exception>
    [PublicAPI]
    public static void AddInfoPanel(string Name, InfoPanel p)
    {
        if (AllPanels.ContainsKey(Name))
        {
            throw new InvalidOperationException("A panel with this name already exists");
        }

        AllPanels.Add(Name, p);
        TogglablePanelNames.Add(Name);
    }
    #endregion

    public static string GetTransState()
    {
        string transState = HeroController.instance.transitionState.ToString();
        if (transState == "WAITING_TO_ENTER_LEVEL") transState = "LOADING";
        if (transState == "WAITING_TO_TRANSITION") transState = "WAITING";
        return transState;
    }
    public static string GetHeroPos()
    {
        if (DebugMod.RefKnight == null)
        {
            return string.Empty;
        }
        float HeroX = DebugMod.RefKnight.transform.position.x;
        float HeroY = DebugMod.RefKnight.transform.position.y;

        return $"({HeroX}, {HeroY})";
    }
    public static string GetStringForBool(bool b)
    {
        return b ? "✓" : "X";
    }
}
