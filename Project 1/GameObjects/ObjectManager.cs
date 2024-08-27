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

        public static void Init()
        {
            gameObjects.Add(new Player());
            Camera.BindCamera((MovingObject)gameObjects[0]);
        }

        public static void Update()
        {
            foreach (GameObject obj in gameObjects)
            {
                obj.Update();
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (GameObject obj in gameObjects)
            {
                obj.Draw(spriteBatch);
            }
        }
    }
}
