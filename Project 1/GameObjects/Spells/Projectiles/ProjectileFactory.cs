﻿using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Project_1.GameObjects.Spells.Projectiles
{
    internal static class ProjectileFactory
    {
        static Dictionary<string, ProjectileData> projectiles;

        public static void Init(ContentManager aContentManager)
        {
            projectiles = new Dictionary<string, ProjectileData>();
            string path = aContentManager.RootDirectory + "\\Data\\Projectiles\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                ProjectileData data = JsonConvert.DeserializeObject<ProjectileData>(rawData);
                projectiles.Add(data.Name, data);
            }
        }

        public static Projectile CreateProjectile(WorldSpace aStartPosition, Spell aSpell, Entity aTarget)
        {
            return new Projectile(aStartPosition, projectiles[aSpell.Name], aSpell, aTarget);
        }
    }
}