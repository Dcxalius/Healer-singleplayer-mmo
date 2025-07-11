using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Doodads
{
    internal static class DoodadManager
    {
        static List<Doodad> doodads;
        static DoodadManager()
        {

            doodads = new List<Doodad>();

            doodads.Add(new Chest(new Camera.WorldSpace(600, 600))); //DEBUG
        }


        public static void Update()
        {
            for (int i = 0; i < doodads.Count; i++)
            {
                doodads[i].Update();
            }
        }

        public static bool Click(ClickEvent aClick)
        {
            for (int i = 0; i < doodads.Count; i++)
            {
                if (doodads[i].Click(aClick)) return true;
            }
            return false;
        }

        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < doodads.Count; i++)
            {
                doodads[i].Draw(aBatch);
            }
        }
    }
}
