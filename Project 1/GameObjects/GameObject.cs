using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal abstract class GameObject
    {
        protected WorldSpace Position { get => position; set => position = value; }
        public WorldSpace Centre { get => position + new WorldSpace(size.ToVector2()) / 2; }
        public WorldSpace FeetPosition { get => Position + new WorldSpace(size.X / 2, size.Y); set => Position = value - new WorldSpace(size.X / 2, size.Y); }
        public virtual WorldSpace FeetSize { get => new WorldSpace(Size); } //TODO: ??
        public virtual Rectangle WorldRectangle { get => new Rectangle(position.ToPoint(), size); }
        public virtual Rectangle ScreenRect { get => new Rectangle(position.ToAbsoltueScreenPosition().ToPoint(), (size.ToVector2() * Camera.Camera.Scale).ToPoint()); }
        public float DistanceTo(WorldSpace aPoint) => FeetPosition.DistanceTo(aPoint);
        public float DistanceTo(GameObject aGameObject) => FeetPosition.DistanceTo(aGameObject.FeetPosition);
        
        protected Textures.Texture gfx;


        WorldSpace position;

        public Point Size
        {
            get => size; 
            protected set
            {
                size = value;
                gfx.size = value;
            }
        }
        Point size;

        List<VisualEffect> effects;

        public GameObject(Textures.Texture aGfx, WorldSpace aStartingPos)
        {
            effects = new List<VisualEffect>();
            gfx = aGfx;
            if (aGfx.Visible != null)
            {
                size = aGfx.Visible.Value.Size;
            }
            else
            {
                size = gfx.size;
            }
            FeetPosition = aStartingPos;
        }

        public virtual void Update()
        {
            gfx.Update();
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                if (effects[i].IsFinished) effects.RemoveAt(i);
            }
        }

        public void Teleport(WorldSpace aNewPos) => FeetPosition = aNewPos;


        public void AddEffect(VisualEffect aEffect)
        {

            effects.Add(aEffect);
            effects.Last().Size = size;
        }

        public virtual void Draw(SpriteBatch aBatch)
        {
            Debug.Assert(gfx != null);

            gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition(), Color.White, FeetPosition.Y);
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].Draw(aBatch, Position, FeetPosition.Y + 0.01f);
            }
        }
    }
}
