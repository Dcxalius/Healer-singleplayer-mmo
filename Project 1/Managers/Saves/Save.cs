using Project_1.GameObjects.Spawners;
using Project_1.GameObjects;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.Saves
{
    internal class Save
    {
        static int currentVersion = 0; //TODO: Increment this whenever major changes to the save system is implemented
        public string Name => name;
        string name;

        int version;

        public string World => name + "\\World";
        public string SpawnZones => name + "\\SpawnZones";
        public string Tiles => World + "\\Tiles";

        public string Units => name + "\\Units";
        public string Guild => Units + "\\Guild";
        public string InWorld => Units + "\\InWorld";
        public string Friendly => InWorld + "\\Friendly";
        public string NonFriendly => InWorld + "\\NonFriendly";

        static string contentRootDirectory;

        public static void Init(string aConentRootDirectory) => contentRootDirectory = aConentRootDirectory;

        public Save(string aName, bool aExistingSave) 
        {
            name = aName;
            if (aExistingSave)
            {
                version = 0;
                return;
            }

            CreateNewSaveFolder();
        }

        public void SaveData()
        {
            ObjectFactory.SaveData();
            TileManager.SaveData();
            SpawnerManager.SaveData();
        }

        public void LoadData()
        {
            TileManager.LoadTiles(TileManager.DeserializeTilesIds(contentRootDirectory), new Microsoft.Xna.Framework.Point(0)); //TODO: Remove hardcoded
            ObjectManager.Load();
        }

        void CreateNewSaveFolder()
        {
            if (System.IO.Directory.Exists(name)) return;
            //TODO: Load version from file
            version = 0;
            System.IO.Directory.CreateDirectory(name);
            System.IO.Directory.CreateDirectory(World);
            System.IO.Directory.CreateDirectory(SpawnZones);
            System.IO.Directory.CreateDirectory(Tiles);

            System.IO.Directory.CreateDirectory(Units);
            System.IO.Directory.CreateDirectory(Guild);
            System.IO.Directory.CreateDirectory(InWorld);
            System.IO.Directory.CreateDirectory(Friendly);
            System.IO.Directory.CreateDirectory(NonFriendly);
        }

        bool VersionCheck(int aVersion)
        {
            if (aVersion == currentVersion) return true;

            //TODO: implement a system that adds/removes folders from a savefile depending on the version
            return false;
        }

    }
}
