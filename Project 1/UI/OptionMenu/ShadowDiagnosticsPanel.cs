using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.OptionMenu
{
    internal class ShadowDiagnosticsPanel : Box
    {
        const float HeaderHeight = 0.06f;
        const float RowHeight = 0.055f;
        const float RowSpacing = 0.01f;

        float nextRowY;

        public ShadowDiagnosticsPanel(RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(new UITexture("GrayBackground", Color.WhiteSmoke), aPos, aSize)
        {
            capturesClick = false;
            capturesRelease = false;

            AddChild(new Label("Shadow Diagnostics", new RelativeScreenPosition(0.03f, 0.02f), new RelativeScreenPosition(0.94f, HeaderHeight), Label.TextAllignment.CentreLeft, Color.Black));

            nextRowY = 0.1f;
            AddToggleRow(
                "Preview Mask",
                DebugManager.Mode(DebugMode.ShadowMaskPreview),
                () => SetDebugMode(DebugMode.ShadowMaskPreview, true),
                () => SetDebugMode(DebugMode.ShadowMaskPreview, false));

            AddToggleRow(
                "Magenta Clear",
                DebugManager.Mode(DebugMode.ShadowMagentaClear),
                () => SetDebugMode(DebugMode.ShadowMagentaClear, true),
                () => SetDebugMode(DebugMode.ShadowMagentaClear, false));
        }

        void AddToggleRow(string aLabel, bool aStartState, Action aOnTicked, Action aOnUnticked)
        {
            AddChild(new DebugToggleRow(aLabel, aStartState, aOnTicked, aOnUnticked, new RelativeScreenPosition(0.03f, nextRowY), new RelativeScreenPosition(0.94f, RowHeight)));
            nextRowY += RowHeight + RowSpacing;
        }

        void SetDebugMode(DebugMode aMode, bool aEnabled)
        {
            bool oldValue = DebugManager.Mode(aMode);
            if (oldValue == aEnabled) return;

            DebugManager.SetMode(aMode, aEnabled);
            OptionManager.AddActionToDoAtExitOfOptionMenu(
                () => DebugManager.SetMode(aMode, oldValue),
                DebugManager.ExportSettings);
        }
    }
}
