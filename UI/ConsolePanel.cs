using DebugMod.UI.Canvas;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI;

public class ConsolePanel : CanvasPanel
{
    public const int MAX_LINES = 16;

    public static ConsolePanel Instance { get; private set; }
    private static readonly List<string> history = [];

    private readonly List<CanvasText> lines = [];

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

        float lineHeight = (Size.y - UICommon.Margin * 2) / MAX_LINES;
        for (int i = 0; i < MAX_LINES; i++)
        {
            CanvasText line = Add(new CanvasText(i.ToString()));
            line.LocalPosition = new Vector2(UICommon.Margin, UICommon.Margin + lineHeight * i);
            line.Size = new Vector2(Size.x - UICommon.Margin * 2, lineHeight);
            lines.Add(line);
        }
    }

    private void UpdateText()
    {
        int line = 0;
        for (int i = Math.Max(history.Count - MAX_LINES, 0); i < history.Count; i++)
        {
            lines[line].Text = history[i];
            line++;
        }

        while (line < MAX_LINES)
        {
            lines[line].Text = "";
            line++;
        }
    }

    public void Reset()
    {
        history.Clear();
        UpdateText();
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
        UpdateText();
    }

    private int WrapIndex(string message)
    {
        CanvasText text = lines[0];
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
