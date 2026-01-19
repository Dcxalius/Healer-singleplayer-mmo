using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
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
        static volatile Doodad[] renderDoodads = Array.Empty<Doodad>();
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            doodads = new List<Doodad>();

            doodads.Add(new Chest(new Camera.WorldSpace(600, 600))); //DEBUG
        }


        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < doodads.Count; i++)
            {
                doodads[i].Update();
            }
        }

        public static bool Click(ClickEvent aClick)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < doodads.Count; i++)
            {
                if (doodads[i].Click(aClick)) return true;
            }
            return false;
        }

        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            Doodad[] snapshot = renderDoodads;
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            renderDoodads = doodads.ToArray();
        }
    }
}
