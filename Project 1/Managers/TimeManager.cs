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
        static GameTime gameTime;
        static TimeSpan? timePaused;
        public static double SecondsSinceLastFrame
        {
            get => gameTime.ElapsedGameTime.TotalSeconds;
        }

        
        public static double TotalFrameTime
        {
            get
            {
                if (!timePaused.HasValue)
                {
                    return gameTime.TotalGameTime.TotalMilliseconds;
                }
                return timePaused.Value.TotalMilliseconds;
            }

        }

        public static TimeSpan TotalFrameTimeAsTimeSpan
        {
            get
            {
                if (!timePaused.HasValue)
                {
                    return gameTime.TotalGameTime;
                }
                return timePaused.Value;
            }
        }
        
        public static void Update(GameTime aGameTime)
        {
            gameTime = aGameTime;
        }

        public static void StartPause()
        {
            Debug.Assert(!timePaused.HasValue, "Tried to pause paused game.");
            timePaused = gameTime.TotalGameTime;
        }

        public static void StopPause()
        {
            Debug.Assert(timePaused.HasValue, "Tried to unpause unpaused game.");
            gameTime.TotalGameTime = timePaused.Value;
            timePaused = null;
        }
    }
}
