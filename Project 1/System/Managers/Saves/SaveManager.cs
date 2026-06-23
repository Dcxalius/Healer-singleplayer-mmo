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
using System.Threading;
using Path = System.IO.Path;

namespace Project_1.Managers
{
    internal static class SaveManager
    {
        static string contentRootDirectory;
        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto};
        static string saveFolder;

        //TODO: Is there a cleaner way to do these checks?
        public static string Effects => Path.Combine(contentRootDirectory, "Effects");
        public static string Settings => Path.Combine(contentRootDirectory, "Settings");
        public static string HudSettings => Path.Combine(Settings, "Hud.set");
        public static string CameraSettings => Path.Combine(Settings, "Camera.set");
        public static string KeyBindSettings => Path.Combine(Settings, "KeyBind.set");
        public static string DebugSettings => Path.Combine(Settings, "Debug.set");



        public static string DefaultSettings => Path.Combine(Settings, "Default");
        public static string DefaultHudSettings => Path.Combine(DefaultSettings, "Hud.def");
        public static string DefaultCameraSettings => Path.Combine(DefaultSettings, "Camera.def");
        public static string DefaultKeyBindSettings => Path.Combine(DefaultSettings, "KeyBind.def");
        public static string DefaultDebugSettings => Path.Combine(DefaultSettings, "Debug.def");
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
        static int nextLoadRequestId;
        static int currentLoadRequestId;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            contentRootDirectory = Game1.ContentManager.RootDirectory;

            //TODO: 
            InitSaveFolder();

            InitSaves();
        }

        static void InitSaves()
        {
            saves = new List<Save>();
            string[] folders = Directory.GetDirectories(saveFolder);
            lock (savesLock) //Q: What could possible race this during init?
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

        public static Save CreateNewSave(string aName)
        {
            ThreadAffinity.AssertSimThread();
            aName = aName.ToUpper();
            lock (savesLock)
            {
                Save save = new Save(aName, false);
                saves.Add(save);
                currentSave = save;
                saves.Sort();
                return save;
            }
        }

        public static void DeleteSave(Save save)
        {
            ThreadAffinity.AssertSimThread();
            if (save == null) return;

            lock (savesLock)
            {
                saves.Remove(save);
                if (ReferenceEquals(currentSave, save))
                {
                    currentSave = null;
                }
            }

            RemovePendingScreenshots(save);
            save.DeleteFiles();
        }

        public static void SetCurrentSave(Save save)
        {
            ThreadAffinity.AssertSimThread();
            currentSave = save;
        }

        static void InitSaveFolder()
        {
            saveFolder = Path.Combine(contentRootDirectory, "Saves"); //Q: Why is this path not one of the string props?

            //Q: Any deeper verification needed that everything is where it is supposed to?
            if (Directory.Exists(saveFolder)) return;

            Directory.CreateDirectory(saveFolder);
        }

        public static bool RequestContinueLastSave()
        {
            ThreadAffinity.AssertSimThread();
            //TODO: Move this to to main thread else? Prehaps the Continue button/auto call in debug should be done on the main thread by way of events?
            Save save;
            lock (savesLock) //TODO: Ideally no locks should be needed
            {
                if (saves.Count == 0) return false;
                save = saves.First();
            }
            return RequestLoadData(save);
        }

        public static bool RequestLoadData(Save save)
        {
            //TODO: Break up and move this to Main thread. Then sending a package of the loaded save data to the sim thread
            ThreadAffinity.AssertSimThread();
            if (save == null) return false;
            currentSave = save;
            int requestId = Interlocked.Increment(ref nextLoadRequestId);
            Volatile.Write(ref currentLoadRequestId, requestId);
            if (!ThreadingSettings.UseWorkerThreads || !WorkerPool.IsRunning)
            {
                SaveLoadPayload payload = SaveLoadPayload.Parse(save);
                MailboxManager.PublishSimCommand(new SaveLoadParsed(payload, requestId));
                return true;
            }

            WorkerPool.Enqueue(() => SaveLoadPayload.Parse(save), payload =>
            {
                MailboxManager.PublishSimCommand(new SaveLoadParsed(payload, requestId));
            });
            return true;
        }

        public static bool ApplyLoadPayload(SaveLoadPayload payload, int requestId)
        {
            //TODO: Break up
            ThreadAffinity.AssertSimThread();
            if (payload == null) return false;
            if (requestId != Volatile.Read(ref currentLoadRequestId))
            {
                // Stale async load completion. A newer load request has already replaced this token.
                return false;
            }
            if (currentSave != null && !string.Equals(currentSave.Name, payload.SaveName, StringComparison.OrdinalIgnoreCase))
            {
                // Defensive check: payload must still match the actively requested save.
                return false;
            }
            if (!TryGetSaveByName(payload.SaveName, out Save save)) return false;
            currentSave = save;

            JsonSerializer serializer = JsonSerializer.Create(serializerSettings);

            if (payload.CameraPosition != null)
            {
                WorldSpace cameraPos = payload.CameraPosition.ToObject<WorldSpace>(serializer);
                Camera.Camera.CentreInWorldSpace = cameraPos;
            }

            List<Chunk> chunks = new List<Chunk>();
            if (payload.TileChunks != null && payload.TileChunks.Length > 0)
            {
                chunks = new List<Chunk>(payload.TileChunks.Length);
                for (int i = 0; i < payload.TileChunks.Length; i++)
                {
                    Chunk chunk = payload.TileChunks[i].ToObject<Chunk>(serializer);
                    if (chunk != null) chunks.Add(chunk);
                }
            }
            TileManager.LoadFromChunks(chunks);

            PlayerData playerData = payload.PlayerData != null ? payload.PlayerData.ToObject<PlayerData>(serializer) : null;
            List<UnitData> guildData = new List<UnitData>();
            if (payload.GuildData != null && payload.GuildData.Length > 0)
            {
                for (int i = 0; i < payload.GuildData.Length; i++)
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
            return true;
        }

        public static void SaveData()
        {
            //TODO: Break up
            ThreadAffinity.AssertSimThread();
            if (currentSave == null) return;
            if (!ThreadingSettings.UseWorkerThreads || !WorkerPool.IsRunning)
            {
                MailboxManager.PublishUiEvent(new SaveDataStarted("Saving..."));
                try
                {
                    currentSave.SaveData();
                    MailboxManager.PublishUiEvent(new SaveDataFinished("Saved"));
                }
                catch (Exception ex) when (TryBuildSaveFailureMessage(ex, out _)) //Q: Why do we discard the message from a TryBuild just to call the same function from NotifySaveFailed? Feels like something is way off
                {
                    //bool knownError = TryBuildSaveFailureMessage(ex, out string msg);
                    //if (!knownError)
                    //{
                    //    msg = "Failed to save.";
                    //    NotifySaveFailed(msg);
                    //    throw;
                    //} Q: Isn't this cleaner?


                    
                    NotifySaveFailed(ex);
                }
                return;
            }

            MailboxManager.PublishUiEvent(new SaveDataStarted("Saving..."));
            SaveWritePayload payload = SaveWritePayload.Capture(currentSave);
            RequestScreenshot(currentSave);
            WorkerPool.Enqueue(() =>
            {
                try
                {
                    payload.Write();
                    MailboxManager.PublishUiEvent(new SaveDataFinished("Saved"));
                }
                catch (Exception ex) when (TryBuildSaveFailureMessage(ex, out _)) //Q: Why do we discard the message from a TryBuild just to call the same function from NotifySaveFailed? Feels like something is way off
                {
                    NotifySaveFailed(ex);
                }
            });
        }

        //TODO: Break up savemanager into multiple files and put it on the top of that file or create a seperate screenshot manager.
        static readonly object screenshotLock = new object();
        static readonly Queue<Save> pendingScreenshots = new Queue<Save>();
        static int pendingScreenshotCount;
        static int pendingScreenshotPeak;
        static long totalScreenshotsEnqueued;
        static long totalScreenshotsProcessed;
        static double lastScreenshotMs;

        public static ScreenshotQueueStats ScreenshotQueueStats => new ScreenshotQueueStats(
            Volatile.Read(ref pendingScreenshotCount),
            Volatile.Read(ref pendingScreenshotPeak),
            Interlocked.Read(ref totalScreenshotsEnqueued),
            Interlocked.Read(ref totalScreenshotsProcessed),
            Volatile.Read(ref lastScreenshotMs));
        public static void RequestScreenshot(Save save) 
        {
            ThreadAffinity.AssertSimThread();
            if (save == null) return;
            lock (screenshotLock)
            {
                pendingScreenshots.Enqueue(save);
            }
            int pending = Interlocked.Increment(ref pendingScreenshotCount);
            Interlocked.Increment(ref totalScreenshotsEnqueued);
            int snapshotPeak;
            while (pending > (snapshotPeak = Volatile.Read(ref pendingScreenshotPeak)))
            {
                if (Interlocked.CompareExchange(ref pendingScreenshotPeak, pending, snapshotPeak) == snapshotPeak)
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
                Interlocked.Decrement(ref pendingScreenshotCount);
                if (save == null) continue;
                try
                {
                    long startTicks = Stopwatch.GetTimestamp();
                    save.SaveScreenshot();
                    double elapsedMs = (Stopwatch.GetTimestamp() - startTicks) * 1000d / Stopwatch.Frequency;
                    Volatile.Write(ref lastScreenshotMs, elapsedMs);
                    Interlocked.Increment(ref totalScreenshotsProcessed);
                }
                catch (Exception ex) when (TryBuildSaveFailureMessage(ex, out _))
                {
                    //Q: Do we really want to mark it as failed if just the screenshot fails? Perhaps a different fail message and just using a default image sounds cleaner
                    NotifySaveFailed(ex);
                }
            }
        }

        static void RemovePendingScreenshots(Save save)
        {
            //Q: I assume we do this so if a save is saved, but before the screenshot is captured, deleted. Is this possible though? Even with tas?
            //Not saying we shouldn't do this, just something to keep in mind and check.
            lock (screenshotLock)
            {
                if (pendingScreenshots.Count == 0) return;

                Queue<Save> remainingScreenshots = new Queue<Save>(pendingScreenshots.Count);
                int removedCount = 0;
                while (pendingScreenshots.Count > 0)
                {
                    Save pendingSave = pendingScreenshots.Dequeue();
                    if (ReferenceEquals(pendingSave, save))
                    {
                        removedCount++;
                        continue;
                    }

                    remainingScreenshots.Enqueue(pendingSave);
                }

                while (remainingScreenshots.Count > 0)
                {
                    pendingScreenshots.Enqueue(remainingScreenshots.Dequeue());
                }

                if (removedCount > 0)
                {
                    Interlocked.Add(ref pendingScreenshotCount, -removedCount);
                }
            }
        }

        public static void ExportData(string aDestination, object aObjectToExport)
        {
            //Q: Move to a JSON Manager?
            string json = JsonConvert.SerializeObject(aObjectToExport, serializerSettings);
            File.WriteAllText(aDestination, json);
        }

        internal static bool TryBuildSaveFailureMessage(Exception exception, out string message)
        {
            Exception baseException = exception?.GetBaseException();
            if (baseException is IOException ioException)
            {
                int errorCode = ioException.HResult & 0xFFFF;
                if (errorCode == 0x70 || errorCode == 0x27)
                {
                    message = "Save failed: the drive is out of free space.";
                    return true;
                }

                message = "Save failed: the save files could not be written.";
                return true;
            }

            if (baseException is UnauthorizedAccessException)
            {
                message = "Save failed: the game does not have permission to write save files.";
                return true;
            }

            message = null;
            return false;
        }

        internal static void NotifySaveFailed(Exception exception)
        {
            string message;
            if (!TryBuildSaveFailureMessage(exception, out message))
            {
                message = "Save failed.";
            }

            MailboxManager.PublishUiEvent(new SaveDataFailed(message));
            MailboxManager.PublishUiEvent(new ChatMessagePosted(ChatMessageType.System, message));
        }

        public static T ImportData<T>(string aJsonString)
        {
            //Q: Move to a JSON Manager?
            //TODO: Figure out a way to get cleaner crash data when this fails.
            
            //Something like this should be done
            //Debug.Assert(aJsonString != File);
            return JsonConvert.DeserializeObject<T>(aJsonString, serializerSettings);
        }

        public static string TrimToNameOnly(string aFile)
        {
            //Q: Move to a JSON Manager?
            string fileOnly = Path.GetFileName(aFile);
            return Path.GetFileNameWithoutExtension(fileOnly);
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
