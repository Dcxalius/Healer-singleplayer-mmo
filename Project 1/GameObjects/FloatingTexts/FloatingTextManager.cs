using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.FloatingTexts
{
    internal static class FloatingTextManager
    {
        static List<FloatingText> floatingTexts;
        static volatile FloatingText[] renderFloatingTexts = Array.Empty<FloatingText>();
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            floatingTexts = new List<FloatingText>();
        }

        public static void AddFloatingText(FloatingText aFloater) => floatingTexts.Add(aFloater);

        public static void DoWhatLeaguePlayersTellMe(FloatingText aText) => floatingTexts.Remove(aText);


        public static void Reset() => floatingTexts.Clear();

        public static void Update() 
        {
            ThreadAffinity.AssertSimThread();
            for (int i = floatingTexts.Count - 1; i >= 0; i--) floatingTexts[i].Update();
        }

        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            FloatingText[] snapshot = renderFloatingTexts;
            for (int i = 0; i < snapshot.Length; i++) snapshot[i].Draw(aBatch);
        }

        internal static void BuildRenderSnapshot()
        {
            renderFloatingTexts = floatingTexts.ToArray();
        }
    }
}
