using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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
        public WorldSpace Position { get => position; protected set => position = value; }
        public WorldSpace Centre { get => position + new WorldSpace(size.ToVector2()) / 2; }
        public WorldSpace FeetPos { get => Position + (WorldSpace)new Vector2(size.X / 2, size.Y); }
        public Rectangle WorldRectangle { get => new Rectangle(position.ToPoint(), size); }
        
        protected Textures.Texture gfx;


        WorldSpace position;
        protected Point size;



        public GameObject(Textures.Texture aGfx, WorldSpace aStartingPos)
        {
            gfx = aGfx;
            position = aStartingPos;
            if (aGfx.Visible != null)
            {
                size = aGfx.Visible.Value.Size;
            }
            else
            {
                size = gfx.size;
            }
        }

        public virtual bool Click(ClickEvent aClickEvent) { return false; }

        public virtual void Update()
        {
            gfx.Update();
        }


        public virtual void Draw(SpriteBatch aBatch)
        {
            Debug.Assert(gfx != null);
            gfx.Draw(aBatch, Camera.Camera.WorldPosToCameraSpace(position), FeetPos.Y);
        }
    }
}
