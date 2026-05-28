using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Project_1.GameObjects.FloatingTexts
{
    internal static class FloatingTextManager
    {
        static List<FloatingText> floatingTexts;
        static readonly ConcurrentQueue<FloatingText> pendingAdds = new ConcurrentQueue<FloatingText>();
        static volatile bool clearRequested;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            floatingTexts = new List<FloatingText>();
        }

        public static void AddFloatingText(FloatingText aFloater)
        {
            ThreadAffinity.AssertSimThread();
            if (aFloater == null) return;
            pendingAdds.Enqueue(aFloater);
        }

        public static void DoWhatLeaguePlayersTellMe(FloatingText aText) => floatingTexts.Remove(aText);


        public static void Reset()
        {
            if (ThreadAffinity.IsMainThread) //Q: What is this check for? If things shouldn't be able to Clear the probably shouldn't be able to call Reset
            {
                floatingTexts.Clear();
                return;
            }
            clearRequested = true;
        }

        public static void Update() 
        {
            //TODO: Decide if the text living on the main thread is ok, or if the should live on sim and send their updates via events
            ThreadAffinity.AssertMainThread();
            if (clearRequested)
            {
                floatingTexts.Clear();
                clearRequested = false;
                while (pendingAdds.TryDequeue(out _)) { }
            }
            while (pendingAdds.TryDequeue(out FloatingText pending))
            {
                floatingTexts.Add(pending);
            }
            for (int i = floatingTexts.Count - 1; i >= 0; i--) floatingTexts[i].Update();
        }

        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < floatingTexts.Count; i++) floatingTexts[i].Draw(aBatch);
        }
    }
}
