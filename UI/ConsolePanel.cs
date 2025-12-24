using DebugMod.UI.Canvas;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI;

public class ConsolePanel : CanvasPanel
{
    public const int MAX_LINES = 16;

    public static ConsolePanel Instance { get; private set; }
    private static readonly List<string> history = [];

    private readonly CanvasText text;

    public static void BuildPanel()
    {
        Instance = new ConsolePanel();
        Instance.Build();
    }

    public ConsolePanel() : base(nameof(ConsolePanel))
    {
        LocalPosition = new Vector2(UICommon.ScreenMargin, Screen.height - UICommon.ScreenMargin - UICommon.ConsoleHeight);
        Size = new Vector2(UICommon.LeftSideWidth, UICommon.ConsoleHeight);

        UICommon.AddBackground(this);

        text = Add(new CanvasText("Text"));
        text.LocalPosition = new Vector2(UICommon.Margin, UICommon.Margin);
        text.Size = new Vector2(Size.x - UICommon.Margin * 2, Size.y - UICommon.Margin * 2);
    }

    public override void Update()
    {
        string consoleString = "";
        int lineCount = 0;

        for (int i = history.Count - 1; i >= 0; i--)
        {
            if (lineCount >= MAX_LINES) break;
            consoleString = history[i] + "\n" + consoleString;
            lineCount++;
        }

        text.Text = consoleString;

        base.Update();
    }

    public void Reset()
    {
        history.Clear();
    }

    public void AddLine(string chatLine)
    {
        while (history.Count > 1000)
        {
            history.RemoveAt(0);
        }

        int wrap = WrapIndex(chatLine);

        while (wrap != -1)
        {
            int index = chatLine.LastIndexOf(' ', wrap, wrap);

            if (index != -1)
            {
                history.Add(chatLine.Substring(0, index));
                chatLine = chatLine.Substring(index + 1);
                wrap = WrapIndex(chatLine);
            }
            else
            {
                break;
            }
        }

        history.Add(chatLine);
    }

    private int WrapIndex(string message)
    {
        int totalLength = 0;

        char[] arr = message.ToCharArray();

        for (int i = 0; i < arr.Length; i++)
        {
            char c = arr[i];
            text.Font.GetCharacterInfo(c, out CharacterInfo characterInfo, text.FontSize);
            totalLength += characterInfo.advance;

            if (totalLength >= text.Size.x) return i;
        }

        return -1;
    }
}
