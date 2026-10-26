using DebugMod.Helpers;
using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DebugMod.UI.CommandPalette;

[HarmonyPatch]
public sealed class CommandPaletteController : MonoBehaviour
{
    #region UI Properties

    private const int MaxVisibleItems = 8;
    private const int MaxRecentItems = 5;
    private const int ContextRowCount = 1;
    private const int SelectionThickness = 2;
    private const float IconHeight = 0.7f;
    private const string PathSeparator = "›";
    private static int PanelWidth => UICommon.ScaleWidth(560);
    private static int RowHeight => UICommon.ScaleHeight(30);
    private static int DetailWidth => UICommon.ScaleWidth(140);
    private static int DetailPadding => UICommon.ScaleWidth(10);

    #endregion

    [HarmonyPatch(typeof(InputHandler), "Update")]
    [HarmonyPrefix]
    private static bool InputHandler_Update() => !IsOpen;

    private static CommandPaletteController _instance;
    private static int? _lastToggleFrame;

    private CanvasPanel panel;
    private CanvasButton backButton;
    private CanvasTextField queryField;
    private CanvasText placeholderText;
    private readonly List<PaletteRow> rows = [];
    private readonly List<CommandPaletteItem.SubmenuItem> navigation = [];
    private readonly List<PaletteEntry> history = [];
    private List<PaletteEntry> filteredItems = [];
    private string query = "";
    private int selectedIndex;
    private int firstVisibleIndex;
    private bool mouseMovedSinceOpen;
    private string queryBeforeWordDelete;
    private readonly object freezeOwner = new();
    private readonly object inputBlocker = new();

    private int openedFrame;

    public static bool IsOpen => _instance != null && _instance.panel != null && _instance.panel.ActiveSelf;

    private Binding removeWordBinding = new Binding(Application.platform == RuntimePlatform.OSXPlayer ? Modifier.Alt : Modifier.Ctrl, KeyCode.Backspace);

    #region Lifecycle Methods

    public static void Build()
    {
        _instance = GUIController.Instance.gameObject.AddComponent<CommandPaletteController>();
    }

    public static void Unload()
    {
        if (_instance == null) return;
        if (IsOpen) _instance.Close();
        _instance.panel?.Destroy();
        _instance = null;
    }

    public static void Toggle()
    {
        if (_instance == null) return;
        if (Time.frameCount == _lastToggleFrame) return;
        _lastToggleFrame = Time.frameCount;

        if (IsOpen) _instance.Close();
        else _instance.Open();
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

    private void OnGUI()
    {
        try
        {
            HandleNavigation();
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Error during command palette OnGUI: {e}");
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
        if (!IsOpen) return;

        if (Input.mousePositionDelta != Vector3.zero) mouseMovedSinceOpen = true;

        if (Input.GetMouseButtonDown(0) && !panel.IsMouseOver() && Time.frameCount > openedFrame)
        {
            Close();
            return;
        }

        if (DebugMod.settings.binds.TryGetValue("MODUI_TOGGLECOMMANDPALETTE", out Binding toggle) && toggle.IsDown())
        {
            Toggle();
            return;
        }

        if (removeWordBinding.IsDown())
        {
            queryBeforeWordDelete = queryField.Text;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Mouse.current?.backButton.wasPressedThisFrame == true)
        {
            if (navigation.Count == 0) Close();
            else NavigateBack();
            return;
        }

        if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0f) && panel.IsMouseOver())
        {
            MoveSelection(Input.mouseScrollDelta.y > 0 ? -1 : 1, wrapAround: false);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) ActivateSelected();
    }

    #region Navigation

    private void HandleNavigation()
    {
        if (!IsOpen || Event.current.type != EventType.KeyDown) return;

        switch (Event.current.keyCode)
        {
            case KeyCode.UpArrow:
                MoveSelection(-1);
                break;
            case KeyCode.DownArrow:
                MoveSelection(1);
                break;
        }
    }

    private void MoveSelection(int offset, bool wrapAround = true)
    {
        if (filteredItems.Count == 0) return;
        if (wrapAround) selectedIndex = (selectedIndex + offset + filteredItems.Count) % filteredItems.Count;
        else selectedIndex = Mathf.Clamp(selectedIndex + offset, 0, filteredItems.Count - 1);
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
        PositionPanel();
        TimeScale.VoteFreeze(freezeOwner, true);
        navigation.Clear();
        ClearQuery();
        selectedIndex = 0;
        mouseMovedSinceOpen = false;
        openedFrame = Time.frameCount;
        panel.ActiveSelf = true;
        SetGameInputBlocked(true);
        queryField.Activate();
        Render();
    }

    private void Close()
    {
        TimeScale.VoteFreeze(freezeOwner, false);
        SetGameInputBlocked(false);
        panel.ActiveSelf = false;
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
        => (navigation.Count == 0 ? DebugMod.CommandPaletteRegistry.RootItems : navigation[^1].GetChildren())
            .Select(item => new PaletteEntry(item, null));

    private IEnumerable<PaletteEntry> SearchItems(IEnumerable<CommandPaletteItem> items, string path = "")
    {
        foreach (CommandPaletteItem item in items)
        {
            if (item is CommandPaletteItem.SubmenuItem submenu)
            {
                string title = submenu.Title();
                string submenuPath = string.IsNullOrEmpty(path) ? title : $"{path} {PathSeparator} {title}";
                yield return new PaletteEntry(submenu, path);
                if (!submenu.SearchChildren) continue;
                foreach (PaletteEntry entry in SearchItems(submenu.GetChildren(), submenuPath)) yield return entry;
                continue;
            }

            string detail = string.IsNullOrEmpty(path) ? null : path;
            yield return new PaletteEntry(item, detail);
        }
    }

    private void ActivateSelected()
    {
        if (filteredItems.Count == 0) return;
        Activate(filteredItems[selectedIndex]);
    }

    private void Activate(PaletteEntry entry)
    {
        switch (entry.Item)
        {
            case CommandPaletteItem.SubmenuItem submenu:
                navigation.Add(submenu);
                ClearQuery();
                selectedIndex = 0;
                Render();
                StartCoroutine(ActivateQueryField());
                break;
            case CommandPaletteItem.ToggleItem toggle:
                Remember(entry);
                toggle.Toggle();
                Close();
                break;
            case CommandPaletteItem.ActionItem action:
                Remember(entry);
                action.Execute();
                Close();
                break;
        }
    }

    private void Remember(PaletteEntry entry)
    {
        history.RemoveAll(historyEntry => historyEntry.Item == entry.Item);
        history.Insert(0, new PaletteEntry(entry.Item, entry.Detail, isRecent: true));
        if (history.Count > MaxRecentItems) history.RemoveAt(MaxRecentItems);
    }

    private IEnumerator ActivateQueryField()
    {
        yield return null;
        if (IsOpen) queryField.Activate();
    }

    #region UI

    private void BuildPanel()
    {
        panel = new CanvasPanel(nameof(CommandPaletteController))
        {
            Size = new Vector2(PanelWidth, 0),
            CollapseMode = CollapseMode.Deny,
        };
        UICommon.AddBackground(panel);

        float contentMargin = panel.ContentMargin(UICommon.Margin);
        using (PanelBuilder builder = new(panel))
        {
            builder.DynamicLength = true;
            builder.OuterPadding = contentMargin;
            builder.InnerPadding = UICommon.Margin;

            CanvasButton queryButton = builder.AppendFixed(new CanvasButton("Query"), RowHeight);
            queryButton.SetImage(UICommon.panelDarkBG);
            queryButton.RemoveHoverBorder();
            queryField = queryButton.SetTextField();
            queryField.Persistent = true;
            queryField.Alignment = TextAnchor.MiddleLeft;
            queryField.OnValueChanged += OnQueryChanged;

            placeholderText = panel.Add(new CanvasText("Placeholder"));
            placeholderText.LocalPosition = new Vector2(contentMargin + CanvasButton.TextMargin, contentMargin);
            placeholderText.Size = new Vector2(PanelWidth - (contentMargin + CanvasButton.TextMargin) * 2, RowHeight);
            placeholderText.Alignment = TextAnchor.MiddleLeft;
            placeholderText.Text = Localization.Get("COMMANDPALETTE_SEARCH");

            float iconSize = RowHeight * IconHeight;
            float iconMargin = (RowHeight - iconSize) / 2;
            backButton = panel.Add(new CanvasButton("Back"));
            backButton.LocalPosition = new Vector2(PanelWidth - contentMargin - iconSize - iconMargin, contentMargin + iconMargin);
            backButton.Size = new Vector2(iconSize, iconSize);
            backButton.ImageOnly(UICommon.images["IconLeftMin"]);
            backButton.OnUpdate += () => backButton.Color = mouseMovedSinceOpen && backButton.IsMouseOver() ? UICommon.accentColor : UICommon.iconColor;
            backButton.OnClicked += NavigateBack;

            BuildRows(builder);

            panel.ActiveSelf = false;
        }

        panel.Build();

        queryField.Size -= new Vector2(RowHeight, 0); // prevent overlapping back button
    }

    private void PositionPanel()
    {
        float top = (Screen.height - panel.Size.y) / 2f;
        if (SaveStatesPanel.Instance != null && SaveStatesPanel.ShouldBeVisible && SaveStatesPanel.ShouldBeExpanded)
        {
            top = SaveStatesPanel.Instance.Position.y + SaveStatesPanel.Instance.Size.y + UICommon.ScreenMargin * 2;
        }

        panel.LocalPosition = new((Screen.width - panel.Size.x) / 2f, top);
    }

    private void BuildRows(PanelBuilder builder)
    {
        for (int i = 0; i < MaxVisibleItems; i++)
        {
            CanvasPanel rowPanel = builder.AppendFixed(new CanvasPanel($"Row {i}") { CollapseMode = CollapseMode.Deny }, RowHeight);

            CanvasButton button = rowPanel.Add(new CanvasButton("Button"));
            button.Size = rowPanel.Size;
            button.Text.Alignment = TextAnchor.MiddleLeft;
            button.RemoveBorder();
            button.ShowHoverBorder = () => mouseMovedSinceOpen;

            CanvasText detail = rowPanel.Add(new CanvasText("Detail"));
            detail.LocalPosition = new Vector2(rowPanel.Size.x - DetailWidth - DetailPadding, 0);
            detail.Size = new Vector2(DetailWidth, rowPanel.Size.y);
            detail.Alignment = TextAnchor.MiddleRight;
            detail.Color = UICommon.iconColor;

            float iconSize = rowPanel.Size.y * IconHeight;
            float iconMargin = (rowPanel.Size.y - iconSize) / 2;
            CanvasImage submenuIcon = rowPanel.Add(new CanvasImage("SubmenuIcon"));
            submenuIcon.LocalPosition = new Vector2(rowPanel.Size.x - iconSize - iconMargin, iconMargin);
            submenuIcon.Size = new Vector2(iconSize, iconSize);
            submenuIcon.SetImage(UICommon.images["IconRightMin"]);

            CanvasBorder selection = rowPanel.Add(new CanvasBorder("Selection"));
            selection.Size = rowPanel.Size;
            selection.Thickness = SelectionThickness;
            selection.Color = UICommon.accentColor;
            selection.Sides = BorderSides.LEFT;
            selection.ActiveSelf = false;

            PaletteRow row = new(rowPanel, button, detail, submenuIcon, selection);
            button.OnClicked += () =>
            {
                selectedIndex = row.ItemIndex;
                Activate(filteredItems[row.ItemIndex]);
            };
            rows.Add(row);
        }
    }

    private void Render()
    {
        backButton.ActiveSelf = navigation.Count > 0;

        IEnumerable<PaletteEntry> items = (string.IsNullOrEmpty(query) ? CurrentItems() : SearchItems(DebugMod.CommandPaletteRegistry.RootItems))
            .Concat(navigation.Count == 0 ? history.AsEnumerable().Reverse() : []);
        filteredItems = items
            .Where(entry => Matches(entry, query))
            .ToList();
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, filteredItems.Count - 1));
        if (selectedIndex < firstVisibleIndex + ContextRowCount)
            firstVisibleIndex = selectedIndex - ContextRowCount;
        if (selectedIndex > firstVisibleIndex + MaxVisibleItems - 1 - ContextRowCount)
            firstVisibleIndex = selectedIndex - MaxVisibleItems + 1 + ContextRowCount;
        firstVisibleIndex = Mathf.Clamp(firstVisibleIndex, 0, Mathf.Max(0, filteredItems.Count - MaxVisibleItems));

        for (int i = 0; i < rows.Count; i++)
        {
            PaletteRow row = rows[i];
            int itemIndex = firstVisibleIndex + i;
            row.Panel.ActiveSelf = itemIndex < filteredItems.Count;
            if (!row.Panel.ActiveSelf) continue;

            PaletteEntry entry = filteredItems[itemIndex];
            row.ItemIndex = itemIndex;
            row.Button.Toggled = itemIndex == selectedIndex;
            row.Selection.ActiveSelf = itemIndex == selectedIndex;
            row.Button.Text.Text = entry.Item.Title();
            row.Detail.Text = entry.IsRecent ? Localization.Get("COMMANDPALETTE_RECENTLY_USED") : entry.Item switch
            {
                CommandPaletteItem.ToggleItem toggle => Localization.Get(toggle.IsEnabled() ? "COMMANDPALETTE_ON" : "COMMANDPALETTE_OFF"),
                _ => entry.Detail ?? ""
            };
            row.SubmenuIcon.ActiveSelf = entry.Item is CommandPaletteItem.SubmenuItem && string.IsNullOrEmpty(entry.Detail);
        }
    }

    #endregion

    private static bool Matches(PaletteEntry entry, string query)
    {
        string[] fields = [entry.Item.Title(), entry.Detail ?? ""];
        return query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .All(term => fields.Any(f => NormalizeSearch(f).Contains(NormalizeSearch(term), StringComparison.OrdinalIgnoreCase)));
    }

    private static string NormalizeSearch(string text) => text.Replace(" ", "").Replace("_", "");

    private void OnQueryChanged(string value)
    {
        query = value ?? "";
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

    private sealed class PaletteRow(CanvasPanel panel, CanvasButton button, CanvasText detail, CanvasImage submenuIcon, CanvasBorder selection)
    {
        public CanvasPanel Panel { get; } = panel;
        public CanvasButton Button { get; } = button;
        public CanvasText Detail { get; } = detail;
        public CanvasImage SubmenuIcon { get; } = submenuIcon;
        public CanvasBorder Selection { get; } = selection;
        public int ItemIndex { get; set; }
    }

    private sealed class PaletteEntry(CommandPaletteItem item, string detail, bool isRecent = false)
    {
        public CommandPaletteItem Item { get; } = item;
        public string Detail { get; } = detail;
        public bool IsRecent { get; } = isRecent;
    }

}
