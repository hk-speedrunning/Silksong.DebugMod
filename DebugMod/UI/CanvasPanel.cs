﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DebugMod.UI
{
    public class CanvasPanel
    {
        private readonly GameObject canvas;
        private readonly Dictionary<string, CanvasButton> buttons = new();
        private readonly Dictionary<string, CanvasPanel> panels = new();
        private readonly Dictionary<string, CanvasImage> images = new();
        private readonly Dictionary<string, CanvasText> texts = new();
        private Vector2 position;
        private Vector2 size;

        public enum MenuItems
        {
            TextButton = 0,
            ImageButton
        };

        private MenuItems LastItem = MenuItems.TextButton;
        public int NumButtons = 0;
        private float xpos = Xposes[0];
        private float ypos = 30f;
        private static float[] Xposes =
        {
            15f, 52f
        };
        public bool active;

        public CanvasPanel(GameObject parent, Texture2D tex, Vector2 pos, Vector2 sz, Rect bgSubSection)
        {
            if (parent == null) return;

            position = pos;
            size = sz;
            canvas = parent;

            if (tex != null)
            {
                AddImage("Background", tex, Vector2.zero, sz, bgSubSection);
            }

            active = true;
        }

        public void AddButton(string name, Texture2D tex, Vector2 pos, Vector2 sz, UnityAction<string> func, Rect bgSubSection, Font font = null, string text = null, int fontSize = 13)
        {
            CanvasButton button = new CanvasButton(canvas, name, tex, position + pos, size + sz, bgSubSection, font, text, fontSize);
            button.AddClickEvent(func);

            buttons.Add(name, button);
        }

        public void AddButton(string name, Texture2D tex, Vector2 pos, Vector2 sz, Action func, Rect bgSubSection, Font font = null, string text = null, int fontSize = 13)
        {
            AddButton(name, tex, pos, sz, _ => func(), bgSubSection, font, text, fontSize);
        }

        public CanvasPanel AddPanel(string name, Texture2D tex, Vector2 pos, Vector2 sz, Rect bgSubSection)
        {
            CanvasPanel panel = new CanvasPanel(canvas, tex, position + pos, sz, bgSubSection);

            panels.Add(name, panel);
            return panel;
        }

        public void AddImage(string name, Texture2D tex, Vector2 pos, Vector2 size, Rect subSprite)
        {
            CanvasImage image = new CanvasImage(canvas, tex, position + pos, size, subSprite);

            images.Add(name, image);
        }

        public void AddText(string name, string text, Vector2 pos, Vector2 sz, Font font, int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            CanvasText t = new CanvasText(canvas, position + pos, sz, font, text, fontSize, style, alignment);

            texts.Add(name, t);
        }

        public CanvasButton GetButton(string buttonName, string panelName = null)
        {
            if (panelName != null && panels.ContainsKey(panelName))
            {
                return panels[panelName].GetButton(buttonName);
            }

            if (buttons.ContainsKey(buttonName))
            {
                return buttons[buttonName];
            }

            return null;
        }

        public CanvasImage GetImage(string imageName, string panelName = null)
        {
            if (panelName != null && panels.ContainsKey(panelName))
            {
                return panels[panelName].GetImage(imageName);
            }

            if (images.ContainsKey(imageName))
            {
                return images[imageName];
            }

            return null;
        }

        public CanvasPanel GetPanel(string panelName)
        {
            if (panels.ContainsKey(panelName))
            {
                return panels[panelName];
            }

            return null;
        }

        public CanvasText GetText(string textName, string panelName = null)
        {
            if (panelName != null && panels.ContainsKey(panelName))
            {
                return panels[panelName].GetText(textName);
            }

            if (texts.ContainsKey(textName))
            {
                return texts[textName];
            }

            return null;
        }

        public void SetPosition(Vector2 pos)
        {
            Vector2 deltaPos = pos - position;
            position = pos;

            foreach (CanvasButton button in buttons.Values)
            {
                button.SetPosition(button.GetPosition() + deltaPos);
            }

            foreach (CanvasImage image in images.Values)
            {
                image.SetPosition(image.GetPosition() + deltaPos);
            }

            foreach (CanvasText text in texts.Values)
            {
                text.SetPosition(text.GetPosition() + deltaPos);
            }

            foreach (CanvasPanel panel in panels.Values)
            {
                panel.SetPosition(panel.GetPosition() + deltaPos);
            }
        }

        public void TogglePanel(string name)
        {
            if (active && panels.ContainsKey(name))
            {
                panels[name].ToggleActive();

                foreach (CanvasPanel panel in panels.Values)
                {
                    if (panel != panels[name] && panel.active && panel.position == panels[name].position)
                    {
                        panel.SetActive(false, false);
                    }
                }
            }
        }

        public void ToggleActive()
        {
            active = !active;
            SetActive(active, false);
        }

        public void SetActive(bool b, bool panel)
        {
            foreach (CanvasButton button in buttons.Values)
            {
                button.SetActive(b);
            }

            foreach (CanvasImage image in images.Values)
            {
                image.SetActive(b);
            }

            foreach (CanvasText t in texts.Values)
            {
                t.SetActive(b);
            }

            if (panel)
            {
                foreach (CanvasPanel p in panels.Values)
                {
                    p.SetActive(b, false);
                }
            }

            active = b;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public void FixRenderOrder()
        {
            foreach (CanvasText t in texts.Values)
            {
                t.MoveToTop();
            }

            foreach (CanvasButton button in buttons.Values)
            {
                button.MoveToTop();
            }

            foreach (CanvasImage image in images.Values)
            {
                image.SetRenderIndex(0);
            }

            foreach (CanvasPanel panel in panels.Values)
            {
                panel.FixRenderOrder();
            }
        }

        public void Destroy()
        {
            foreach (CanvasButton button in buttons.Values)
            {
                button.Destroy();
            }

            foreach (CanvasImage image in images.Values)
            {
                image.Destroy();
            }

            foreach (CanvasText t in texts.Values)
            {
                t.Destroy();
            }

            foreach (CanvasPanel p in panels.Values)
            {
                p.Destroy();
            }
        }

        public Vector2 GetNextPos(MenuItems currentItem)
        {
            if (currentItem == MenuItems.TextButton)
            {
                if (NumButtons != 0) ypos += 30f;
                xpos = 5f;
                LastItem = MenuItems.TextButton;
            }
            else if (currentItem == MenuItems.ImageButton)
            {
                if (LastItem == MenuItems.TextButton)
                {
                    xpos = Xposes[0];
                    if (NumButtons != 0) ypos += 30f;
                }
                else if (LastItem == MenuItems.ImageButton)
                {
                    if (xpos == Xposes[1])
                    {
                        if (NumButtons != 0) ypos += 30f;
                    }
                    xpos = xpos == Xposes[0] ? Xposes[1] : Xposes[0];
                }
                LastItem = MenuItems.ImageButton;
            }

            NumButtons++;
            return new Vector2(xpos, ypos);;
        }
    }
}
