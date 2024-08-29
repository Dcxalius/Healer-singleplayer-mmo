using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal static class ObjectManager
    {
        public static List<GameObject> gameObjects = new List<GameObject>();

        static Player player = null;

        public static void Init()
        {
            player = new Player();
            Camera.BindCamera(player);
        }

        public static void Update()
        {
            player.Update();
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Update();
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            player.Draw(spriteBatch);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(spriteBatch);
            }
        }
    }
}
