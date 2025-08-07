using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows
{
    internal class Window : Box
    {
        static RelativeScreenPosition furthestLeftWindow;
        static RelativeScreenPosition spacing;
        public static RelativeScreenPosition WindowSize => size;
        static RelativeScreenPosition size;

        static int nrOfBookletsOpen;
        static int maxNrOfOpenWindows;

        static RelativeScreenPosition GetNextOpenPosition => furthestLeftWindow + (spacing + new RelativeScreenPosition(size.X, 0)) * nrOfBookletsOpen;
        static List<Window> openWindows;

        public static bool IsWindowOpen(string aWindowName) => openWindows != null && openWindows.Any(window => window.GetType().Name == aWindowName);

        static public void Init(RelativeScreenPosition aFurthestLeftWindow, RelativeScreenPosition aSpacing, RelativeScreenPosition aSize)
        {
            nrOfBookletsOpen = 0;
            furthestLeftWindow = aFurthestLeftWindow;
            spacing = aSpacing;
            size = aSize;

            maxNrOfOpenWindows = 1 + (int)((1 - (furthestLeftWindow.X + size.X)) / (spacing.X + size.X));
            openWindows = new List<Window>();
        }
        public Window(UITexture aGfx) : base(aGfx, RelativeScreenPosition.Zero, size)
        {
            Visible = false;
            hudMoveable = false;

            RelativeScreenPosition buttonSize = RelativeScreenPosition.GetSquareFromX(0.05f, size.ToAbsoluteScreenPos());
            GFXButton gFXButton = new GFXButton(new List<Action>() { new Action(CloseWindow) }, new GfxPath(GfxType.UI, "XButton"), RelativeScreenPosition.One.OnlyX + new RelativeScreenPosition(-buttonSize.X, 0), buttonSize, Color.White);
            AddChild(gFXButton);
        }

        public void OpenWindow()
        {
            if (nrOfBookletsOpen >= maxNrOfOpenWindows) return;

            if (!Visible) OpenBooklet();
            Visible = true;
        }

        public void CloseWindow()
        {
            if (Visible) CloseBooklet();
            Visible = false;
        }

        public override void ToggleVisibilty()
        {
            if (nrOfBookletsOpen >= maxNrOfOpenWindows && !Visible) return;

            if (!Visible) OpenBooklet();
            if (Visible) CloseBooklet();

            base.ToggleVisibilty();

        }

        protected virtual void OpenBooklet()
        {
            Move(GetNextOpenPosition);
            openWindows.Add(this);
            nrOfBookletsOpen++;
        }

        protected virtual void CloseBooklet()
        {
            int startIndex = openWindows.FindIndex(xdd => xdd == this);
            nrOfBookletsOpen--;
            if (startIndex == -1) return;
            RelativeScreenPosition lastPos = RelativePos;
            RelativeScreenPosition nextPos = RelativePos;
            for (int i = startIndex; i < openWindows.Count; i++)
            {
                lastPos = openWindows[i].RelativePos;
                openWindows[i].Move(nextPos);
                nextPos = lastPos;
            }
            openWindows.Remove(this);
        }
    }
}
