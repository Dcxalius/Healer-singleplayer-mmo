using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal abstract class SpawnGeometry
    {
        public virtual WorldSpace GetNewSpawnPosition { get; }
        public abstract WorldSpace Centre { get; }
        public abstract float Length { get; }
        public SpawnGeometry() { }

        static public SpawnGeometry CreateGeometry(WorldSpace[] aWorldSpaces) //TODO: Honestly wtf xdd
        {
            if (aWorldSpaces.Length == 1)
            {
                return new SpawnPoint(aWorldSpaces[0]);
            }

            if (aWorldSpaces.Length == 2)
            {
                return new SpawnRectangle(aWorldSpaces[0], aWorldSpaces[1]);
            }



            throw new NotImplementedException();
            return null;
        }
    }
}
