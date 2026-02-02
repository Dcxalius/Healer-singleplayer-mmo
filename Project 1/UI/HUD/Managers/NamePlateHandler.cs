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

            int maxPasses = 50;
            int passes = 0;
            List<Rectangle> collisonRects = new List<Rectangle>();
            do
            {
                passes++;
                List<NamePlate> namePlates = this.namePlates.Values.ToList();


                List<(int, int)> collisionIndexes = new List<(int, int)>();
                collisonRects.Clear();

                for (int i = 0; i < namePlates.Count; i++)
                {
                    for (int j = 0; j < namePlates.Count; j++)
                    {
                        if (i == j) continue;
                        if (collisionIndexes.Contains((j, i))) continue;

                        Rectangle r = Rectangle.Intersect(namePlates[i].AbsolutePos, namePlates[j].AbsolutePos);
                        //Debug.Assert(namePlates[i].AbsolutePos != namePlates[j].AbsolutePos);
                        if (namePlates[i].AbsolutePos == namePlates[j].AbsolutePos)
                        {
                            namePlates[i].Bump(new AbsoluteScreenPosition(0, -1 - (int)RandomManager.RollDouble() * 3));
                            r = Rectangle.Intersect(namePlates[i].AbsolutePos, namePlates[j].AbsolutePos);
                        }
                        if (r.Size.X != 0 && r.Size.Y != 0)
                        {
                            collisionIndexes.Add((i, j));
                            collisonRects.Add(r);
                        }
                    }
                }

                for (int i = 0; i < collisonRects.Count; i++)
                {
                    UpdateSingleNamePlate(namePlates[collisionIndexes[i].Item1], collisonRects[i]);
                    UpdateSingleNamePlate(namePlates[collisionIndexes[i].Item2], collisonRects[i]);
                }
            }
            while (collisonRects.Count > 0 && passes < maxPasses);

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

        public NamePlate[] GetDrawList()
        {
            AssertUiOrMainThread();
            return namePlates.Values.ToArray();
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }
    }
}
