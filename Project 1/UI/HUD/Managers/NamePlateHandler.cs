using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Managers
{
    internal class NamePlateHandler
    {
        Dictionary<Entity, NamePlate> namePlates = new Dictionary<Entity, NamePlate>();

        public void AddNamePlate(Entity aEntity, NamePlate aNamePlate) => namePlates.Add(aEntity, aNamePlate);

        public void RemoveNamePlate(Entity aEntity) => namePlates.Remove(aEntity);


        public void Update()
        {

            foreach (KeyValuePair<Entity, NamePlate> namePlate in namePlates)
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

            //DebugManager.Print(typeof(HUDManager), "Passes of nameplatedupdate was: " + passes);
        }

        void UpdateSingleNamePlate(NamePlate aNamePlate, Rectangle aRect)
        {
            Vector2 dir = (aNamePlate.AbsolutePos.Center - aRect.Center).ToVector2();
            dir.Normalize();

            if (float.IsNaN(dir.X) || float.IsNaN(dir.Y)) dir = Vector2.Zero;

            //DebugManager.Print(typeof(HUDManager), "Nameplate of " + aNamePlate.Name + " was at " + aNamePlate.RelativePos);

            aNamePlate.Move(aNamePlate.RelativePos + new AbsoluteScreenPosition((int)(aRect.Size.X / 2 * dir.X), (int)(aRect.Size.Y / 2 * dir.Y)).ToRelativeScreenPosition()); //+ new AbsoluteScreenPosition(1 * Math.Sign(dir.X), 1 * Math.Sign(dir.Y))).ToRelativeScreenPosition());

            //DebugManager.Print(typeof(HUDManager), "Nameplate of " + aNamePlate.Name + " is now at " + aNamePlate.RelativePos);
        }

        public void Rescale()
        {
            foreach (KeyValuePair<Entity, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Rescale();
            }
        }

        public void Draw(SpriteBatch aBatch)
        {
            foreach (KeyValuePair<Entity, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Draw(aBatch);
            }
        }
    }
}
