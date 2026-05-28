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
        public static double MilisecondSinceLastFrame => instanceTime.ElapsedGameTime.TotalMilliseconds;
        public static double SecondsSinceLastFrame => instanceTime.ElapsedGameTime.TotalSeconds;

        /// <summary>
        /// Returns the time since the game was launched
        /// </summary>
        public static double InstanceTotalFrameTime => instanceTime.TotalGameTime.TotalMilliseconds;
        public static TimeSpan InstanceTotalFrameTimeAsTimeSpan => instanceTime.TotalGameTime;

        /// <summary>
        /// Returns the time since the player created the character/world
        /// </summary>
        public static double TotalFrameTime => playTime.TotalGameTime.TotalMilliseconds;

        public static TimeSpan TotalFrameTimeAsTimeSpan => playTime.TotalGameTime;
        
        public static bool Paused => !pausers.IsEmpty; //Q: Should this be public?


        static GameTime instanceTime;
        static GameTime playTime;
        static ConcurrentDictionary<object, int> pausers; //Object is what called the pause, int is how many times that object has called pause without calling unpause
        static bool initialized;

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
            if (Paused) return;
            playTime.TotalGameTime += aGameTime.ElapsedGameTime;
        }

        /// <summary>
        /// The pausing object must call StopPause the same amount of times it called StartPause to actually unpause the game.
        /// aPauser should probably always be a this.
        /// </summary>
        /// <param name="aPauser"></param>
        public static void StartPause(Object aPauser)
        {
            ThreadAffinity.AssertGameThread();
            Debug.Assert(aPauser != null, "Pauser cannot be null");
            pausers.AddOrUpdate(aPauser, 1, (_, count) => count + 1);
        }

        /// <summary>
        /// The pausing object must call this method the same amount of times it called StartPause to actually unpause the game.
        /// aUnpauser should probably always be a this.
        /// </summary>
        /// <param name="aUnpauser"></param>

        public static void StopPause(Object aUnpauser)
        {
            ThreadAffinity.AssertGameThread();
            Debug.Assert(aUnpauser != null, "Pauser cannot be null");
            while (true)
            {
                if (!pausers.TryGetValue(aUnpauser, out int count)) return;
                if (count <= 1)
                {
                    if (pausers.TryRemove(aUnpauser, out _)) return;
                }
                else
                {
                    if (pausers.TryUpdate(aUnpauser, count - 1, count)) return;
                }
            }
        }

        //TODO: Save the time data
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
