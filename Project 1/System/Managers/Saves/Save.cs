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
using Project_1.Managers.States;
using System.IO;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.Managers.Saves
{
    internal class Save : IComparable<Save>
    {
        static int currentVersion = 0; 
        //TODO: Increment this whenever major changes to the save system is implemented
        //TODO: Codify this ^
        //TODO: Ponder how the game should treat changes from the player.

        public string Name => name;
        string name;
        bool isDeleted;
        public int Version => version;
        public string nameAsPath => contentRootDirectory + name.ToUpper();

        int version;

        //Q: Is there a cleaner way to do this? And if so, do we care?
        public string World => nameAsPath + "\\World";
        public string Corpses => World + "\\Corpses";
        public string SpawnZones => World + "\\SpawnZones";
        public string Tiles => World + "\\Tiles";

        public string Units => nameAsPath + "\\Units";
        public string Guild => Units + "\\Guild";
        public string InWorld => Units + "\\InWorld";
        public string Friendly => InWorld + "\\Friendly";
        public string NonFriendly => InWorld + "\\NonFriendly";

        static string contentRootDirectory => Game1.ContentManager.RootDirectory + "\\Saves\\";

        public string CameraPosition => nameAsPath + "\\Camera.pos";
        string SaveDetailsPath => nameAsPath + "\\Save.Details";
        internal string SaveDetailsPathForWrite => SaveDetailsPath;
        public string ImagePath => nameAsPath + "\\SaveImage.png";
        public SaveDetails SaveDetails => saveDetails;
        SaveDetails saveDetails;

        internal void SetSaveDetails(SaveDetails details)
        {
            saveDetails = details;
        }

        public Save(string aName, bool aExistingSave) 
        {
            //Q: Is it cleaner to check in here if its an existing save? Or possibly two seperate Save ctors, one for when creating a new game and one to call in the init loop
            name = aName;
            if (aExistingSave)
            {
                version = currentVersion;
                string file = File.ReadAllText(SaveDetailsPath);
                saveDetails = SaveManager.ImportData<SaveDetails>(file);
                return;
            }
            else
            {
                try
                {
                    CreateNewSaveFolder();
                    SaveData();
                }
                catch
                {
                    DeleteFiles();
                    throw;
                }
            }
        }

        void SaveSaveDetails()
        {
            string name = ObjectManager.Player.Name;
            string className = ObjectManager.Player.ClassData.Name;
            int level = ObjectManager.Player.Level.CurrentLevel;
            TimeSpan timeSpan = TimeManager.TotalFrameTimeAsTimeSpan;
            saveDetails = new SaveDetails(name, className, level, timeSpan);
            SaveManager.ExportData(SaveDetailsPath, saveDetails);
        }

        internal void SaveScreenshot()
        {
            //TODO: Come up with a good system to mark which methods exists only for the singlethread fallbacks
            ThreadAffinity.AssertMainThread();
            if (isDeleted || !Directory.Exists(nameAsPath)) return;
            AbsoluteScreenPosition windowSize = Camera.Camera.WindowSize;
            try
            {
                using Stream imageStream = File.Create(ImagePath);
                if (StateManager.FinalGameFrame != null)
                {
                    StateManager.FinalGameFrame.SaveAsPng(imageStream, windowSize.X, windowSize.Y);
                }
            }
            catch
            {
                //Q: Why is this done? What is the possible error entries?
                //Either way, we should probably check if the image already exists, back it up virtually, and then revert to the old picture if this part fails
                
                if (File.Exists(ImagePath))
                {
                    File.Delete(ImagePath);
                }
                throw;
            }
        }

        public void SaveData()
        {
            ThreadAffinity.AssertSimThread();
            SaveSaveDetails();
            Camera.Camera.SavePosition(this);

            ObjectFactory.SaveData(this);
            CorpseManager.Save(this);
            TileManager.SaveData(this);
            SpawnerManager.SaveData(this);
            //TimeManager.Save(this); //Done through savedetails atm
            if (ThreadAffinity.IsMainThread)
            {
                SaveScreenshot();
            }
            else
            {
                ScreenshotManager.RequestScreenshot(this);
            }
        }

        //Q: This feels like duped methods. Any particular reason why?
        public void ClearFolder(string aPath)
        {
            foreach (string filePath in Directory.GetFiles(aPath))
            {
                File.Delete(filePath);
            }
        }

        internal void DeleteFiles()
        {
            isDeleted = true;
            if (!Directory.Exists(nameAsPath)) return;
            Directory.Delete(nameAsPath, true);
        }
        //

        void CreateNewSaveFolder()
        {
            if (Directory.Exists(nameAsPath)) return; //Q: Should this be a throw? If the folder already exists shouldn't we check structure integrity?
            version = currentVersion;
            
            Directory.CreateDirectory(nameAsPath);
            Directory.CreateDirectory(World);
            Directory.CreateDirectory(SpawnZones);
            Directory.CreateDirectory(Tiles);

            Directory.CreateDirectory(Units);
            Directory.CreateDirectory(Guild);
            Directory.CreateDirectory(InWorld);
            Directory.CreateDirectory(Friendly);
            Directory.CreateDirectory(NonFriendly);
            Directory.CreateDirectory(Corpses);
        }

        bool VersionCheck(int aVersion)
        {
            if (aVersion == currentVersion) return true;

            //TODO: implement a system that adds/removes folders from a savefile depending on the version
            return false;
        }

        public int CompareTo(Save other)
        {
            if (saveDetails.TimeInfo > other.saveDetails.TimeInfo) return -1; 
            if (saveDetails.TimeInfo < other.saveDetails.TimeInfo) return 1;
            return 0;
        }
    }
}
