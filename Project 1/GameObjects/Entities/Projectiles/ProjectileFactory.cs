using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Project_1.GameObjects.Entities.Projectiles
{
    internal static class ProjectileFactory
    {
        static Dictionary<string, ProjectileData> projectiles;

        static ProjectileFactory()
        {
            projectiles = new Dictionary<string, ProjectileData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\Projectiles\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                ProjectileData data = JsonConvert.DeserializeObject<ProjectileData>(rawData);
                projectiles.Add(data.Name, data);
            }
        }

        public static Projectile CreateProjectile(Entity aCaster, WorldSpace aStartPosition, Spell aSpell, Entity aTarget)
        {
            return new Projectile(aCaster, aStartPosition, projectiles[aSpell.Name], aSpell, aTarget);
        }
    }
}
