using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
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
        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto};
        public static void Init(ContentManager aContentManager)
        {
            contentRootDirectory = aContentManager.RootDirectory;
        }

        public static void LoadData() //TODO: Make this take an argument to allow multiple saves
        {
            TileManager.LoadTiles(TileManager.DeserializeTilesIds(contentRootDirectory), new Microsoft.Xna.Framework.Point(0)); //TODO: Remove hardcoded
            ObjectManager.Load();
        }

        public static void SaveData() //TODO: Make this take an argument to allow multiple saves
        {
            ObjectFactory.SaveData();
            TileManager.SaveData();
            SpawnerManager.SaveData();
        }

        public static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport, serializerSettings);
            System.IO.File.WriteAllText(contentRootDirectory + "\\SaveData\\" + aDestination, json);
        }

        public static T ImportData<T>(string aJsonString)
        {
            return JsonConvert.DeserializeObject<T>(aJsonString, serializerSettings);
        }

        public static string TrimToNameOnly(string aFile)
        {
            string fileOnly = aFile.Split('\\').Last();
            return fileOnly.Split(".")[0];
        }
    }
}
