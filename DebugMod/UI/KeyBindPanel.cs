using System.Collections.Generic;
using System.Linq;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class KeyBindPanel : CanvasPanel
{
    private class CategoryInfo
    {
        public string Name;
        public List<string> Functions = new();
        
        public CategoryInfo(string Name)
        {
            this.Name = Name;
        }

        public void Add(string method) => Functions.Add(method);

        public int NumPages => (Functions.Count + ItemsPerPage - 1) / ItemsPerPage; // Ceiling division

        public IEnumerable<string> ItemsOnPage(int index)
        {
            for (int i = ItemsPerPage * index; i < ItemsPerPage * (index + 1) && i < Functions.Count; i++)
            {
                yield return Functions[i];
            }
        }


        public static List<string> Categories = new()
        {
            "Gameplay Altering",
            "Savestates",
            "Misc",
            "Visual",
            "Mod UI",
            "Enemy Panel",
            "Cheats",
            "Tools",
            "Skills",
            "Spells",
            "Bosses",
            "Items",
            "Masks & Spools",
            "Consumables",
        };
        public static Dictionary<string, CategoryInfo> CategoryInfos = new();
        public static int TotalPages => CategoryInfos.Select(x => x.Value.NumPages).Sum();

        public static List<(string categoryName, int indexInCategory)> pageData = new();
        public static void GeneratePageData()
        {
            pageData.Clear();
            foreach (string categoryName in Categories)
            {
                if (CategoryInfos.TryGetValue(categoryName, out CategoryInfo info))
                {
                    for (int i = 0; i < info.NumPages; i++)
                    {
                        pageData.Add((categoryName, i));
                    }
                }
            }
        }
        
        public static int currentPage = 0;

        public static void AddFunction(string category, string name)
        {
            if (!CategoryInfos.TryGetValue(category, out CategoryInfo info))
            {
                info = new CategoryInfo(category);

                if (!Categories.Contains(category)) Categories.Add(category);
                CategoryInfos.Add(category, info);
            }

            info.Add(name);
        }

        public static List<string> FunctionsOnCurrentPage()
        {
            (string categoryName, int indexInCategory) = pageData[currentPage];

            return CategoryInfos[categoryName].ItemsOnPage(indexInCategory).ToList();
        }
        public static string CurrentCategory => pageData[currentPage].categoryName;
    }

    public const int ItemsPerPage = 11;
    
    public static KeyCode keyWarning = KeyCode.None;

    public static KeyBindPanel Instance { get; private set; }

    public static void BuildPanel()
    {
        Instance = new KeyBindPanel();
        Instance.Build();
    }

    // TODO: Refactor to allow rotating images
    public KeyBindPanel() : base(nameof(KeyBindPanel), null, new Vector2(1123, 456), Vector2.zero, UICommon.images["HelpBG"])
    {
        AddText("Label", "Binds", new Vector2(130f, -25f), Vector2.zero, UICommon.trajanBold,
            30);

        AddText("Category", "", new Vector2(25f, 25f), Vector2.zero, UICommon.trajanNormal, 20);
        AddText("Help", "", new Vector2(25f, 50f), Vector2.zero, UICommon.arial, 15);
        AddButton("Page", UICommon.images["ButtonRect"], new Vector2(125, 250), Vector2.zero,
            () => NextClicked(false),
            new Rect(0, 0, UICommon.images["ButtonRect"].width,
                UICommon.images["ButtonRect"].height), UICommon.trajanBold, "# / #");


        AddButton(
            "NextPage",
            UICommon.images["ScrollBarArrowRight"],
            new Vector2(223, 254),
            Vector2.zero,
            () => NextClicked(false),
            new Rect(
                0,
                0,
                UICommon.images["ScrollBarArrowRight"].width,
                UICommon.images["ScrollBarArrowRight"].height)
        );
        AddButton(
            "PrevPage",
            UICommon.images["ScrollBarArrowLeft"],
            new Vector2(95, 254),
            Vector2.zero,
            () => NextClicked(true),
            new Rect(
                0,
                0,
                UICommon.images["ScrollBarArrowLeft"].width,
                UICommon.images["ScrollBarArrowLeft"].height)
        );

        for (int i = 0; i < ItemsPerPage; i++)
        {
            int index = i; // so that the for loop doesn't mutate the captured variable

            AddButton(i.ToString(), UICommon.images["Scrollbar_point"],
                new Vector2(290f, 45f + 17.5f * i), Vector2.zero, () => ChangeBind(index),
                new Rect(0, 0, UICommon.images["Scrollbar_point"].width,
                    UICommon.images["Scrollbar_point"].height));
            AddButton($"run{i}", UICommon.images["ButtonRun"],
                new Vector2(308f, 51f + 17.5f * i), new Vector2(12f, 12f), () => RunBind(index),
                new Rect(0, 0, UICommon.images["ButtonRun"].width,
                    UICommon.images["ButtonRun"].height));
        }
    
        //Build pages based on categories
        foreach (var action in DebugMod.bindActions.Values)
        {
            CategoryInfo.AddFunction(action.Category, action.Name);
        }
        CategoryInfo.GeneratePageData();

        GetText("Category").Text = CategoryInfo.CurrentCategory;
        GetButton("Page").Text.Text = CategoryInfo.currentPage + 1 + " / " + CategoryInfo.TotalPages;
        UpdateHelpText();
    }
    
    private void RunBind(int index) {
        string bindName = CategoryInfo.FunctionsOnCurrentPage()[index];

        if (DebugMod.bindActions.TryGetValue(bindName, out var action))
        {
            action.Action.Invoke();
        }
        else
        {
            DebugMod.LogError("Error running bind: not found");
        }
    }

    public void UpdateHelpText()
    {
        if (CategoryInfo.currentPage < 0 || CategoryInfo.currentPage >= CategoryInfo.TotalPages) return;

        string cat = CategoryInfo.CurrentCategory;
        List<string> helpPage = CategoryInfo.FunctionsOnCurrentPage();

        string updatedText = "";

        foreach (string bindStr in helpPage)
        {
            updatedText += bindStr + " - ";

            if (DebugMod.settings.binds.ContainsKey(bindStr))
            {
                KeyCode code = DebugMod.settings.binds[bindStr];

                if (code != KeyCode.None)
                {
                    updatedText += DebugMod.settings.binds[bindStr].ToString();
                }
                else
                {
                    updatedText += "WAITING";
                }
            }
            else
            {
                updatedText += "UNBOUND";
            }

            updatedText += "\n";
        }

        GetText("Help").Text = updatedText;
    }

    private void NextClicked(bool previous)
    {
        if (previous)
        {
            CategoryInfo.currentPage--;
            if (CategoryInfo.currentPage < 0) CategoryInfo.currentPage = CategoryInfo.TotalPages - 1;
        }
        else
        {
            CategoryInfo.currentPage++;
            if (CategoryInfo.currentPage >= CategoryInfo.TotalPages) CategoryInfo.currentPage = 0;
        }

        GetText("Category").Text = CategoryInfo.CurrentCategory;
        GetButton("Page").Text.Text = CategoryInfo.currentPage + 1 + " / " + CategoryInfo.TotalPages;
        UpdateHelpText();
    }

    private void ChangeBind(int index)
    {
        if (index < 0 || index >= CategoryInfo.FunctionsOnCurrentPage().Count)
        {
            DebugMod.LogWarn("Invalid bind change button clicked. Should not be possible");
            return;
        }

        string bindName = CategoryInfo.FunctionsOnCurrentPage()[index];

        if (DebugMod.settings.binds.ContainsKey(bindName))
        {
            DebugMod.settings.binds[bindName] = KeyCode.None;
        }
        else
        {
            DebugMod.settings.binds.Add(bindName, KeyCode.None);
        }

        UpdateHelpText();
    }

    public override void Update()
    {
        ActiveSelf = DebugMod.settings.HelpPanelVisible;

        if (ActiveInHierarchy && CategoryInfo.currentPage >= 0 && CategoryInfo.currentPage < CategoryInfo.TotalPages)
        {
            for (int i = 0; i < ItemsPerPage; i++)
            {
                GetButton(i.ToString()).ActiveSelf = CategoryInfo.FunctionsOnCurrentPage().Count > i;
                GetButton($"run{i}").ActiveSelf = CategoryInfo.FunctionsOnCurrentPage().Count > i;
            }
        }
    }
}
