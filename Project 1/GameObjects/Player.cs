using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.GameObjects;
using Project_1.Input;

namespace Project_1
{
    internal class Player : Entity
    {
        public Player() : base(new Textures.AnimatedTexture(new GfxPath(GfxType.Object, "Walker"), new Point(32), Textures.AnimatedTexture.AnimationType.Random, 0, TimeSpan.FromMilliseconds(500)), new Vector2(100,100), 100)
        { 
        
        }

        int speed = 50;

        public override void Update()
        {

            if (InputManager.GetHold(Keys.Left))
            {
                //velocity += new Vector2(-(float)(Speed * TimeManager.gt.ElapsedGameTime.TotalSeconds), 0);
                velocity.X -= (float)(speed * TimeManager.gt.ElapsedGameTime.TotalSeconds);
            }
            if (InputManager.GetHold(Keys.Right))
            {
                velocity.X += (float)(speed * TimeManager.gt.ElapsedGameTime.TotalSeconds);
            }
            if (InputManager.GetHold(Keys.Up))
            {
                velocity.Y -= (float)(speed * TimeManager.gt.ElapsedGameTime.TotalSeconds);
            }

            if (InputManager.GetHold(Keys.Down))
            {
                velocity.Y += (float)(speed * TimeManager.gt.ElapsedGameTime.TotalSeconds);
            }

            base.Update();
        }

       
    }
}
