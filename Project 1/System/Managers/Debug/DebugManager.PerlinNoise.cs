using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Project_1.Managers
{
    internal static partial class DebugManager
    {
        readonly struct PerlinNoiseExportRequest
        {
            public PerlinNoiseExportRequest(string fileName, Point size, Color[] pixels)
            {
                FileName = fileName;
                Size = size;
                Pixels = pixels;
            }

            public string FileName { get; }
            public Point Size { get; }
            public Color[] Pixels { get; }
        }

        static readonly ConcurrentQueue<PerlinNoiseExportRequest> pendingPerlinNoiseExports = new ConcurrentQueue<PerlinNoiseExportRequest>();
        static int perlinNoiseExportSequence;

        public static bool ShouldDumpPerlinNoisePngs => Mode(DebugMode.DumpPerlinNoisePngs);

        public static void QueuePerlinHeightmapExport(int chunkId, Point chunkPosition, float[,] height01Samples)
        {
            if (!ShouldDumpPerlinNoisePngs) return;
            if (height01Samples == null) return;

            int width = height01Samples.GetLength(0);
            int height = height01Samples.GetLength(1);
            if (width <= 0 || height <= 0) return;

            Color[] pixels = new Color[width * height];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte value = (byte)Math.Clamp((int)MathF.Round(MathHelper.Clamp(height01Samples[x, y], 0f, 1f) * 255f), 0, 255);
                    pixels[index++] = new Color(value, value, value, (byte)255);
                }
            }

            int exportId = Interlocked.Increment(ref perlinNoiseExportSequence);
            string fileName = $"chunk_{chunkId}_x{chunkPosition.X}_y{chunkPosition.Y}_{exportId:D6}.png";
            pendingPerlinNoiseExports.Enqueue(new PerlinNoiseExportRequest(fileName, new Point(width, height), pixels));
        }

        public static void ProcessPendingPerlinNoiseExports()
        {
            ThreadAffinity.AssertMainThread();

            while (pendingPerlinNoiseExports.TryDequeue(out PerlinNoiseExportRequest request))
            {
                try
                {
                    string exportDirectory = Path.Combine(Game1.ContentManager.RootDirectory, "Debug", "PerlinHeightmaps");
                    Directory.CreateDirectory(exportDirectory);
                    string filePath = Path.Combine(exportDirectory, request.FileName);

                    using Texture2D texture = GraphicsManager.CreateNewTexture(request.Size);
                    texture.SetData(request.Pixels);

                    using Stream stream = File.Create(filePath);
                    texture.SaveAsPng(stream, request.Size.X, request.Size.Y);
                }
                catch (Exception ex)
                {
                    Print($"Failed to export Perlin heightmap PNG: {ex.Message}");
                }
            }
        }
    }
}
