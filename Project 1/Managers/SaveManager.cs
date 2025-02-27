using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    internal static class SaveManager
    {
        static string contentRootDirectory;
        public static void Init(ContentManager aContentManager)
        {
            contentRootDirectory = aContentManager.RootDirectory;
        }

        public static void LoadData() //TODO: Make this take an argument to allow multiple saves
        {
            TileManager.LoadTiles(TileManager.DeserializeTilesIds(contentRootDirectory), new Microsoft.Xna.Framework.Point(0)); //TODO: Remove hardcoded
            ObjectManager.Load();
        }

        public static void SaveData()
        {
            ObjectFactory.SaveData();
            TileManager.SaveData();
        }

        public static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport);
            System.IO.File.WriteAllText(contentRootDirectory + "\\SaveData\\" + aDestination, json);
        }
    }
}
