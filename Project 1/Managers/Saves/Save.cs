using Project_1.GameObjects.Spawners;
using Project_1.GameObjects;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Unit;

namespace Project_1.Managers.Saves
{
    internal class Save
    {
        static int currentVersion = 0; //TODO: Increment this whenever major changes to the save system is implemented
        public string Name => name;
        string name;
        public string nameAsPath => contentRootDirectory + name.ToUpper();

        int version;

        public string World => nameAsPath + "\\World";
        public string Corpses => World + "\\Corpses";
        public string SpawnZones => World + "\\SpawnZones";
        public string Tiles => World + "\\Tiles";

        public string Units => nameAsPath + "\\Units";
        public string Guild => Units + "\\Guild";
        public string InWorld => Units + "\\InWorld";
        public string Friendly => InWorld + "\\Friendly";
        public string NonFriendly => InWorld + "\\NonFriendly";

        static string contentRootDirectory;

        public string CameraSettings => nameAsPath + "\\Camera.pos";
        string SaveDetailsPath => nameAsPath + "\\Save.Details";
        public SaveDetails SaveDetails => saveDetails;
        SaveDetails saveDetails;

        public static void Init(string aContentRootDirectory) => contentRootDirectory = aContentRootDirectory + "\\Saves\\";

        public Save(string aName, bool aExistingSave) 
        {
            name = aName;
            if (aExistingSave)
            {
                version = 0; //TODO: Implement this system
                string file = System.IO.File.ReadAllText(SaveDetailsPath);
                saveDetails = SaveManager.ImportData<SaveDetails>(file);
                return;
            }
            else
            {
                CreateNewSaveFolder();

                SaveData();
                //saveDetails = new SaveDetails(name, "className", 0);
                //SaveManager.ExportData(SaveDetailsPath, saveDetails);
                //Camera.Camera.Save(this);
            }
        }

        void SaveSaveDetails()
        {
            string name = ObjectManager.Player.Name;
            string className = ObjectManager.Player.ClassData.Name;
            int level = ObjectManager.Player.Level.CurrentLevel;
            saveDetails = new SaveDetails(name, className, level);
            SaveManager.ExportData(SaveDetailsPath, saveDetails);
        }

        public void SaveData()
        {
            SaveSaveDetails();
            Camera.Camera.Save(this);
            ObjectFactory.SaveData(this);
            ObjectManager.Save(this);
            TileManager.SaveData(this);
            SpawnerManager.SaveData(this);
        }

        public void LoadData()
        {

            Camera.Camera.Load(this);
            TileManager.Load(this); 
            ObjectManager.Load(this);
            SpawnerManager.Load(this);
            //TimeManager.Load(); //TODO: Implement
        }

        public void ClearFolder(string aPath)
        {
            string[] files = System.IO.Directory.GetDirectories(aPath);
            for (int i = 0; i < files.Length; i++)
            {
                System.IO.Directory.Delete(files[i]);
            }
        }

        void CreateNewSaveFolder()
        {
            if (System.IO.Directory.Exists(nameAsPath)) return;
            //TODO: Load version from file
            version = 0;
            System.IO.Directory.CreateDirectory(nameAsPath);
            System.IO.Directory.CreateDirectory(World);
            System.IO.Directory.CreateDirectory(SpawnZones);
            System.IO.Directory.CreateDirectory(Tiles);

            System.IO.Directory.CreateDirectory(Units);
            System.IO.Directory.CreateDirectory(Guild);
            System.IO.Directory.CreateDirectory(InWorld);
            System.IO.Directory.CreateDirectory(Friendly);
            System.IO.Directory.CreateDirectory(NonFriendly);
            System.IO.Directory.CreateDirectory(Corpses);
        }

        bool VersionCheck(int aVersion)
        {
            if (aVersion == currentVersion) return true;

            //TODO: implement a system that adds/removes folders from a savefile depending on the version
            return false;
        }

    }
}
