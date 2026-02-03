using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Managers.Saves;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Tiles;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.Managers
{
    internal static class SaveManager
    {
        static string contentRootDirectory;
        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto};
        static string saveFolder;

        public static string Effects => System.IO.Path.Combine(contentRootDirectory, "Effects");
        public static string Settings => System.IO.Path.Combine(contentRootDirectory, "Settings");
        public static string HudSettings => System.IO.Path.Combine(Settings, "Hud.set");
        public static string CameraSettings => System.IO.Path.Combine(Settings, "Camera.set");
        public static string KeyBindSettings => System.IO.Path.Combine(Settings, "KeyBind.set");



        public static string DefaultSettings => System.IO.Path.Combine(Settings, "Default");
        public static string DefaultHudSettings => System.IO.Path.Combine(DefaultSettings, "Hud.def");
        public static string DefaultCameraSettings => System.IO.Path.Combine(DefaultSettings, "Camera.def");
        public static string DefaultKeyBindSettings => System.IO.Path.Combine(DefaultSettings, "KeyBind.def");
        public static Save[] Saves
        {
            get
            {
                ThreadAffinity.AssertGameThread();
                lock (savesLock)
                {
                    return saves.ToArray();
                }
            }
        }
        static List<Save> saves;
        static readonly object savesLock = new object();


        public static Save CurrentSave => currentSave;
        static volatile Save currentSave;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            contentRootDirectory = Game1.ContentManager.RootDirectory;

            saveFolder = System.IO.Path.Combine(contentRootDirectory, "Saves");

            InitSaveFolder();

            saves = new List<Save>();
            string[] folders = System.IO.Directory.GetDirectories(saveFolder);
            lock (savesLock)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    string name = TrimToNameOnly(folders[i]).ToUpper();
                    saves.Add(new Save(name, true));
                }
                saves.Sort();
            }
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


        public static bool NameAlreadyExists(string aName)
        {
            ThreadAffinity.AssertGameThread();
            lock (savesLock)
            {
                return saves.Find(x => x.Name == aName.ToUpper()) != null;
            }
        }

        public static bool TryGetSaveByName(string name, out Save save)
        {
            ThreadAffinity.AssertGameThread();
            lock (savesLock)
            {
                save = null;
                if (string.IsNullOrWhiteSpace(name)) return false;
                save = saves.Find(x => x.Name == name.ToUpper());
                return save != null;
            }
        }

        public static void CreateNewSave(string aName)
        {
            ThreadAffinity.AssertSimThread();
            aName = aName.ToUpper();
            lock (savesLock)
            {
                saves.Add(new Save(aName, false));
                currentSave = saves.Last();
                saves.Sort();
            }
        }

        static void InitSaveFolder()
        {
            if (System.IO.Directory.Exists(saveFolder)) return;

            System.IO.Directory.CreateDirectory(saveFolder);
        }

        //public static void LoadData(string aName) => LoadData(saves[aName]);

        public static void ContinueLastSave()
        {
            ThreadAffinity.AssertSimThread();
            Save save;
            lock (savesLock)
            {
                save = saves.First();
            }
            LoadData(save);
        }

        public static bool RequestContinueLastSave()
        {
            ThreadAffinity.AssertSimThread();
            Save save;
            lock (savesLock)
            {
                if (saves.Count == 0) return false;
                save = saves.First();
            }
            return RequestLoadData(save);
        }


        public static void LoadData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            currentSave = aSave;
            currentSave.LoadData();
        }

        public static bool RequestLoadData(Save save)
        {
            ThreadAffinity.AssertSimThread();
            if (save == null) return false;
            currentSave = save;
            if (!ThreadingSettings.UseWorkerThreads || !WorkerPool.IsRunning)
            {
                save.LoadData();
                return false;
            }

            WorkerPool.Enqueue(() => SaveLoadPayload.Parse(save), payload =>
            {
                Mailboxes.Main.Publish(new SaveLoadParsed(payload));
            });
            return true;
        }

        public static void ApplyLoadPayload(SaveLoadPayload payload)
        {
            ThreadAffinity.AssertSimThread();
            if (payload == null) return;
            if (!TryGetSaveByName(payload.SaveName, out Save save)) return;
            currentSave = save;

            JsonSerializer serializer = JsonSerializer.Create(serializerSettings);

            if (payload.CameraPosition != null)
            {
                WorldSpace cameraPos = payload.CameraPosition.ToObject<WorldSpace>(serializer);
                Camera.Camera.CentreInWorldSpace = cameraPos;
            }

            List<Chunk> chunks = new List<Chunk>();
            if (payload.TileChunks != null)
            {
                chunks = new List<Chunk>(payload.TileChunks.Count);
                for (int i = 0; i < payload.TileChunks.Count; i++)
                {
                    Chunk chunk = payload.TileChunks[i].ToObject<Chunk>(serializer);
                    if (chunk != null) chunks.Add(chunk);
                }
            }
            TileManager.LoadFromChunks(chunks);

            PlayerData playerData = payload.PlayerData != null ? payload.PlayerData.ToObject<PlayerData>(serializer) : null;
            List<UnitData> guildData = new List<UnitData>();
            if (payload.GuildData != null)
            {
                for (int i = 0; i < payload.GuildData.Count; i++)
                {
                    UnitData unit = payload.GuildData[i].ToObject<UnitData>(serializer);
                    if (unit != null) guildData.Add(unit);
                }
            }

            ObjectFactory.ApplyLoadedData(playerData, guildData);
            ObjectManager.LoadFromFactoryData();

            CorpseManager.LoadFromTokens(payload.Corpses, serializer);
            SpawnerManager.LoadFromTokens(payload.SpawnZones, payload.SavedMobs, serializer);
            TimeManager.Load(currentSave);
        }

        public static void SaveHUD() => Mailboxes.Ui.Publish(new HudSaveRequested());

        public static void SaveData()
        {
            ThreadAffinity.AssertSimThread();
            if (currentSave == null) return;
            if (!ThreadingSettings.UseWorkerThreads || !WorkerPool.IsRunning)
            {
                Mailboxes.Ui.Publish(new SaveDataStarted());
                try
                {
                    currentSave.SaveData();
                }
                finally
                {
                    Mailboxes.Ui.Publish(new SaveDataFinished());
                }
                return;
            }

            Mailboxes.Ui.Publish(new SaveDataStarted());
            SaveWritePayload payload = SaveWritePayload.Capture(currentSave);
            RequestScreenshot(currentSave);
            WorkerPool.Enqueue(() =>
            {
                try
                {
                    payload.Write();
                }
                finally
                {
                    Mailboxes.Ui.Publish(new SaveDataFinished());
                }
            });
        }

        static readonly object screenshotLock = new object();
        static readonly Queue<Save> pendingScreenshots = new Queue<Save>();
        static int pendingScreenshotCount;
        static int pendingScreenshotPeak;
        static long totalScreenshotsEnqueued;
        static long totalScreenshotsProcessed;
        static double lastScreenshotMs;

        public static ScreenshotQueueStats ScreenshotQueueStats => new ScreenshotQueueStats(
            System.Threading.Volatile.Read(ref pendingScreenshotCount),
            System.Threading.Volatile.Read(ref pendingScreenshotPeak),
            System.Threading.Interlocked.Read(ref totalScreenshotsEnqueued),
            System.Threading.Interlocked.Read(ref totalScreenshotsProcessed),
            System.Threading.Volatile.Read(ref lastScreenshotMs));

        public static void RequestScreenshot(Save save)
        {
            ThreadAffinity.AssertSimThread();
            if (save == null) return;
            lock (screenshotLock)
            {
                pendingScreenshots.Enqueue(save);
            }
            int pending = System.Threading.Interlocked.Increment(ref pendingScreenshotCount);
            System.Threading.Interlocked.Increment(ref totalScreenshotsEnqueued);
            int snapshotPeak;
            while (pending > (snapshotPeak = System.Threading.Volatile.Read(ref pendingScreenshotPeak)))
            {
                if (System.Threading.Interlocked.CompareExchange(ref pendingScreenshotPeak, pending, snapshotPeak) == snapshotPeak)
                {
                    break;
                }
            }
        }

        public static void ProcessPendingScreenshots()
        {
            ThreadAffinity.AssertMainThread();
            while (true)
            {
                Save save;
                lock (screenshotLock)
                {
                    if (pendingScreenshots.Count == 0) return;
                    save = pendingScreenshots.Dequeue();
                }
                System.Threading.Interlocked.Decrement(ref pendingScreenshotCount);
                long startTicks = Stopwatch.GetTimestamp();
                save.SaveScreenshot();
                double elapsedMs = (Stopwatch.GetTimestamp() - startTicks) * 1000d / Stopwatch.Frequency;
                System.Threading.Volatile.Write(ref lastScreenshotMs, elapsedMs);
                System.Threading.Interlocked.Increment(ref totalScreenshotsProcessed);
            }
        }

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
            string fileOnly = System.IO.Path.GetFileName(aFile);
            return System.IO.Path.GetFileNameWithoutExtension(fileOnly);
        }
    }

    internal readonly struct ScreenshotQueueStats
    {
        public ScreenshotQueueStats(int pending, int peak, long totalEnqueued, long totalProcessed, double lastScreenshotMs)
        {
            Pending = pending;
            Peak = peak;
            TotalEnqueued = totalEnqueued;
            TotalProcessed = totalProcessed;
            LastScreenshotMs = lastScreenshotMs;
        }

        public int Pending { get; }
        public int Peak { get; }
        public long TotalEnqueued { get; }
        public long TotalProcessed { get; }
        public double LastScreenshotMs { get; }
    }
}
