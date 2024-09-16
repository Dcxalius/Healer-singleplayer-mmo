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
using System.Runtime.CompilerServices;

namespace Project_1
{
    internal class Player : Entity
    {
        public List<StrongBox<Walker>> g = new List<StrongBox<Walker>>();

        int speed = 50;

        public Player() : base(new Textures.AnimatedTexture(new GfxPath(GfxType.Object, "Player"), new Point(32), Textures.AnimatedTexture.AnimationType.Random, 0, TimeSpan.FromMilliseconds(500)), new Vector2(100,100), 100)
        { 
        
        }


        void MouseWalk()
        {
            if (HasDestination) { return; }
            if (InputManager.GetHold(Keys.Left))
            {
                velocity.X -= (float)(speed * TimeManager.SecondsSinceLastFrame);
            }
            if (InputManager.GetHold(Keys.Right))
            {
                velocity.X += (float)(speed * TimeManager.SecondsSinceLastFrame);
            }
            if (InputManager.GetHold(Keys.Up))
            {
                velocity.Y -= (float)(speed * TimeManager.SecondsSinceLastFrame);
            }
            if (InputManager.GetHold(Keys.Down))
            {
                velocity.Y += (float)(speed * TimeManager.SecondsSinceLastFrame);
            }
        }

        public override void Update()
        {
            MouseWalk();
            g[0].Value.TEST();

            base.Update();
        }

       public void AddToCommand(Walker a)
        {
            g.Add(new StrongBox<Walker>(a));
        }
    }
}
