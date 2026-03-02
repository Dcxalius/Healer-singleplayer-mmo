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
        const int MaxCollisionPasses = 64;
        const int MinVerticalDriftBeforeHorizontalFallbackPx = 60;
        const int VerticalDriftByPlateHeightMultiplier = 3;
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
                namePlate.Value.ResetCollisionOffset();
            }

            namePlateScratch.Clear();
            foreach (NamePlate plate in namePlates.Values)
            {
                namePlateScratch.Add(plate);
            }

            if (namePlateScratch.Count < 2) return;

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
                    ResolveCollision(namePlateScratch[first], namePlateScratch[second], collisionRectScratch[i]);
                }
            }
            while (collisionRectScratch.Count > 0 && passes < MaxCollisionPasses);

            //DebugManager.Print("Passes of nameplatedupdate was: " + passes);
        }

        static void ResolveCollision(NamePlate first, NamePlate second, Rectangle overlap)
        {
            if (overlap.Width <= 0 || overlap.Height <= 0) return;

            bool preferVertical = ShouldPreferVerticalOffset(first, second);
            if (preferVertical)
            {
                ApplyVerticalSeparation(first, second, overlap);
                return;
            }

            ApplyHorizontalSeparation(first, second, overlap);
        }

        static bool ShouldPreferVerticalOffset(NamePlate first, NamePlate second)
        {
            int firstLimit = Math.Max(MinVerticalDriftBeforeHorizontalFallbackPx, first.AbsolutePos.Height * VerticalDriftByPlateHeightMultiplier);
            int secondLimit = Math.Max(MinVerticalDriftBeforeHorizontalFallbackPx, second.AbsolutePos.Height * VerticalDriftByPlateHeightMultiplier);
            bool firstTooFar = Math.Abs(first.CollisionOffset.Y) >= firstLimit;
            bool secondTooFar = Math.Abs(second.CollisionOffset.Y) >= secondLimit;
            return !firstTooFar && !secondTooFar;
        }

        static void ApplyVerticalSeparation(NamePlate first, NamePlate second, Rectangle overlap)
        {
            int direction = Math.Sign(first.AbsolutePos.Center.Y - second.AbsolutePos.Center.Y);
            if (direction == 0)
            {
                direction = first.AnchorAbsolutePosition.Y <= second.AnchorAbsolutePosition.Y ? -1 : 1;
            }

            int totalShift = overlap.Height + 1;
            int firstShift = totalShift / 2;
            int secondShift = totalShift - firstShift;
            first.AddCollisionOffset(new AbsoluteScreenPosition(0, direction * firstShift));
            second.AddCollisionOffset(new AbsoluteScreenPosition(0, -direction * secondShift));
        }

        static void ApplyHorizontalSeparation(NamePlate first, NamePlate second, Rectangle overlap)
        {
            int direction = Math.Sign(first.AbsolutePos.Center.X - second.AbsolutePos.Center.X);
            if (direction == 0)
            {
                direction = first.AnchorAbsolutePosition.X <= second.AnchorAbsolutePosition.X ? -1 : 1;
            }

            int totalShift = overlap.Width + 1;
            int firstShift = totalShift / 2;
            int secondShift = totalShift - firstShift;
            first.AddCollisionOffset(new AbsoluteScreenPosition(direction * firstShift, 0));
            second.AddCollisionOffset(new AbsoluteScreenPosition(-direction * secondShift, 0));
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

        public int CopyDrawList(NamePlateRenderSnapshot[] destination)
        {
            AssertUiOrMainThread();
            if (destination == null || destination.Length == 0) return 0;

            int max = Math.Min(destination.Length, namePlates.Count);
            int i = 0;
            foreach (NamePlate plate in namePlates.Values)
            {
                if (i >= max) break;
                destination[i++] = plate.BuildRenderSnapshot();
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
