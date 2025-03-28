using Microsoft.Xna.Framework;
using Project_1.Managers.Saves;
using System;
using System.Collections.Generic;
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
        static TimeSpan? timePaused;
        static List<Object> pausers;

        public static double SecondsSinceLastFrame => instanceTime.ElapsedGameTime.TotalSeconds;

        public static double InstanceTotalFrameTime => instanceTime.TotalGameTime.TotalMilliseconds;
        public static TimeSpan InstanceTotalFrameTimeAsTimeSpan => instanceTime.TotalGameTime;

        public static double TotalFrameTime => playTime.TotalGameTime.TotalMilliseconds;

        public static TimeSpan TotalFrameTimeAsTimeSpan => playTime.TotalGameTime;
        
        public static bool Paused => pausers.Count > 0;

        public static void Init()
        {
            playTime = new GameTime();
            pausers = new List<object>();
        }

        public static void Update(GameTime aGameTime)
        {
            instanceTime = aGameTime;
            if (!Paused)
            {
                playTime.TotalGameTime += aGameTime.ElapsedGameTime;
            }
        }

        public static void StartPause(Object aPauser) => pausers.Add(aPauser);

        public static void StopPause(Object aPauser) => pausers.Remove(aPauser);

        //public static void Save(Save aSave)
        //{
        //    SaveManager.ExportData()
        //}

        public static void Load(Save aSave)
        {
            playTime.TotalGameTime = aSave.SaveDetails.TimeInSave;
        }
    }
}
