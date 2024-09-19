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

        static Player player = null;

        public static void Init()
        {
            player = new Player();
            gameObjects.Add(new Walker(new Microsoft.Xna.Framework.Vector2(200, 200)));
            Camera.BindCamera(player);
        }

        public static void Remove(GameObject aObject)
        {

            gameObjects.Remove(aObject);
        }

        public static void Update()
        {
            player.Update();
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Update();
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

            if(!foundHit && aClickEvent.ButtonPressed == ClickEvent.ClickType.Left)
            {
                player.ClearCommand();
            }

            if (!foundHit && aClickEvent.ButtonPressed == ClickEvent.ClickType.Right)
            {
                player.IssueMoveOrder(aClickEvent);
            }
        }
        public static void Draw(SpriteBatch aSpriteBatch)
        {
            player.Draw(aSpriteBatch);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(aSpriteBatch);
            }
        }
    }
}
