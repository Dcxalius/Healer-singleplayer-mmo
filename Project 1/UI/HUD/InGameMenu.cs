using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;

namespace Project_1.UI.HUD
{
    internal sealed class InGameMenu : Box
    {
        const float ScreenButtonWidth = 0.025f;
        const float ScreenButtonHeight = 0.04f;
        const float ScreenButtonSpacing = 0.004f;
        const float ScreenEdgePaddingX = 0.01f;

        readonly List<InGameMenuButton> buttons = new List<InGameMenuButton>();

        public InGameMenu() : base(null, new UITexture("GrayBackground", new Color(35, 35, 35, 185)), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            hudMoveable = false;
            capturesClick = false;
            capturesRelease = false;
        }

        public void AddButton(string label, KeyBindManager.KeyListner key, Action action)
        {
            var button = new InGameMenuButton(this, label, key, action, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
            buttons.Add(button);
            Layout();
        }

        void Layout()
        {
            RelativeScreenPosition edgePadding = RelativeScreenPosition.GetSquareFromX(ScreenEdgePaddingX);
            float width = edgePadding.X * 2 + buttons.Count * ScreenButtonWidth + Math.Max(0, buttons.Count - 1) * ScreenButtonSpacing;
            float height = ScreenButtonHeight + edgePadding.Y * 2;

            Resize(new RelativeScreenPosition(width, height));
            Move(new RelativeScreenPosition(0.02f, 1f - height - 0.01f));

            for (int i = 0; i < buttons.Count; i++)
            {
                float x = edgePadding.X + i * (ScreenButtonWidth + ScreenButtonSpacing);
                buttons[i].Move(new RelativeScreenPosition(x / width, edgePadding.Y / height));
                buttons[i].Resize(new RelativeScreenPosition(ScreenButtonWidth / width, ScreenButtonHeight / height));
            }
        }
    }

    internal sealed class InGameMenuButton : Button
    {
        readonly string labelText;
        readonly KeyBindManager.KeyListner key;
        readonly Label titleLabel;//TODO: Replace text label with icon
        readonly Label hotkeyLabel;
        string lastHotkeyText;

        public InGameMenuButton(UIElement parent, string label, KeyBindManager.KeyListner key, Action action, RelativeScreenPosition pos, RelativeScreenPosition size)
            : base(parent, new List<Action> { action }, pos, size, Color.White)
        {
            labelText = label;
            this.key = key;

            titleLabel = new Label(this, new RelativeScreenPosition(0.04f, 0.08f), new RelativeScreenPosition(0.92f, 0.46f), Label.TextAllignment.Centred, Color.Black, aTextSize: 10f, aText: label);
            hotkeyLabel = new Label(this, new RelativeScreenPosition(0.04f, 0.52f), new RelativeScreenPosition(0.92f, 0.36f), Label.TextAllignment.Centred, Color.DarkMagenta, aTextSize: 8f);

            KeyBindManager.KeyBindingChanged += OnKeyBindingChanged;
            RefreshHotkeyText();
        }

        void OnKeyBindingChanged(KeyBindManager.KeyBindingChangedEvent e)
        {
            if (e.Listner != key) return;
            RefreshHotkeyText();
        }

        void RefreshHotkeyText()
        {
            string hotkey = GetHotkeyText();
            if (hotkey == lastHotkeyText) return;

            lastHotkeyText = hotkey;
            titleLabel.Text = labelText;
            hotkeyLabel.Text = hotkey;
        }

        string GetHotkeyText()
        {
            KeySet primaryKey = KeyBindManager.GetKey(true, key);
            if (primaryKey.Key != Microsoft.Xna.Framework.Input.Keys.None)
            {
                return primaryKey.ToString();
            }

            KeySet secondaryKey = KeyBindManager.GetKey(false, key);
            if (secondaryKey.Key != Microsoft.Xna.Framework.Input.Keys.None)
            {
                return secondaryKey.ToString();
            }

            return "";
        }
    }
}
