using Microsoft.Xna.Framework;
using System;

namespace Project_1.Tiles
{
    internal static class PerlinNoiseGenerator
    {
        static readonly Vector2[] gradients = new Vector2[]
        {
            new Vector2(1f, 0f),
            new Vector2(-1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f),
            new Vector2(0.70710677f, 0.70710677f),
            new Vector2(-0.70710677f, 0.70710677f),
            new Vector2(0.70710677f, -0.70710677f),
            new Vector2(-0.70710677f, -0.70710677f)
        };

        public static float Fractal01(int x, int y, int seed, float scale, int octaves, float persistence, float lacunarity)
        {
            if (scale <= 0f) throw new ArgumentOutOfRangeException(nameof(scale));
            if (octaves <= 0) throw new ArgumentOutOfRangeException(nameof(octaves));
            if (persistence <= 0f || persistence > 1f) throw new ArgumentOutOfRangeException(nameof(persistence));
            if (lacunarity < 1f) throw new ArgumentOutOfRangeException(nameof(lacunarity));

            float sum = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float maxAmplitude = 0f;

            for (int octave = 0; octave < octaves; octave++)
            {
                float sampleX = x * frequency / scale;
                float sampleY = y * frequency / scale;
                float noise = Sample(sampleX, sampleY, seed + octave * 1013);
                sum += noise * amplitude;
                maxAmplitude += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            if (maxAmplitude <= 0f) return 0.5f;

            float normalized = sum / maxAmplitude;
            return MathHelper.Clamp((normalized + 1f) * 0.5f, 0f, 1f);
        }

        static float Sample(float x, float y, int seed)
        {
            int x0 = (int)MathF.Floor(x);
            int y0 = (int)MathF.Floor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float dx = x - x0;
            float dy = y - y0;

            float g00 = DotGradient(x0, y0, dx, dy, seed);
            float g10 = DotGradient(x1, y0, dx - 1f, dy, seed);
            float g01 = DotGradient(x0, y1, dx, dy - 1f, seed);
            float g11 = DotGradient(x1, y1, dx - 1f, dy - 1f, seed);

            float u = Fade(dx);
            float v = Fade(dy);

            float nx0 = MathHelper.Lerp(g00, g10, u);
            float nx1 = MathHelper.Lerp(g01, g11, u);
            return MathHelper.Lerp(nx0, nx1, v);
        }

        static float DotGradient(int gridX, int gridY, float dx, float dy, int seed)
        {
            int hash = Hash(gridX, gridY, seed);
            Vector2 gradient = gradients[hash & 7];
            return (gradient.X * dx) + (gradient.Y * dy);
        }

        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int hash = seed;
                hash ^= x * 374761393;
                hash = (hash << 13) ^ hash;
                hash ^= y * 668265263;
                hash = (hash << 17) ^ hash;
                hash *= 1274126177;
                return hash;
            }
        }
    }
}
