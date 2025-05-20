using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Projectiles
{
    internal struct ProjectileData
    {
        public string Name => name;
        string name;

        public Point Size { get => size; }
        Point size;

        //public SpellData Spell { get; }
        //SpellData spell; 

        public float MaxSpeed { get => maxSpeed; }
        float maxSpeed;

        public GfxPath GfxPath { get => gfxPath; }
        GfxPath gfxPath;

        [JsonConstructor]
        public ProjectileData(string name, float maxSpeed, int sizeX, int sizeY)
        {
            this.name = name;
            this.maxSpeed = maxSpeed;
            //spell = SpellFactory.GetSpell(name);
            gfxPath = new GfxPath(GfxType.Object, name);
            size = new Point(sizeX, sizeY);



            Assert();
        }

        void Assert()
        {
            Debug.Assert(name != null);
            Debug.Assert(maxSpeed >= 0);

            Debug.Assert(gfxPath != null);
            //Debug.Assert(spell != null);

        }
    }
}
