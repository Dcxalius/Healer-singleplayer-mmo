using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal struct MobData
    {
        public enum Aggresive
        {
            Passive,
            Aggresive
        }

        public string Name => name;
        string name;

        public ClassData ClassData => classData;
        ClassData classData;

        public Relation RelationData => relationData;
        Relation relationData;

        public Level Level => new Level(RandomManager.RollInt(minLevel, maxLevel), 0);
        int minLevel;
        int maxLevel;

        public Equipment Equipment => equipment;
        Equipment equipment;

        public GfxPath GfxPath => gfxPath;
        readonly GfxPath gfxPath;

        public GfxPath CorpseGfxPath => corpseGfxPath;
        readonly GfxPath corpseGfxPath;

        [JsonConstructor]
        public MobData(string name, string corpseGfxName, string className, Aggresive relation, int minLevel, int maxLevel, int?[] equipment)
        {
            this.name = name;
            relationData = new Relation(relation);
            classData = new ClassData(relationData.ToPlayer, className);
            this.maxLevel = maxLevel;
            this.minLevel = minLevel;
            this.equipment = new Equipment(equipment);

            gfxPath = new GfxPath(GfxType.Object, name);

            if (corpseGfxName != null)
            {
                corpseGfxPath = new GfxPath(GfxType.Corpse, corpseGfxName);
            }
            else
            {
                corpseGfxPath = new GfxPath(GfxType.Corpse, "Corpse");
            }
        }
    }
}
