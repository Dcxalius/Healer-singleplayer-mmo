using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.HUD;
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

        public static List<Entity> entities = new List<Entity>();

        static Player player = null;

        public static void Init()
        {
            player = new Player();
            entities.Add(new Walker(new Microsoft.Xna.Framework.Vector2(200, 200)));
            Camera.BindCamera(player);
        }

        public static void Remove(Entity aObject)
        {

            entities.Remove(aObject);
        }

        public static void Update()
        {
            player.Update();
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Update();
            }
        }


        public static void Click(ClickEvent aClickEvent)
        {
            bool foundHit = false;
            foundHit = player.Click(aClickEvent);
            for (int i = 0; i < entities.Count && !foundHit; i++)
            {
                foundHit = entities[i].Click(aClickEvent);
            }

            if(!foundHit && aClickEvent.ButtonPressed == ClickEvent.ClickType.Left)
            {
                player.ClearCommand();
                HUDManager.SetNewTarget(null);
            }

            if (!foundHit && aClickEvent.ButtonPressed == ClickEvent.ClickType.Right)
            {
                player.IssueMoveOrder(aClickEvent);
            }
        }
        public static void Draw(SpriteBatch aSpriteBatch)
        {
            player.Draw(aSpriteBatch);
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Draw(aSpriteBatch);
            }
        }
    }
}
