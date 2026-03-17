using Microsoft.Xna.Framework;
using Project_1.Managers.Saves;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    internal static class TimeManager
    {
        static GameTime instanceTime;
        static GameTime playTime;
        static ConcurrentDictionary<object, int> pausers;
        static bool initialized;

        public static double MilisecondSinceLastFrame => instanceTime.ElapsedGameTime.TotalMilliseconds;
        public static double SecondsSinceLastFrame => instanceTime.ElapsedGameTime.TotalSeconds;

        public static double InstanceTotalFrameTime => instanceTime.TotalGameTime.TotalMilliseconds;
        public static TimeSpan InstanceTotalFrameTimeAsTimeSpan => instanceTime.TotalGameTime;

        public static double TotalFrameTime => playTime.TotalGameTime.TotalMilliseconds;

        public static TimeSpan TotalFrameTimeAsTimeSpan => playTime.TotalGameTime;
        
        public static bool Paused => !pausers.IsEmpty;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            playTime = new GameTime();
            pausers = new ConcurrentDictionary<object, int>();
        }

        public static void Update(GameTime aGameTime)
        {
            ThreadAffinity.AssertMainThread();
            instanceTime = aGameTime;
            if (!Paused)
            {
                playTime.TotalGameTime += aGameTime.ElapsedGameTime;
            }
        }

        public static void StartPause(Object aPauser)
        {
            ThreadAffinity.AssertGameThread();
            if (aPauser == null) return;
            pausers.AddOrUpdate(aPauser, 1, (_, count) => count + 1);
        }

        public static void StopPause(Object aPauser)
        {
            ThreadAffinity.AssertGameThread();
            if (aPauser == null) return;
            while (true)
            {
                if (!pausers.TryGetValue(aPauser, out int count)) return;
                if (count <= 1)
                {
                    if (pausers.TryRemove(aPauser, out _)) return;
                }
                else
                {
                    if (pausers.TryUpdate(aPauser, count - 1, count)) return;
                }
            }
        }

        //public static void Save(Save aSave)
        //{
        //    SaveManager.ExportData()
        //}

        public static void Load(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            playTime.TotalGameTime = aSave.SaveDetails.TimeInSave;
        }
    }
}
