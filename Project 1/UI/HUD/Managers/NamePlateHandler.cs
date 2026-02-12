using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Managers
{
    internal class NamePlateHandler
    {
        Dictionary<int, NamePlate> namePlates = new Dictionary<int, NamePlate>();
        readonly List<NamePlate> namePlateScratch = new List<NamePlate>();
        readonly List<(int, int)> collisionIndexScratch = new List<(int, int)>();
        readonly List<Rectangle> collisionRectScratch = new List<Rectangle>();

        public void AddNamePlate(in EntityUiSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            if (snapshot.RenderId <= 0) return;
            if (namePlates.ContainsKey(snapshot.RenderId)) return;
            namePlates.Add(snapshot.RenderId, new NamePlate(snapshot));
            HUDManager.InvalidatePlates();
        }

        public void RefreshNamePlate(in EntityUiSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            if (snapshot.RenderId <= 0) return;
            if (!namePlates.TryGetValue(snapshot.RenderId, out NamePlate namePlate)) return;
            namePlate.Refresh(snapshot);
        }

        public void RemoveNamePlate(int renderId)
        {
            ThreadAffinity.AssertUiThread();
            if (renderId <= 0) return;
            namePlates.Remove(renderId);
            HUDManager.InvalidatePlates();
        }


        public void Update()
        {
            ThreadAffinity.AssertUiThread();

            foreach (KeyValuePair<int, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Update();
            }

            namePlateScratch.Clear();
            foreach (NamePlate plate in namePlates.Values)
            {
                namePlateScratch.Add(plate);
            }

            if (namePlateScratch.Count < 2) return;

            int maxPasses = 50;
            int passes = 0;
            do
            {
                passes++;
                collisionIndexScratch.Clear();
                collisionRectScratch.Clear();

                for (int i = 0; i < namePlateScratch.Count; i++)
                {
                    for (int j = i + 1; j < namePlateScratch.Count; j++)
                    {
                        Rectangle r = Rectangle.Intersect(namePlateScratch[i].AbsolutePos, namePlateScratch[j].AbsolutePos);
                        if (namePlateScratch[i].AbsolutePos == namePlateScratch[j].AbsolutePos)
                        {
                            namePlateScratch[i].Bump(new AbsoluteScreenPosition(0, -1 - (int)RandomManager.RollDouble() * 3));
                            r = Rectangle.Intersect(namePlateScratch[i].AbsolutePos, namePlateScratch[j].AbsolutePos);
                        }
                        if (r.Size.X != 0 && r.Size.Y != 0)
                        {
                            collisionIndexScratch.Add((i, j));
                            collisionRectScratch.Add(r);
                        }
                    }
                }

                for (int i = 0; i < collisionRectScratch.Count; i++)
                {
                    (int first, int second) = collisionIndexScratch[i];
                    UpdateSingleNamePlate(namePlateScratch[first], collisionRectScratch[i]);
                    UpdateSingleNamePlate(namePlateScratch[second], collisionRectScratch[i]);
                }
            }
            while (collisionRectScratch.Count > 0 && passes < maxPasses);

            //DebugManager.Print("Passes of nameplatedupdate was: " + passes);
        }

        void UpdateSingleNamePlate(NamePlate aNamePlate, Rectangle aRect)
        {
            Vector2 dir = (aNamePlate.AbsolutePos.Center - aRect.Center).ToVector2();
            dir.Normalize();

            if (float.IsNaN(dir.X) || float.IsNaN(dir.Y)) dir = Vector2.Zero;

            //DebugManager.Print("Nameplate of " + aNamePlate.Name + " was at " + aNamePlate.RelativePos);

            aNamePlate.Move(aNamePlate.RelativePos + new AbsoluteScreenPosition((int)(aRect.Size.X / 2 * dir.X), (int)(aRect.Size.Y / 2 * dir.Y)).ToRelativeScreenPosition()); //+ new AbsoluteScreenPosition(1 * Math.Sign(dir.X), 1 * Math.Sign(dir.Y))).ToRelativeScreenPosition());

            //DebugManager.Print("Nameplate of " + aNamePlate.Name + " is now at " + aNamePlate.RelativePos);
        }

        public void Rescale()
        {
            AssertUiOrMainThread();
            foreach (KeyValuePair<int, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Rescale();
            }
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            foreach (KeyValuePair<int, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Draw(aBatch);
            }
        }

        public int DrawListCount
        {
            get
            {
                AssertUiOrMainThread();
                return namePlates.Count;
            }
        }

        public int CopyDrawList(NamePlate[] destination)
        {
            AssertUiOrMainThread();
            if (destination == null || destination.Length == 0) return 0;

            int max = Math.Min(destination.Length, namePlates.Count);
            int i = 0;
            foreach (NamePlate plate in namePlates.Values)
            {
                if (i >= max) break;
                destination[i++] = plate;
            }
            return i;
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }
    }
}
