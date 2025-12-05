using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Managers.Saves;
using Project_1.UI.HUD.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static string Effects => Path.Combine(contentRootDirectory, "Effects");
        public static string Settings => Path.Combine(contentRootDirectory, "Settings");
        public static string HudSettings => Path.Combine(Settings, "Hud.set");
        public static string CameraSettings => Path.Combine(Settings, "Camera.set");
        public static string KeyBindSettings => Path.Combine(Settings, "KeyBind.set");



        public static string DefaultSettings => Path.Combine(Settings, "Default");
        public static string DefaultHudSettings => Path.Combine(DefaultSettings, "Hud.def");
        public static string DefaultCameraSettings => Path.Combine(DefaultSettings, "Camera.def");
        public static string DefaultKeyBindSettings => Path.Combine(DefaultSettings, "KeyBind.def");
        public static Save[] Saves => saves.ToArray();
        static List<Save> saves;


        public static Save CurrentSave => currentSave;
        static Save currentSave;

        static SaveManager()
        {
            contentRootDirectory = Game1.ContentManager.RootDirectory;

            saveFolder = Path.Combine(contentRootDirectory, "Saves");

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
            string fileOnly = Path.GetFileName(aFile);
            return Path.GetFileNameWithoutExtension(fileOnly);
        }
    }
}
