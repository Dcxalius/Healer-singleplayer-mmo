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

        public enum Fullscreen
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

        public Fullscreen xddFullscreen //TODO: Fix name
        {
            get => fullscreen;
            set
            {
                fullscreen = value;
                GraphicsManager.SetWindowSize(windowSize, fullscreen);
            }
        }
        Fullscreen fullscreen;

        [JsonConstructor]
        public CameraSettings(Follow followSetting, Point windowSize, Fullscreen xddFullscreen)
        {
            follow = followSetting;
            this.windowSize = windowSize;
            this.fullscreen = xddFullscreen;
        }

        public void SetCamera()
        {
            GraphicsManager.SetWindowSize(windowSize, fullscreen);
        }
    }
}
