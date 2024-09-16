using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.UIElements.PlateBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal static class ObjectManager
    {
        public static Player Player { get => player; }

        public static List<GameObject> gameObjects = new List<GameObject>();
        //public static List<StrongBox<GameObject>> gameObjects = new List<StrongBox<GameObject>>();

        static Player player = null;

        public static void Init()
        {
            player = new Player();
            //gameObjects.Add(new StrongBox<GameObject>(new Walker(new Microsoft.Xna.Framework.Vector2(200, 200))));
            gameObjects.Add(new Walker(new Microsoft.Xna.Framework.Vector2(200, 200)));
            //player.g.Add(gameObjects[0]);
            ((Walker)gameObjects[0]).AssumingDirectControl(ref player);
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

        public static void Click(ClickEvent aClickEvent)
        {
            bool foundHit = false;
            foundHit = player.Click(aClickEvent);
            for (int i = 0; i < gameObjects.Count && !foundHit; i++)
            {
                foundHit = gameObjects[i].Click(aClickEvent);
            }

            if (!foundHit && aClickEvent.ButtonPressed == ClickEvent.ClickType.Right)
            {

            }
        }
    }
}
