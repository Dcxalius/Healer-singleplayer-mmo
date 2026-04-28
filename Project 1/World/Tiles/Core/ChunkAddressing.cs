using Microsoft.Xna.Framework;
using System;

namespace Project_1.Tiles
{
    internal static class ChunkAddressing
    {
        public static int GetChunkId(Point position) => GetChunkId(position.X, position.Y);

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

        public static Point GetChunkPosition(int id)
        {
            int circle = (int)Math.Ceiling((Math.Sqrt(id + 1) - 1) / 2);
            int highestNrInCircle = HighestNrInCircle(circle);
            int dif = highestNrInCircle - id;
            if (dif == 0)
            {
                return new Point(circle, -circle);
            }

            int sideLength = circle * 2;
            if (dif % sideLength == 0)
            {
                if (dif / sideLength == 1) return new Point(circle, circle);
                if (dif / sideLength == 2) return new Point(-circle, circle);
                if (dif / sideLength == 3) return new Point(-circle, -circle);
                throw new Exception("ohno");
            }

            Point returnPoint = new Point();

            if ((float)dif / sideLength < 1f)
            {
                returnPoint.X = circle;
                returnPoint.Y = -circle + dif;
            }
            else if ((float)dif / sideLength < 2f)
            {
                returnPoint.X = circle - (dif - sideLength);
                returnPoint.Y = circle;
            }
            else if ((float)dif / sideLength < 3f)
            {
                returnPoint.X = -circle;
                returnPoint.Y = circle - (dif - sideLength * 2);
            }
            else if ((float)dif / sideLength < 4f)
            {
                returnPoint.X = -circle + (dif - sideLength * 3);
                returnPoint.Y = -circle;
            }
            else
            {
                throw new Exception("ohno");
            }

            return returnPoint;
        }

        static int HighestNrInCircle(int circleSize) => 4 * (((circleSize + 1) * (circleSize + 1)) - (circleSize + 1));
    }
}
