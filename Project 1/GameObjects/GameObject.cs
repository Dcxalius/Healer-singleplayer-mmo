using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public Vector2 Position { get => pos; }

        //public static int GameObjectsToDraw = 0;

        public Rectangle WorldRectangle { get => new Rectangle(pos.ToPoint(), size); }
        
        protected Textures.Texture gfx;


        protected Vector2 pos;
        protected Point size;

        public GameObject(Textures.Texture aGfx, Vector2 aStartingPos)
        {
            
            gfx = aGfx;
            pos = aStartingPos;
            size = aGfx.Visible.Value.Size;
        }

        public virtual bool Click(ClickEvent aClickEvent) { return false; }

        public virtual void Update()
        {
            gfx.Update();
            //DebugManager.Print(GetType(), "Objects draw: " + GameObjectsToDraw );
            //GameObjectsToDraw = 0;
        }


        public virtual void Draw(SpriteBatch aBatch)
        {
            Debug.Assert(gfx != null);

            //Should be moved to texture?
            if (Camera.MomAmIInFrame(new Rectangle(Camera.WorldPosToCameraSpace(pos).ToPoint(), (gfx.size.ToVector2() *Camera.Scale).ToPoint())))
            //if (Camera.MomAmIInFrame(new Rectangle(pos.ToPoint(), gfx.size)))
            {
                //GameObjectsToDraw++;
                //sb.Draw(gfx, Camera.WorldPosToCameraSpace(pos), Color.White);
                gfx.Draw(aBatch, Camera.WorldPosToCameraSpace(pos));
            }
        }

    }
}
