using Microsoft.Xna.Framework;
using Project_1.Managers;
using System;
using System.Collections.Generic;

namespace Project_1.Textures
{
    internal static class TextureCatalog
    {
        //Q: Unsure what the purpose of this is, since the things in it are dupes of whats in texturemanager?
        //Should this be where we store all texture info, including the textures themselves once they are loaded?
        static Dictionary<string, Point>[] sizes;
        static Dictionary<string, Color>[] avgColors;
        static bool initialized;

        public static void Init(Dictionary<string, Point>[] textureSizes, Dictionary<string, Color>[] textureAvgColors)
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            sizes = textureSizes ?? throw new ArgumentNullException(nameof(textureSizes));
            avgColors = textureAvgColors ?? throw new ArgumentNullException(nameof(textureAvgColors));
            initialized = true;
        }

        public static Point GetSize(GfxPath path)
        {
            if (path == null || path.Name == null) return Point.Zero;
            if (sizes == null) return Point.Zero;

            int typeIndex = (int)path.Type;
            if (typeIndex < 0 || typeIndex >= sizes.Length) return Point.Zero;

            var dict = sizes[typeIndex];
            if (dict != null && dict.TryGetValue(path.Name, out var size))
            {
                return size;
            }

            var debugDict = sizes[(int)GfxType.Debug];
            if (debugDict != null && debugDict.TryGetValue("MissingTexture", out size))
            {
                return size;
            }

            return Point.Zero;
        }

        public static Color GetAvgColor(GfxPath path)
        {
            if (path == null || path.Name == null) return Color.White;
            if (avgColors == null) return Color.White;

            int typeIndex = (int)path.Type;
            if (typeIndex >= 0 && typeIndex < avgColors.Length)
            {
                var dict = avgColors[typeIndex];
                if (dict != null && dict.TryGetValue(path.Name, out var color))
                {
                    return color;
                }
            }

            var debugDict = avgColors[(int)GfxType.Debug];
            if (debugDict != null && debugDict.TryGetValue("MissingTexture", out var fallback))
            {
                return fallback;
            }

            return Color.White;
        }
    }
}
