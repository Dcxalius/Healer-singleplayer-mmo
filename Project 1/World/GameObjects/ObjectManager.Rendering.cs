using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.System.Models.BaseModels;
using Project_1.Tiles;
using Project_1.UI.HUD.Managers;

namespace Project_1.GameObjects
{
    internal static partial class ObjectManager
    {
        internal static void DrawMinimapSnapshots(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aMinimapSize)
        {
            ThreadAffinity.AssertMainThread();
            ApplyRenderUpdates();
            foreach (EntityRenderSnapshot snapshot in renderPlayers.Values)
            {
                snapshot.DrawMinimap(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
            foreach (EntityRenderSnapshot snapshot in renderEntities.Values)
            {
                snapshot.DrawMinimap(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
            foreach (EntityRenderSnapshot snapshot in renderNpcs.Values)
            {
                snapshot.DrawMinimap(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
        }

        internal static void DrawSnapshots(SpriteBatch aSpriteBatch)
        {
            ThreadAffinity.AssertMainThread();
            ApplyRenderUpdates();
            foreach (EntityRenderSnapshot snapshot in renderPlayers.Values)
            {
                snapshot.Draw(aSpriteBatch);
            }
            foreach (EntityRenderSnapshot snapshot in renderEntities.Values)
            {
                snapshot.Draw(aSpriteBatch);
            }
            foreach (EntityRenderSnapshot snapshot in renderNpcs.Values)
            {
                snapshot.Draw(aSpriteBatch);
            }
        }

        internal static void DrawModelSnapshots()
        {
            ThreadAffinity.AssertMainThread();
            renderModelEntities.ApplyUpdates();
            EntityModelRenderer.DrawSnapshots(renderModelEntities.Values);
        }

        public static LightSnapshot RenderLightSnapshot => renderLightSnapshot;

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            PublishPlayerSnapshot();
            PublishEntitySnapshots(entities, renderEntities, knownEntityIds, currentEntityIds);
            PublishEntitySnapshots(npcs, renderNpcs, knownNpcIds, currentNpcIds);
            PublishModelSnapshots();
        }

        static void ApplyRenderUpdates()
        {
            renderPlayers.ApplyUpdates();
            renderEntities.ApplyUpdates();
            renderNpcs.ApplyUpdates();
        }

        static void PublishPlayerSnapshot()
        {
            currentPlayerIds.Clear();
            if (player != null)
            {
                EntityRenderSnapshot snapshot = player.BuildRenderSnapshot();
                renderPlayers.EnqueueUpdate(snapshot);
                currentPlayerIds.Add(snapshot.RenderId);
                renderLightSnapshot = BuildRenderLightSnapshot();
            }
            else
            {
                renderLightSnapshot = LightSnapshot.Empty;
            }

            PublishRemovals(renderPlayers, knownPlayerIds, currentPlayerIds);
        }

        static void PublishModelSnapshots()
        {
            if (!DebugManager.Mode(DebugMode.ModelPreview))
            {
                renderModelEntities.RequestClear();
                knownModelIds.Clear();
                currentModelIds.Clear();
                return;
            }

            currentModelIds.Clear();
            List<Entity> all = BuildAllScratch();
            for (int i = 0; i < all.Count; i++)
            {
                Entity entity = all[i];
                if (entity == null) continue;
                EntityModelRenderSnapshot snapshot = entity.BuildModelRenderSnapshot();
                renderModelEntities.EnqueueUpdate(snapshot);
                currentModelIds.Add(snapshot.RenderId);
            }

            PublishRemovals(renderModelEntities, knownModelIds, currentModelIds);
        }

        static LightSnapshot BuildRenderLightSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (player == null) return LightSnapshot.Empty;

            int count = 0;
            float nearbyRangeWorld = LightSnapshot.NearbyEmitterRangeTiles * Tile.Size.X;
            WorldSpace playerFeet = player.FeetPosition;
            Point originTile = TileManager.GetGridPos(playerFeet);

            AddEmitterIfEligible(player, true, playerFeet, nearbyRangeWorld, ref count);

            for (int i = 0; i < guild.Count && count < LightSnapshot.MaxLights; i++)
            {
                GuildMember member = guild[i];
                if (member == null) continue;
                if (!player.Party.IsInParty(member)) continue;
                AddEmitterIfEligible(member, true, playerFeet, nearbyRangeWorld, ref count);
            }

            for (int i = 0; i < entities.Count && count < LightSnapshot.MaxLights; i++)
            {
                AddEmitterIfEligible(entities[i], false, playerFeet, nearbyRangeWorld, ref count);
            }

            for (int i = 0; i < guild.Count && count < LightSnapshot.MaxLights; i++)
            {
                AddEmitterIfEligible(guild[i], false, playerFeet, nearbyRangeWorld, ref count);
            }

            for (int i = 0; i < npcs.Count && count < LightSnapshot.MaxLights; i++)
            {
                AddEmitterIfEligible(npcs[i], false, playerFeet, nearbyRangeWorld, ref count);
            }

            return LightSnapshot.CreateFromScratch(count, lightPositionScratch, lightRadiusTilesScratch, lightCoreScratch, lightDistanceToPlayerScratch, originTile);
        }

        static void AddEmitterIfEligible(Entity entity, bool forceInclude, WorldSpace playerFeet, float nearbyRangeWorld, ref int count)
        {
            ThreadAffinity.AssertSimThread();
            if (entity == null) return;
            if (count >= LightSnapshot.MaxLights) return;
            if (entity is not ILightEmitter emitter) return;

            if (!forceInclude && entity.FeetPosition.DistanceTo(playerFeet) > nearbyRangeWorld)
            {
                return;
            }

            int renderId = entity.RenderId;
            for (int i = 0; i < count; i++)
            {
                if (lightRenderIdScratch[i] != renderId) continue;
                if (forceInclude) lightCoreScratch[i] = true;
                return;
            }

            WorldSpace feet = entity.FeetPosition;
            lightRenderIdScratch[count] = renderId;
            lightPositionScratch[count] = feet;
            lightRadiusTilesScratch[count] = Math.Max(0.1f, emitter.LightRadiusTiles);
            lightCoreScratch[count] = forceInclude;
            lightDistanceToPlayerScratch[count] = feet.DistanceTo(playerFeet);
            count++;
        }

        static void PublishEntitySnapshots<T>(IList<T> source, RenderCache<EntityRenderSnapshot> cache, HashSet<int> knownIds, HashSet<int> currentIds) where T : Entity
        {
            currentIds.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                Entity entity = source[i];
                if (entity == null) continue;
                EntityRenderSnapshot snapshot = entity.BuildRenderSnapshot();
                cache.EnqueueUpdate(snapshot);
                currentIds.Add(snapshot.RenderId);
            }

            PublishRemovals(cache, knownIds, currentIds);
        }

        static void PublishRemovals<TSnapshot>(RenderCache<TSnapshot> cache, HashSet<int> knownIds, HashSet<int> currentIds) where TSnapshot : struct, IRenderSnapshot
        {
            foreach (int id in knownIds)
            {
                if (!currentIds.Contains(id))
                {
                    cache.EnqueueRemove(id);
                }
            }
            knownIds.Clear();
            foreach (int id in currentIds)
            {
                knownIds.Add(id);
            }
        }

        static void PublishPlayerUiSnapshot()
        {
            if (player == null)
            {
                MailboxManager.PublishUiEvent(new PlayerUiSnapshot(false, false, 0, true, 1, WorldSpace.Zero, false, WorldSpace.Zero, Array.Empty<SpellUiSnapshot>()));
                return;
            }

            Spell[] knownSpells = player.SpellBook.Spells;
            SpellUiSnapshot[] spellSnapshots = new SpellUiSnapshot[knownSpells.Length];
            for (int i = 0; i < knownSpells.Length; i++)
            {
                Spell spell = knownSpells[i];
                spellSnapshots[i] = new SpellUiSnapshot(spell.SpellKey, spell.GfxPath, spell.OffCooldown, spell.RatioOfCooldownDone, SpellDescriptorSnapshot.FromSpell(spell));
            }

            Entity target = player.Target;
            MailboxManager.PublishUiEvent(new PlayerUiSnapshot(
                true,
                player.InCombatOrPartyInCombat,
                player.Gold,
                player.OffGlobalCooldown,
                player.RatioOfGlobalCooldownDone,
                player.FeetPosition,
                target != null,
                target?.FeetPosition ?? WorldSpace.Zero,
                spellSnapshots));
        }

        internal static void AppendMinimapDots(List<MinimapDotSnapshot> dots)
        {
            ThreadAffinity.AssertSimThread();
            if (dots == null) return;
            if (player != null)
            {
                dots.Add(new MinimapDotSnapshot(player.FeetPosition, player.MinimapColor, true));
            }
            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];
                if (entity == null) continue;
                dots.Add(new MinimapDotSnapshot(entity.FeetPosition, entity.MinimapColor));
            }
            for (int i = 0; i < guild.Count; i++)
            {
                GuildMember member = guild[i];
                if (member == null) continue;
                dots.Add(new MinimapDotSnapshot(member.FeetPosition, member.MinimapColor));
            }
            for (int i = 0; i < npcs.Count; i++)
            {
                Npc npc = npcs[i];
                if (npc == null) continue;
                dots.Add(new MinimapDotSnapshot(npc.FeetPosition, npc.MinimapColor));
            }
        }
    }
}
