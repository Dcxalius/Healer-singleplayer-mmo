using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Unit;
using Project_1.Textures;
using System;
using System.Collections.Generic;

namespace Project_1.System.Models.BaseModels
{
    internal static class EntityModelCatalog
    {
        // Specific art sets can opt into custom shells here without changing the generic fallback sizing rules.
        static readonly Dictionary<string, EntityModelDefinition> definitionsByTextureKey = new Dictionary<string, EntityModelDefinition>(StringComparer.OrdinalIgnoreCase);
        static readonly EntityModelDefinition mediumUnitDefinition = new EntityModelDefinition("unit/default/medium", UnitModel.Type.Medium);
        static readonly EntityModelDefinition largeUnitDefinition = new EntityModelDefinition("unit/default/large", UnitModel.Type.Large);
        static readonly EntityModelDefinition eliteUnitDefinition = new EntityModelDefinition("unit/default/elite", UnitModel.Type.Elite);
        static readonly EntityModelDefinition bossUnitDefinition = new EntityModelDefinition("unit/default/boss", UnitModel.Type.Boss);

        public static void RegisterForTexture(GfxPath aTexturePath, EntityModelDefinition aDefinition)
        {
            if (aDefinition == null) throw new ArgumentNullException(nameof(aDefinition));

            string textureKey = BuildTextureKey(aTexturePath);
            if (textureKey == null) throw new ArgumentException("Texture path must contain a name.", nameof(aTexturePath));
            definitionsByTextureKey[textureKey] = aDefinition;
        }

        public static EntityModelDefinition Resolve(Entity aEntity)
        {
            if (aEntity == null) throw new ArgumentNullException(nameof(aEntity));

            string textureKey = BuildTextureKey(aEntity.PresentationTexturePath);
            if (textureKey != null && definitionsByTextureKey.TryGetValue(textureKey, out EntityModelDefinition overrideDefinition))
            {
                return overrideDefinition;
            }

            return ResolveDefault(aEntity);
        }

        static EntityModelDefinition ResolveDefault(Entity aEntity)
        {
            if (aEntity is Player || aEntity is GuildMember)
            {
                return largeUnitDefinition;
            }

            return aEntity.UnitType switch
            {
                UnitType.Player => largeUnitDefinition,
                UnitType.Elite => eliteUnitDefinition,
                UnitType.Boss => bossUnitDefinition,
                _ => mediumUnitDefinition
            };
        }

        static string BuildTextureKey(GfxPath aTexturePath)
        {
            if (aTexturePath == null || string.IsNullOrWhiteSpace(aTexturePath.Name)) return null;
            return aTexturePath.ToString();
        }
    }
}
