using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Managers.Saves;
using Project_1.Tiles;
using Project_1.UI.HUD;
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
        public static string Settings => contentRootDirectory + "\\Settings";
        public static string HudSettings => Settings + "\\Hud.set";
        public static string DefaultSettings => Settings + "\\Default";
        public static string DefaultHudSettings => DefaultSettings + "\\Hud.def";
        public static Save[] Saves => saves.ToArray();
        static List<Save> saves;


        public static Save CurrentSave => currentSave;
        static Save currentSave;
        public static void Init(ContentManager aContentManager)
        {
            contentRootDirectory = aContentManager.RootDirectory;
            Save.Init(contentRootDirectory);
            saveFolder = contentRootDirectory + "\\Saves";

            InitSaveFolder();

            saves = new List<Save>();
            string[] folders = System.IO.Directory.GetDirectories(saveFolder);
            for (int i = 0; i < folders.Length; i++)
            {
                string name = TrimToNameOnly(folders[i]).ToUpper();
                saves.Add(new Save(name, true));
            }
            saves.Sort();
        }

        //public string LoadEntireFile(string aPath)
        //{
        //    try
        //    {
        //        System.IO.File.ReadAllText(aPath);
        //    }
        //    catch (Exception)
        //    {
                

        //        throw;
        //    }
        //}


        public static bool NameAlreadyExists(string aName) => saves.Find(x => x.Name == aName.ToUpper()) != null;

        public static void CreateNewSave(string aName)
        {
            aName = aName.ToUpper();
            saves.Add(new Save(aName, false));
            currentSave = saves.Last();
            saves.Sort();
        }

        static void InitSaveFolder()
        {
            if (System.IO.Directory.Exists(saveFolder)) return;

            System.IO.Directory.CreateDirectory(saveFolder);
        }

        //public static void LoadData(string aName) => LoadData(saves[aName]);

        public static void ContinueLastSave() => LoadData(saves.First());


        public static void LoadData(Save aSave)
        {
            currentSave = aSave;
            currentSave.LoadData();
        }

        public static void SaveHUD() => HUDManager.Save();

        public static void SaveData() => currentSave.SaveData();

        public static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport, serializerSettings);
            System.IO.File.WriteAllText(aDestination, json);
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
