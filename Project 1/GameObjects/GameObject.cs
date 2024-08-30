using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1
{
    internal abstract class GameObject
    {
        public Vector2 Position { get => pos; }

        //public static int GameObjectsToDraw = 0;

        
        protected Textures.Texture gfx;


        protected Vector2 pos;

        public GameObject(Textures.Texture aGfx, Vector2 aStartingPos)
        {
            
            gfx = aGfx;
            pos = aStartingPos;
        }

        public virtual void Update()
        {
            gfx.Update();
            //DebugManager.Print(GetType(), "Objects draw: " + GameObjectsToDraw );
            //GameObjectsToDraw = 0;
        }


        public void Draw(SpriteBatch aBatch)
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
