using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.OptionMenu
{
    internal class DebugOptionsPanel : Box
    {
        const float HeaderHeight = 0.06f;
        const float RowHeight = 0.055f;
        const float RowSpacing = 0.01f;

        float nextRowY;

        public DebugOptionsPanel(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.WhiteSmoke), aPos, aSize)
        {
            capturesClick = false;
            capturesRelease = false;

            AddChild(new Label("Debug Overlay", new RelativeScreenPosition(0.03f, 0.02f), new RelativeScreenPosition(0.94f, HeaderHeight), Label.TextAllignment.CentreLeft, Color.Black));

            nextRowY = 0.1f;
            AddToggleRow(
                "Enabled",
                DebugManager.Mode(DebugMode.DebugOverlay),
                () => SetDebugOverlayEnabled(true),
                () => SetDebugOverlayEnabled(false));

            nextRowY += 0.02f;
            AddChild(new Label("Visible Overlay Info", new RelativeScreenPosition(0.03f, nextRowY), new RelativeScreenPosition(0.94f, HeaderHeight), Label.TextAllignment.CentreLeft, Color.Black));
            nextRowY += HeaderHeight + 0.01f;

            AddOverlayToggle("FPS", DebugOverlayInfo.Fps);
            AddOverlayToggle("Frame Time", DebugOverlayInfo.FrameTime);
            AddOverlayToggle("Total Time", DebugOverlayInfo.TotalTime);
            AddOverlayToggle("Mailbox Queue", DebugOverlayInfo.MailboxQueue);
            AddOverlayToggle("Mailbox Dispatch", DebugOverlayInfo.MailboxDispatch);
            AddOverlayToggle("Worker Pool", DebugOverlayInfo.Worker);
            AddOverlayToggle("Screenshot Queue", DebugOverlayInfo.ScreenshotQueue);
            AddOverlayToggle("Sim Thread", DebugOverlayInfo.SimThread);
            AddOverlayToggle("UI Thread", DebugOverlayInfo.UiThread);
            AddOverlayToggle("Render Sync", DebugOverlayInfo.RenderSync);
        }

        void AddOverlayToggle(string aLabel, DebugOverlayInfo aInfo)
        {
            AddToggleRow(
                aLabel,
                DebugManager.OverlayInfoEnabled(aInfo),
                () => SetOverlayInfoEnabled(aInfo, true),
                () => SetOverlayInfoEnabled(aInfo, false));
        }

        void AddToggleRow(string aLabel, bool aStartState, Action aOnTicked, Action aOnUnticked)
        {
            AddChild(new DebugToggleRow(aLabel, aStartState, aOnTicked, aOnUnticked, new RelativeScreenPosition(0.03f, nextRowY), new RelativeScreenPosition(0.94f, RowHeight)));
            nextRowY += RowHeight + RowSpacing;
        }

        void SetDebugOverlayEnabled(bool aEnabled)
        {
            bool oldValue = DebugManager.Mode(DebugMode.DebugOverlay);
            if (oldValue == aEnabled) return;

            DebugManager.SetMode(DebugMode.DebugOverlay, aEnabled);
            OptionManager.AddActionToDoAtExitOfOptionMenu(
                () => DebugManager.SetMode(DebugMode.DebugOverlay, oldValue),
                DebugManager.ExportSettings);
        }

        void SetOverlayInfoEnabled(DebugOverlayInfo aInfo, bool aEnabled)
        {
            bool oldValue = DebugManager.OverlayInfoEnabled(aInfo);
            if (oldValue == aEnabled) return;

            DebugManager.SetOverlayInfo(aInfo, aEnabled);
            OptionManager.AddActionToDoAtExitOfOptionMenu(
                () => DebugManager.SetOverlayInfo(aInfo, oldValue),
                DebugManager.ExportSettings);
        }
    }

    internal class DebugToggleRow : UIElement
    {
        public DebugToggleRow(string aLabel, bool aStartState, Action aOnTicked, Action aOnUnticked, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(new UITexture("WhiteBackground", Color.Wheat), aPos, aSize)
        {
            capturesClick = false;
            capturesRelease = false;

            CheckBox checkBox = new CheckBox(aStartState, aOnTicked, aOnUnticked, new RelativeScreenPosition(0.01f, 0.14f), new RelativeScreenPosition(0.075f, 0.72f));
            Label label = new Label(aLabel, new RelativeScreenPosition(0.11f, 0), new RelativeScreenPosition(0.88f, 1), Label.TextAllignment.CentreLeft, Color.Black);

            AddChild(checkBox);
            AddChild(label);
        }
    }
}
