using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
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

        static public void Init(RelativeScreenPosition aFurthestLeftWindow, RelativeScreenPosition aSpacing, RelativeScreenPosition aSize)
        {
            nrOfBookletsOpen = 0;
            furthestLeftWindow = aFurthestLeftWindow;
            spacing = aSpacing;
            size = aSize;

            maxNrOfOpenWindows = 1 + (int)((1 - (furthestLeftWindow.X + size.X))/ (spacing.X + size.X));
            openWindows = new List<Window>();
        }
        public Window(UITexture aGfx) : base(aGfx, RelativeScreenPosition.Zero, size)
        {
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

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
