using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Camera
{
    internal class CameraSettings
    {
        public enum Follow
        {
            Free,
            CircleSoftBound,
            RectangleSoftBound,
            Hardbound
        }

        public enum WindowType
        {
            Fullscreen,
            Borderless,
            Windowed
        }

        public Follow FollowSetting
        {
            get => follow;
            set => follow = value;
        }

        Follow follow;

        public Point WindowSize
        {
            get => windowSize;
            set
            {
                windowSize = value;
                GraphicsManager.SetWindowSize(windowSize, fullscreen);
            }
        }
        Point windowSize;

        public WindowType Fullscreen
        {
            get => fullscreen;
            set
            {
                fullscreen = value;
                GraphicsManager.SetWindowSize(windowSize, fullscreen);
            }
        }
        WindowType fullscreen;

        [JsonConstructor]
        public CameraSettings(Follow followSetting, Point windowSize, WindowType fullscreen)
        {
            follow = followSetting;
            this.windowSize = windowSize;
            this.fullscreen = fullscreen;
        }

        public void SetCamera()
        {
            GraphicsManager.SetWindowSize(windowSize, fullscreen);
        }
    }
}
