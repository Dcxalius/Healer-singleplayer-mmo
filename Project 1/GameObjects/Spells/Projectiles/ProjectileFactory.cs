using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
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

        public static Projectile CreateProjectile(Spell aSpell)
        {
            return new Projectile(aSpell.Owner.Centre, projectiles[aSpell.Name], aSpell, aSpell.Owner.Target);
        }
    }
}
