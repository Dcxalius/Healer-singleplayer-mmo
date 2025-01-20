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
using Newtonsoft.Json;
using Microsoft.VisualBasic.ApplicationServices;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Content;
using Project_1.Managers;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Camera;
using Project_1.GameObjects.Spells.Projectiles;
using Project_1.GameObjects.Entities.Temp;
using Project_1.GameObjects.Spawners;

namespace Project_1.GameObjects
{
    internal static class ObjectManager
    {
        public static Player Player { get => player; }

        public static List<Entity> entities = new List<Entity>();
        public static List<Corpse> corpses = new List<Corpse>();
        public static List<Projectile> projectiles = new List<Projectile>();

        static Player player = null;

        static List<FloatingText> floatingTexts = new List<FloatingText>();

        public static void Init()
        {
            player = new Player(new WorldSpace(10*32));
            Camera.Camera.BindCamera(player);



            
            entities.Add(new Walker(new WorldSpace(11*32)));//Debug
            player.AddToParty(entities[entities.Count - 1] as Walker); //Debug


        }

        public static void DoWhatLeaguePlayersTellMe(FloatingText aText)
        {
            floatingTexts.Remove(aText);
        }

        public static void AddProjectile(Projectile aProjectile)
        {
            projectiles.Add(aProjectile);
        }

        public static void AddCorpse(Corpse aCorpse)
        {
            corpses.Add(aCorpse);
        }

        public static void DespawnCorpse(Corpse aCorpse)
        {
            corpses.Remove(aCorpse);
        }

        public static void RemoveEntity(Entity aObject)
        {
            entities.Remove(aObject);
        }

        public static void SpawnFloatingText(FloatingText aFloatingText)
        {
            floatingTexts.Add(aFloatingText);
        }

        public static void Update()
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();
                if (projectiles[i].IsFinished)
                {
                    projectiles.RemoveAt(i);
                }
            }

            player.Update();
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                entities[i].Update();
            }
            for (int i = corpses.Count - 1; i >= 0; i--)
            {
                corpses[i].Update();
            }
            for (int i = 0; i < floatingTexts.Count; i++)
            {
                floatingTexts[i].Update();
            }
            if (TimeManager.TotalFrameTime % 2000 < 1)
            {
                player.ServerTick();
                for (int i = 0; i < entities.Count; i++)
                {
                    entities[i].ServerTick();
                }
            }
        }


        public static void Click(ClickEvent aClickEvent)
        {
            if (player.Click(aClickEvent)) { return; }
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].Click(aClickEvent)) { return; }
            }
            if (SpawnerManager.Click(aClickEvent)) { return; }
            for (int i = 0; i < corpses.Count; i++)
            {
                if (corpses[i].Click(aClickEvent)) { return; }
            }


            LeftClickedGround(aClickEvent);
            RightClickGround(aClickEvent);
        }

        static void LeftClickedGround(ClickEvent aClickEvent)
        {

            if (aClickEvent.ButtonPressed == InputManager.ClickType.Left)
            {
                if (aClickEvent.ModifiersOr(new InputManager.HoldModifier[]{ InputManager.HoldModifier.Shift, InputManager.HoldModifier.Ctrl}))
                {
                    player.ClearCommand();
                }
                else
                {
                    player.RemoveTarget();
                }
            }
        }

        static void RightClickGround(ClickEvent aClickEvent)
        {
            if (aClickEvent.ButtonPressed == InputManager.ClickType.Right)
            {
                player.IssueMoveOrder(aClickEvent);
            }
        }

        public static void Draw(SpriteBatch aSpriteBatch)
        {
            for (int i = 0; i < corpses.Count; i++)
            {
                corpses[i].Draw(aSpriteBatch);
            }
            player.Draw(aSpriteBatch);
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Draw(aSpriteBatch);
            }

            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(aSpriteBatch);
            }

            for (int i = 0; i < floatingTexts.Count; i++)
            {
                floatingTexts[i].Draw(aSpriteBatch);
            }

        }
    }
}
