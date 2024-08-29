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
        }


        public void Draw(SpriteBatch aBatch)
        {
            Debug.Assert(gfx != null);

            if (Camera.MomAmIInFrame(new Rectangle(pos.ToPoint(), gfx.size)))
            {
                //sb.Draw(gfx, Camera.WorldPosToCameraSpace(pos), Color.White);
                gfx.Draw(aBatch, Camera.WorldPosToCameraSpace(pos));
            }
        }

    }
}
