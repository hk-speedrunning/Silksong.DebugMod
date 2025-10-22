using System.Collections.Generic;
using System.Linq;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public static class KeyBindPanel
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

    private static CanvasPanel panel;
    
    public static KeyCode keyWarning = KeyCode.None;

    // TODO: Refactor to allow rotating images
    public static void BuildMenu(GameObject canvas)
    {
        panel = new CanvasPanel(
            nameof(KeyBindPanel),
            null,
            new Vector2(1123, 456),
            Vector2.zero,
            GUIController.Instance.images["HelpBG"],
            new Rect(0, 0, GUIController.Instance.images["HelpBG"].width,
                GUIController.Instance.images["HelpBG"].height));
        panel.AddText("Label", "Binds", new Vector2(130f, -25f), Vector2.zero, GUIController.Instance.trajanBold,
            30);

        panel.AddText("Category", "", new Vector2(25f, 25f), Vector2.zero, GUIController.Instance.trajanNormal, 20);
        panel.AddText("Help", "", new Vector2(25f, 50f), Vector2.zero, GUIController.Instance.arial, 15);
        panel.AddButton("Page", GUIController.Instance.images["ButtonRect"], new Vector2(125, 250), Vector2.zero,
            () => NextClicked(false),
            new Rect(0, 0, GUIController.Instance.images["ButtonRect"].width,
                GUIController.Instance.images["ButtonRect"].height), GUIController.Instance.trajanBold, "# / #");


        panel.AddButton(
            "NextPage",
            GUIController.Instance.images["ScrollBarArrowRight"],
            new Vector2(223, 254),
            Vector2.zero,
            () => NextClicked(false),
            new Rect(
                0,
                0,
                GUIController.Instance.images["ScrollBarArrowRight"].width,
                GUIController.Instance.images["ScrollBarArrowRight"].height)
        );
        panel.AddButton(
            "PrevPage",
            GUIController.Instance.images["ScrollBarArrowLeft"],
            new Vector2(95, 254),
            Vector2.zero,
            () => NextClicked(true),
            new Rect(
                0,
                0,
                GUIController.Instance.images["ScrollBarArrowLeft"].width,
                GUIController.Instance.images["ScrollBarArrowLeft"].height)
        );

        for (int i = 0; i < ItemsPerPage; i++)
        {
            int index = i; // so that the for loop doesn't mutate the captured variable

            panel.AddButton(i.ToString(), GUIController.Instance.images["Scrollbar_point"],
                new Vector2(290f, 45f + 17.5f * i), Vector2.zero, () => ChangeBind(index),
                new Rect(0, 0, GUIController.Instance.images["Scrollbar_point"].width,
                    GUIController.Instance.images["Scrollbar_point"].height));
            panel.AddButton($"run{i}", GUIController.Instance.images["ButtonRun"],
                new Vector2(308f, 51f + 17.5f * i), new Vector2(12f, 12f), () => RunBind(index),
                new Rect(0, 0, GUIController.Instance.images["ButtonRun"].width,
                    GUIController.Instance.images["ButtonRun"].height));
        }
    
        //Build pages based on categories
        foreach (var action in DebugMod.bindActions.Values)
        {
            CategoryInfo.AddFunction(action.Category, action.Name);
        }
        CategoryInfo.GeneratePageData();

        panel.GetText("Category").UpdateText(CategoryInfo.CurrentCategory);
        panel.GetButton("Page").UpdateText(CategoryInfo.currentPage + 1 + " / " + CategoryInfo.TotalPages);
        UpdateHelpText();
    }
    
    private static void RunBind(int index) {
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

    public static void UpdateHelpText()
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

        panel.GetText("Help").UpdateText(updatedText);
    }

    private static void NextClicked(bool previous)
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

        panel.GetText("Category").UpdateText(CategoryInfo.CurrentCategory);
        panel.GetButton("Page").UpdateText(CategoryInfo.currentPage + 1 + " / " + CategoryInfo.TotalPages);
        UpdateHelpText();
    }

    private static void ChangeBind(int index)
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

    public static void Update()
    {
        if (panel == null)
        {
            return;
        }

        if (GUIController.ForceHideUI())
        {
            panel.ActiveSelf = false;
            return;
        }

        panel.ActiveSelf = DebugMod.settings.HelpPanelVisible;

        if (panel.ActiveInHierarchy && CategoryInfo.currentPage >= 0 && CategoryInfo.currentPage < CategoryInfo.TotalPages)
        {
            for (int i = 0; i < ItemsPerPage; i++)
            {
                panel.GetButton(i.ToString()).ActiveSelf = CategoryInfo.FunctionsOnCurrentPage().Count > i;
                panel.GetButton($"run{i}").ActiveSelf = CategoryInfo.FunctionsOnCurrentPage().Count > i;
            }
        }
    }
}
