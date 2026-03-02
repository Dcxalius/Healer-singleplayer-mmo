using Project_1.Camera;
using Project_1.Textures;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        public static void HoldSpell(GfxPath spellGfxPath, AbsoluteScreenPosition aGrabOffset)
        {
            AssertUiThreadOrMainFallback();
            heldSpell.HoldMe(spellGfxPath, aGrabOffset);
            InvalidateUi();
        }

        public static void ReleaseSpell()
        {
            AssertUiThreadOrMainFallback();
            heldSpell.ReleaseMe();
            InvalidateUi();
        }

        public static void FinishChannel()
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.FinishCast();
            InvalidateUi();
        }

        public static void CancelChannel()
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.CancelCast();
            InvalidateUi();
        }

        public static void UpdateChannelSpell(float aNewVal)
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.Value = aNewVal;
            InvalidateUi();
        }

        public static void ChannelSpell(GfxPath spellGfxPath, double castDurationMs)
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.CastSpell(spellGfxPath, castDurationMs);
            InvalidateUi();
        }

        public static void LoadSpellBar(string[] spellNames)
        {
            AssertUiThreadOrMainFallback();
            firstSpellBar.LoadBar(spellNames);
            InvalidateUi();
        }

        public static string[] SaveSpellBar
        {
            get
            {
                AssertUiThreadOrMainFallback();
                return firstSpellBar.SaveBar();
            }
        }
    }
}
