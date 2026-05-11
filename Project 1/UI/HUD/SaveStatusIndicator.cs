using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.UI.HUD
{
    internal class SaveStatusIndicator : Box
    {
        const double SavedDisplayMs = 5000d;
        const double FailedDisplayMs = 7000d;
        static readonly Color SavingColor = new Color(45, 45, 45, 210);
        static readonly Color SavedColor = new Color(45, 120, 65, 220);
        static readonly Color FailedColor = new Color(155, 55, 55, 220);
        static readonly RelativeScreenPosition IconRelativePos = new RelativeScreenPosition(0.04f, 0.15f);
        static readonly RelativeScreenPosition IconRelativeSize = new RelativeScreenPosition(0.14f, 0.7f);
        static readonly RelativeScreenPosition TextRelativePos = new RelativeScreenPosition(0.24f, 0f);

        readonly UITexture savingTexture = new UITexture("LeftSwirl", Color.White);
        readonly UITexture savedTexture = new UITexture("CheckMark", Color.White);
        readonly UITexture failedTexture = new UITexture("LeftSwirl", Color.IndianRed);
        readonly Text statusText = new Text("Comfortaa-msdf", Color.White, 12f);

        UITexture currentIcon;
        int activeSaves;
        double stateUntilMs;
        bool pendingFailure;
        string pendingFailureMessage;

        public SaveStatusIndicator(RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(null, new UITexture("GrayBackground", SavingColor), aPos, aSize)
        {
            currentIcon = savingTexture;
            Visible = false;
            Dragable = false;
            capturesClick = false;
            capturesRelease = false;
            capturesScroll = false;
            hudMoveable = false;
            AlwaysOnScreen = true;
            ForceVolatileRender = true;
        }

        public void NotifySaveStarted(string message)
        {
            if (activeSaves == 0)
            {
                pendingFailure = false;
                pendingFailureMessage = null;
            }

            activeSaves++;
            stateUntilMs = 0;
            ShowSaving(message);
        }

        public void NotifySaveFinished(string message)
        {
            if (activeSaves > 0) activeSaves--;
            if (activeSaves != 0) return;
            if (pendingFailure)
            {
                ShowFailed(pendingFailureMessage);
                return;
            }

            ShowSaved(message);
        }

        public void NotifySaveFailed(string message)
        {
            if (activeSaves > 0) activeSaves--;
            pendingFailure = true;
            pendingFailureMessage = string.IsNullOrWhiteSpace(message) ? "Save failed" : message;
            if (activeSaves == 0)
            {
                ShowFailed(pendingFailureMessage);
            }
        }

        public override void Update()
        {
            ThreadAffinity.AssertUiThread();
            if (activeSaves > 0)
            {
                Visible = true;
                return;
            }

            if (stateUntilMs > 0)
            {
                double now = TimeManager.InstanceTotalFrameTimeAsTimeSpan.TotalMilliseconds;
                if (now <= stateUntilMs)
                {
                    Visible = true;
                    return;
                }

                stateUntilMs = 0;
            }

            Visible = false;
        }

        public override void Rescale()
        {
            base.Rescale();
            statusText.Rescale();
        }

        protected override void DrawSelf(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (!Visible) return;

            base.DrawSelf(aBatch);
            if (currentIcon != null)
            {
                currentIcon.Draw(aBatch, BuildIconRectangle());
            }

            statusText.CentreLeftDraw(aBatch, BuildTextAnchor());
        }

        Rectangle BuildIconRectangle()
        {
            AbsoluteScreenPosition iconLocation = Location + (IconRelativePos * Size.ToRelativeScreenPosition()).ToAbsoluteScreenPos();
            AbsoluteScreenPosition iconSize = (IconRelativeSize * Size.ToRelativeScreenPosition()).ToAbsoluteScreenPos();
            return new Rectangle(iconLocation, iconSize);
        }

        AbsoluteScreenPosition BuildTextAnchor()
        {
            return Location + (TextRelativePos * Size.ToRelativeScreenPosition()).ToAbsoluteScreenPos() + new AbsoluteScreenPosition(0, Size.Y / 2);
        }

        void ShowSaving(string message)
        {
            gfx.Color = SavingColor;
            currentIcon = savingTexture;
            statusText.Value = string.IsNullOrWhiteSpace(message) ? "Saving..." : message;
            Visible = true;
        }

        void ShowSaved(string message)
        {
            gfx.Color = SavedColor;
            currentIcon = savedTexture;
            statusText.Value = string.IsNullOrWhiteSpace(message) ? "Saved" : message;
            stateUntilMs = TimeManager.InstanceTotalFrameTimeAsTimeSpan.TotalMilliseconds + SavedDisplayMs;
            Visible = true;
        }

        void ShowFailed(string message)
        {
            gfx.Color = FailedColor;
            currentIcon = failedTexture;
            statusText.Value = string.IsNullOrWhiteSpace(message) ? "Save failed" : message;
            stateUntilMs = TimeManager.InstanceTotalFrameTimeAsTimeSpan.TotalMilliseconds + FailedDisplayMs;
            Visible = true;
        }
    }
}
