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

namespace Project_1.GameObjects
{
    internal static class ObjectManager
    {
        public static Player Player { get => player; }

        public static List<Entity> entities = new List<Entity>();

        static Player player = null;

        static Dictionary<string, UnitData> unitData = new Dictionary<string, UnitData>();
        static UnitData defaultData = new UnitData();

        public static void Init(ContentManager aC)
        {
            UnitData data = new UnitData("Sheep", 100, UnitData.RelationToPlayer.Neutral, 50);
            //ExportData("C:\\Users\\Cassandra\\source\\repos\\Project 1\\Project 1\\Content\\UnitData.json", data);
            ImportData(aC.RootDirectory, aC);
            player = new Player();
            Camera.BindCamera(player);
            entities.Add(new Walker(new Microsoft.Xna.Framework.Vector2(200, 200)));
            player.AddToParty(entities[entities.Count - 1] as Walker);
            entities.Add(new Sheep(new Microsoft.Xna.Framework.Vector2(500, 500)));

        }

        public static UnitData GetData(string aName)
        {
            if (unitData.ContainsKey(aName))
            {
                return unitData[aName];
            }
            else
            {
                DebugManager.Print(typeof(ObjectManager), "Error getting data for unit " +  aName);
                return defaultData;
            }
        }

        static void ImportData(string aPathToData, ContentManager aContentManager)
        {
            string[] dataAsString = System.IO.File.ReadAllLines(aPathToData + "\\Data\\UnitData.json");

            //string[] dataAsString = aContentManager.Load<string[]>("Data\\UnitData.json");


            for (int i = 0; i < dataAsString.Length; i++)
            {
                UnitData data = JsonConvert.DeserializeObject<UnitData>(dataAsString[i]);
                unitData.Add(data.Name, data);

            }

        }

        static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport);

            System.IO.File.WriteAllText(aDestination, json);
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
