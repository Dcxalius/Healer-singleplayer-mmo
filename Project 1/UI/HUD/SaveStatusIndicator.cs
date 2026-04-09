using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;

namespace Project_1.UI.HUD
{
    internal class SaveStatusIndicator : UIElement
    {
        const double SavedDisplayMs = 5000d;

        readonly UITexture savingTexture;
        readonly UITexture savedTexture;
        int activeSaves;
        double savedUntilMs;

        public SaveStatusIndicator(RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(null, new UITexture("LeftSwirl", Color.White), aPos, aSize)
        {
            savingTexture = gfx;
            savedTexture = new UITexture("CheckMark", Color.White);
            Visible = false;
            Dragable = false;
            capturesClick = false;
            capturesRelease = false;
            capturesScroll = false;
            hudMoveable = false;
            AlwaysOnScreen = true;
        }

        public void NotifySaveStarted()
        {
            activeSaves++;
            savedUntilMs = 0;
            gfx = savingTexture;
            Visible = true;
        }

        public void NotifySaveFinished()
        {
            if (activeSaves > 0) activeSaves--;
            if (activeSaves == 0)
            {
                savedUntilMs = TimeManager.InstanceTotalFrameTimeAsTimeSpan.TotalMilliseconds + SavedDisplayMs;
                gfx = savedTexture;
                Visible = true;
            }
        }

        public override void Update()
        {
            ThreadAffinity.AssertUiThread();
            if (activeSaves > 0)
            {
                if (!ReferenceEquals(gfx, savingTexture))
                {
                    gfx = savingTexture;
                }
                Visible = true;
                return;
            }

            if (savedUntilMs > 0)
            {
                double now = TimeManager.InstanceTotalFrameTimeAsTimeSpan.TotalMilliseconds;
                if (now <= savedUntilMs)
                {
                    if (!ReferenceEquals(gfx, savedTexture))
                    {
                        gfx = savedTexture;
                    }
                    Visible = true;
                    return;
                }
                savedUntilMs = 0;
            }

            Visible = false;
        }
    }
}
