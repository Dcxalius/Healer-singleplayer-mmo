using Microsoft.Xna.Framework;
using System;

namespace Project_1.Tiles
{
    internal static class ChunkAddressing
    {

        /// <inheritdoc cref="GetChunkId(int, int)"/>
        public static int GetChunkId(Point position) => GetChunkId(position.X, position.Y);

        /// <summary>
        /// Gets a unique chunk ID for a given chunk position. The chunk at (0,0) has ID 0, and the IDs spiral outwards from there in a anti-clockwise direction with id 1 being the chunk above at (0, -1).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int GetChunkId(int x, int y)
        {
            if (x == 0 && y == 0) return 0;

            int dirInt;
            int furthestDir;
            int shortestDir;

            if (Math.Abs(x) >= Math.Abs(y))
            {
                if (x < 0)
                {
                    dirInt = 3;
                    shortestDir = y;
                }
                else
                {
                    dirInt = 7;
                    shortestDir = -y;
                }

                furthestDir = x;
            }
            else
            {
                if (y < 0)
                {
                    dirInt = 1;
                    shortestDir = -x;
                }
                else
                {
                    dirInt = 5;
                    shortestDir = x;
                }

                furthestDir = y;
            }

            return dirInt * Math.Abs(furthestDir) + HighestNrInCircle(Math.Abs(furthestDir) - 1) + shortestDir;
        }

        /// <summary>
        /// Gets the chunk position for a given chunk ID. The chunk at (0,0) has ID 0, and the IDs spiral outwards from there in a anti-clockwise direction with id 1 being the chunk above at (0, -1).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Point GetChunkPosition(int id)
        {
            int circle = (int)Math.Ceiling((Math.Sqrt(id + 1) - 1) / 2);
            int highestNrInCircle = HighestNrInCircle(circle);
            int dif = highestNrInCircle - id;
            if (dif == 0)
            {
                return new Point(circle, -circle); //Top right corner
            }

            int sideLength = circle * 2;
            if (dif % sideLength == 0)
            {
                if (dif / sideLength == 1) return new Point(circle, circle); //Bottom right corner
                if (dif / sideLength == 2) return new Point(-circle, circle); //Bottom left corner
                if (dif / sideLength == 3) return new Point(-circle, -circle); //Top left corner
            }

            Point returnPoint = new Point();

            if ((float)dif / sideLength < 1f) return new Point(circle, -circle + dif); //Right side
            else if ((float)dif / sideLength < 2f) return new Point(circle - (dif - sideLength), circle); //Bottom side
            else if ((float)dif / sideLength < 3f) return new Point(-circle, circle - (dif - sideLength * 2)); //Left side
            else if ((float)dif / sideLength < 4f) return new Point(-circle + (dif - sideLength * 3), -circle); //Top side

            return returnPoint;
        }

        static int HighestNrInCircle(int circleSize) => 4 * (((circleSize + 1) * (circleSize + 1)) - (circleSize + 1));
    }
}
