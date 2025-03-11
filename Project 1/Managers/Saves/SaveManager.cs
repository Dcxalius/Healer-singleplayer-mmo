using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Managers.Saves;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    internal static class SaveManager
    {
        static string contentRootDirectory;
        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto};
        static string saveFolder;
        static Dictionary<string, Save> saves;

        public static string CurrentSaveName => currentSave.Name;
        static Save currentSave;
        public static void Init(ContentManager aContentManager)
        {
            contentRootDirectory = aContentManager.RootDirectory;
            Save.Init(contentRootDirectory);
            saveFolder = contentRootDirectory + "\\Saves";

            InitSaveFolder();

            saves = new Dictionary<string, Save>();
            string[] folders = System.IO.Directory.GetDirectories(saveFolder);
            for (int i = 0; i < folders.Length; i++)
            {
                string name = TrimToNameOnly(folders[i]).ToUpper();
                saves.Add(name, new Save(name, true));
            }

        }

        public static bool NameAlreadyExists(string aName) => saves.ContainsKey(aName.ToUpper());

        public static void CreateNewSave(string aName)
        {
            aName = aName.ToUpper();
            saves.Add(aName, new Save(aName, false));
            currentSave = saves[aName];
        }

        static void InitSaveFolder()
        {
            if (System.IO.Directory.Exists(saveFolder)) return;

            System.IO.Directory.CreateDirectory(saveFolder);
        }

        public static void LoadData(string aName)
        {
            currentSave = saves[aName];
            currentSave.LoadData();
        }

        public static void SaveData() => currentSave.SaveData();

        public static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport, serializerSettings);
            System.IO.File.WriteAllText(contentRootDirectory + "\\Saves\\" + aDestination, json);
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
