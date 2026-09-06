using DebugMod.Helpers;
using DebugMod.UI;
using DebugMod.UI.Canvas;
using DebugMod.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DebugMod.CommandPalette;

public sealed class CommandPaletteController : MonoBehaviour
{
    #region UI Properties

    private const int MaxVisibleItems = 8;
    private const float RepeatInitialDelay = .2f;
    private const float RepeatInterval = .05f;
    private const int SelectionThickness = 2;
    private const string SubmenuIndicator = "›";
    private static int PanelWidth => UICommon.ScaleWidth(560);
    private static int HorizontalPadding => UICommon.ScaleWidth(12);
    private static int VerticalPadding => UICommon.ScaleHeight(12);
    private static int RowHeight => UICommon.ScaleHeight(30);
    private static int RowPadding => UICommon.ScaleHeight(4);
    private static int RowsTop => VerticalPadding + RowHeight + VerticalPadding;

    private static int PanelHeight =>
        RowsTop + MaxVisibleItems * RowHeight + (MaxVisibleItems - 1) * RowPadding + VerticalPadding;

    private static int PanelTop => UICommon.ScaleHeight(400);
    private static int QueryTextPadding => UICommon.ScaleWidth(6);
    private static int DetailWidth => UICommon.ScaleWidth(140);
    private static int DetailPadding => UICommon.ScaleWidth(10);

    #endregion

    private static CommandPaletteController _instance;
    private static int? _closedFrame;

    private CommandPaletteRegistry registry;
    private CanvasPanel panel;
    private CanvasTextField queryField;
    private CanvasText placeholderText;
    private readonly List<PaletteRow> rows = [];
    private readonly List<CommandPaletteItem.SubmenuItem> navigation = [];
    private List<PaletteEntry> filteredItems = [];
    private string query = "";
    private int selectedIndex;
    private KeyCode repeatingNavigationKey;
    private float nextNavigationRepeat;
    private string queryBeforeWordDelete;
    private readonly object freezeOwner = new();
    private readonly object inputBlocker = new();

    public static bool IsOpen => _instance != null && _instance.panel != null && _instance.panel.ActiveSelf;
    public static bool IsInputBlocked => IsOpen || _closedFrame == Time.frameCount;

    #region Lifecycle Methods

    public static void Build()
    {
        _instance = GUIController.Instance.gameObject.AddComponent<CommandPaletteController>();
        _instance.registry = CommandPaletteCommands.CreateRegistry();
    }

    public static void Unload()
    {
        if (_instance == null) return;
        if (IsOpen) _instance.Close();
        _instance.panel?.Destroy();
        _instance = null;
    }

    private void OnDestroy()
    {
        try
        {
            TimeScale.VoteFreeze(freezeOwner, false);
            SetGameInputBlocked(false);
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Error during command palette OnDestroy: {e}");
        }
    }

    private void Update()
    {
        try
        {
            HandleKeybindings();
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Error during command palette Update: {e}");
        }
    }

    private void LateUpdate()
    {
        try
        {
            HandleWordDeletion();
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Error during command palette LateUpdate: {e}");
        }
    }
    
    #endregion

    private void HandleKeybindings()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (IsOpen) Close();
            else Open();
            return;
        }

        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Backspace) &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            queryBeforeWordDelete = queryField.Text;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (navigation.Count == 0) Close();
            else NavigateBack();
            return;
        }

        HandleNavigationRepeat();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) ActivateSelected();
    }

    #region Navigation

    private void HandleNavigationRepeat()
    {
        if (repeatingNavigationKey != KeyCode.None && !Input.GetKey(repeatingNavigationKey)) repeatingNavigationKey = KeyCode.None;

        if (Input.GetKeyDown(KeyCode.UpArrow)) StartNavigationRepeat(KeyCode.UpArrow);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) StartNavigationRepeat(KeyCode.DownArrow);

        if (repeatingNavigationKey != KeyCode.None && Time.realtimeSinceStartup >= nextNavigationRepeat)
        {
            RunNavigationAction(repeatingNavigationKey);
            nextNavigationRepeat += RepeatInterval;
        }
    }
    
    private void StartNavigationRepeat(KeyCode key)
    {
        repeatingNavigationKey = key;
        RunNavigationAction(key);
        nextNavigationRepeat = Time.realtimeSinceStartup + RepeatInitialDelay;
    }

    private void RunNavigationAction(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow:
                MoveSelection(-1);
                break;
            case KeyCode.DownArrow:
                MoveSelection(1);
                break;
        }
    }

    private void MoveSelection(int offset)
    {
        if (filteredItems.Count == 0) return;
        selectedIndex = (selectedIndex + offset + filteredItems.Count) % filteredItems.Count;
        Render();
    }
    
    private void NavigateBack()
    {
        navigation.RemoveAt(navigation.Count - 1);
        ClearQuery();
        selectedIndex = 0;
        Render();
        StartCoroutine(ActivateQueryField());
    }

    #endregion

    private void HandleWordDeletion()
    {
        if (queryBeforeWordDelete == null) return;

        string beforeWordDelete = queryBeforeWordDelete;
        queryBeforeWordDelete = null;
        if (!IsOpen) return;

        int end = beforeWordDelete.Length;
        while (end > 0 && char.IsWhiteSpace(beforeWordDelete[end - 1])) end--;
        while (end > 0 && !char.IsWhiteSpace(beforeWordDelete[end - 1])) end--;

        string updatedQuery = beforeWordDelete[..end];
        query = updatedQuery;
        queryField.SetTextWithoutNotify(updatedQuery);
        placeholderText.ActiveSelf = string.IsNullOrEmpty(updatedQuery);
        selectedIndex = 0;
        Render();
    }

    private void Open()
    {
        if (panel == null) BuildPanel();
        TimeScale.VoteFreeze(freezeOwner, true);
        navigation.Clear();
        ClearQuery();
        selectedIndex = 0;
        repeatingNavigationKey = KeyCode.None;
        panel.ActiveSelf = true;
        SetGameInputBlocked(true);
        queryField.Activate();
        Render();
    }

    private void Close()
    {
        _closedFrame = Time.frameCount;
        TimeScale.VoteFreeze(freezeOwner, false);
        SetGameInputBlocked(false);
        panel.ActiveSelf = false;
        repeatingNavigationKey = KeyCode.None;
        queryField.Deactivate();
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void SetGameInputBlocked(bool shouldBlock)
    {
        HeroController hero = HeroController.SilentInstance;
        if (!hero) return;
        if (shouldBlock) hero.AddInputBlocker(inputBlocker);
        else hero.RemoveInputBlocker(inputBlocker);
    }

    private IEnumerable<PaletteEntry> CurrentItems()
        => (navigation.Count == 0 ? registry.RootItems : navigation[^1].GetChildren())
            .Select(item => new PaletteEntry(item, item.Detail));

    private IEnumerable<PaletteEntry> SearchItems(IEnumerable<CommandPaletteItem> items, string path = "")
    {
        foreach (CommandPaletteItem item in items)
        {
            if (item is CommandPaletteItem.SubmenuItem submenu)
            {
                string submenuPath = string.IsNullOrEmpty(path) ? submenu.Title : $"{path} {SubmenuIndicator} {submenu.Title}";
                yield return new PaletteEntry(submenu, path);
                if (!submenu.SearchChildren) continue;
                foreach (PaletteEntry entry in SearchItems(submenu.GetChildren(), submenuPath)) yield return entry;
                continue;
            }

            string detail = string.IsNullOrEmpty(path) ? item.Detail : string.IsNullOrEmpty(item.Detail) ? path : $"{path} {SubmenuIndicator} {item.Detail}";
            yield return new PaletteEntry(item, detail);
        }
    }

    private void ActivateSelected()
    {
        if (filteredItems.Count == 0) return;
        Activate(filteredItems[selectedIndex].Item);
    }

    private void Activate(CommandPaletteItem item)
    {
        switch (item)
        {
            case CommandPaletteItem.SubmenuItem submenu:
                navigation.Add(submenu);
                ClearQuery();
                selectedIndex = 0;
                Render();
                StartCoroutine(ActivateQueryField());
                break;
            case CommandPaletteItem.ToggleItem toggle:
                toggle.Toggle();
                Close();
                break;
            case CommandPaletteItem.ActionItem action:
                action.Execute();
                Close();
                break;
        }
    }

    private IEnumerator ActivateQueryField()
    {
        yield return null;
        if (IsOpen) queryField.Activate();
    }
    
    #region UI

    private void BuildPanel()
    {
        Vector2 panelSize = new(PanelWidth, PanelHeight);
        panel = new CanvasPanel("Panel")
        {
            LocalPosition = new Vector2((Screen.width - panelSize.x) / 2f, PanelTop),
            Size = panelSize,
            CollapseMode = CollapseMode.Deny,
        };
        UICommon.AddBackground(panel);

        CanvasButton queryButton = panel.Add(new CanvasButton("Query"));
        queryButton.LocalPosition = new Vector2(HorizontalPadding, VerticalPadding);
        queryButton.Size = new Vector2(panelSize.x - HorizontalPadding * 2, RowHeight);
        queryButton.SetImage(UICommon.panelDarkBG);
        queryButton.RemoveHoverBorder();
        queryField = queryButton.SetTextField();
        queryField.Persistent = true;
        queryField.Alignment = TextAnchor.MiddleLeft;
        queryField.OnValueChanged += OnQueryChanged;

        placeholderText = panel.Add(new CanvasText("Placeholder"));
        placeholderText.LocalPosition = new Vector2(HorizontalPadding + QueryTextPadding, VerticalPadding);
        placeholderText.Size = new Vector2(panelSize.x - (HorizontalPadding + QueryTextPadding) * 2, RowHeight);
        placeholderText.Alignment = TextAnchor.MiddleLeft;
        placeholderText.Text = Localization.Get("COMMANDPALETTE_SEARCH");

        BuildRows(panel, panelSize);
        panel.Build();

        panel.ActiveSelf = false;
    }

    private void BuildRows(CanvasPanel parent, Vector2 panelSize)
    {
        for (int i = 0; i < MaxVisibleItems; i++)
        {
            CanvasPanel rowPanel = parent.Add(new CanvasPanel($"Row {i}"));
            rowPanel.LocalPosition = new Vector2(HorizontalPadding, RowsTop + i * (RowHeight + RowPadding));
            rowPanel.Size = new Vector2(panelSize.x - HorizontalPadding * 2, RowHeight);
            rowPanel.CollapseMode = CollapseMode.Deny;

            CanvasButton button = rowPanel.Add(new CanvasButton("Button"));
            button.Size = rowPanel.Size;
            button.Text.Alignment = TextAnchor.MiddleLeft;
            button.RemoveBorder();

            CanvasText detail = rowPanel.Add(new CanvasText("Detail"));
            detail.LocalPosition = new Vector2(rowPanel.Size.x - DetailWidth - DetailPadding, 0);
            detail.Size = new Vector2(DetailWidth, rowPanel.Size.y);
            detail.Alignment = TextAnchor.MiddleRight;
            detail.Color = UICommon.iconColor;

            CanvasBorder selection = rowPanel.Add(new CanvasBorder("Selection"));
            selection.Size = rowPanel.Size;
            selection.Thickness = SelectionThickness;
            selection.Color = UICommon.accentColor;
            selection.Sides = BorderSides.LEFT;
            selection.ActiveSelf = false;

            PaletteRow row = new(rowPanel, button, detail, selection);
            button.OnClicked += () =>
            {
                selectedIndex = row.ItemIndex;
                Activate(row.Item);
            };
            rows.Add(row);
        }
    }

    private void Render()
    {
        filteredItems = (string.IsNullOrEmpty(query) ? CurrentItems() : SearchItems(registry.RootItems))
            .Where(entry => Matches(entry, query))
            .ToList();
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, filteredItems.Count - 1));
        int firstVisibleIndex = Mathf.Clamp(selectedIndex - MaxVisibleItems + 1, 0, Mathf.Max(0, filteredItems.Count - MaxVisibleItems));

        for (int i = 0; i < rows.Count; i++)
        {
            PaletteRow row = rows[i];
            int itemIndex = firstVisibleIndex + i;
            row.Panel.ActiveSelf = itemIndex < filteredItems.Count;
            if (!row.Panel.ActiveSelf) continue;

            PaletteEntry entry = filteredItems[itemIndex];
            row.Item = entry.Item;
            row.ItemIndex = itemIndex;
            row.Button.Toggled = itemIndex == selectedIndex;
            row.Selection.ActiveSelf = itemIndex == selectedIndex;
            row.Button.Text.Text = entry.Item.Title;
            row.Detail.Text = entry.Item switch
            {
                CommandPaletteItem.ToggleItem toggle => Localization.Get(toggle.IsEnabled() ? "COMMANDPALETTE_ON" : "COMMANDPALETTE_OFF"),
                CommandPaletteItem.SubmenuItem => string.IsNullOrEmpty(entry.Detail) ? SubmenuIndicator : entry.Detail,
                _ => entry.Detail ?? ""
            };
        }
    }
    
    #endregion

    private static bool Matches(PaletteEntry entry, string query)
    {
        string text = NormalizeSearch($"{entry.Item.Title} {entry.Detail}");
        return query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .All(term => text.Contains(NormalizeSearch(term), StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeSearch(string text) => text.Replace(" ", "").Replace("_", "");

    private void OnQueryChanged(string value)
    {
        query = value;
        selectedIndex = 0;
        placeholderText.ActiveSelf = string.IsNullOrEmpty(value);
        Render();
    }

    private void ClearQuery()
    {
        query = "";
        queryField.SetTextWithoutNotify(query);
        placeholderText.ActiveSelf = true;
    }

    private sealed class PaletteRow(CanvasPanel panel, CanvasButton button, CanvasText detail, CanvasBorder selection)
    {
        public CanvasPanel Panel { get; } = panel;
        public CanvasButton Button { get; } = button;
        public CanvasText Detail { get; } = detail;
        public CanvasBorder Selection { get; } = selection;
        public CommandPaletteItem Item { get; set; }
        public int ItemIndex { get; set; }
    }

    private sealed class PaletteEntry(CommandPaletteItem item, string detail)
    {
        public CommandPaletteItem Item { get; } = item;
        public string Detail { get; } = detail;
    }

}
