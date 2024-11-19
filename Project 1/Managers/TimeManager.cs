using Microsoft.Xna.Framework;
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
        static GameTime gt;
        static TimeSpan? timePaused;
        public static double SecondsSinceLastFrame
        {
            get => gt.ElapsedGameTime.TotalSeconds;
        }
        
        public static double TotalFrameTime
        {
            get
            {
                if (!timePaused.HasValue)
                {
                    return gt.TotalGameTime.TotalMilliseconds;
                }
                return timePaused.Value.TotalMilliseconds;
            }

        }
        
        public static void Update(GameTime aGameTime)
        {
            gt = aGameTime;
        }

        public static void StartPause()
        {
            Debug.Assert(!timePaused.HasValue, "Tried to pause paused game.");
            timePaused = gt.TotalGameTime;
        }

        public static void StopPause()
        {
            Debug.Assert(timePaused.HasValue, "Tried to unpause unpaused game.");
            gt.TotalGameTime = timePaused.Value;
            timePaused = null;
        }
    }
}
