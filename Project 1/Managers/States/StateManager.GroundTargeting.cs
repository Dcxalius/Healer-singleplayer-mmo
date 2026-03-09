using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.Textures;

namespace Project_1.Managers.States
{
    internal static partial class StateManager
    {
        static void HandleSpellCastRequested(SpellCastRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!player.SpellBook.TryGetSpell(e.SpellName, out Spell spell)) return;

            if (spell.RequiresGroundTarget)
            {
                BeginGroundTargeting(spell);
                return;
            }

            CancelGroundTargeting();
            player.StartCast(spell);
        }

        static bool HasGroundTargetPendingSpell => groundTargetPendingSpell != null;

        static void BeginGroundTargeting(Spell spell)
        {
            ThreadAffinity.AssertSimThread();
            if (spell == null || !spell.RequiresGroundTarget)
            {
                CancelGroundTargeting();
                return;
            }

            Player player = ObjectManager.Player;
            if (IsGroundSpellUnavailableFromCooldown(spell, player))
            {
                CancelGroundTargeting();
                return;
            }

            groundTargetPendingSpell = spell;
            WorldSpace hoveredPos = WorldSpace.FromRelativeScreenSpace(MouseStateCache.Relative);
            GroundTargetPlacement placement = ResolveGroundTargetPlacement(spell, hoveredPos);
            groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Active(
                placement.CastPosition,
                spell.GroundTargetWidth,
                spell.GroundTargetHeight,
                spell.GroundTargetShape,
                placement.OutOfGrace);
        }

        static void CancelGroundTargeting()
        {
            if (SimThread.IsRunning)
            {
                ThreadAffinity.AssertSimThread();
            }
            groundTargetPendingSpell = null;
            groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Inactive;
        }

        static void UpdateGroundTargetPreview()
        {
            ThreadAffinity.AssertSimThread();
            if (!HasGroundTargetPendingSpell)
            {
                groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Inactive;
                return;
            }

            if (currentStateEnum != States.Game)
            {
                CancelGroundTargeting();
                return;
            }

            WorldSpace hoveredPos = WorldSpace.FromRelativeScreenSpace(MouseStateCache.Relative);
            Spell spell = groundTargetPendingSpell;
            Player player = ObjectManager.Player;
            if (IsGroundSpellUnavailableFromCooldown(spell, player))
            {
                CancelGroundTargeting();
                return;
            }

            GroundTargetPlacement placement = ResolveGroundTargetPlacement(spell, hoveredPos);
            groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Active(
                placement.CastPosition,
                spell.GroundTargetWidth,
                spell.GroundTargetHeight,
                spell.GroundTargetShape,
                placement.OutOfGrace);
        }

        static void UpdateGroundSpellVisuals()
        {
            ThreadAffinity.AssertSimThread();
            if (activeGroundSpellVisuals.Count == 0)
            {
                groundSpellVisualSnapshots = Array.Empty<GroundSpellVisualSnapshot>();
                return;
            }

            double now = TimeManager.TotalFrameTime;
            for (int i = activeGroundSpellVisuals.Count - 1; i >= 0; i--)
            {
                if (activeGroundSpellVisuals[i].ExpireAtMs > now) continue;
                activeGroundSpellVisuals.RemoveAt(i);
            }

            groundSpellVisualSnapshots = activeGroundSpellVisuals.ToArray();
        }

        static void AddGroundSpellVisual(Spell spell, WorldSpace worldPos)
        {
            ThreadAffinity.AssertSimThread();
            if (spell?.HitEffectGfxPath == null) return;
            if (string.IsNullOrWhiteSpace(spell.HitEffectGfxPath.Name)) return;
            if (string.Equals(spell.HitEffectGfxPath.Name, "None", StringComparison.OrdinalIgnoreCase)) return;

            const double lifetimeMs = 1000d;
            activeGroundSpellVisuals.Add(new GroundSpellVisualSnapshot(
                spell.HitEffectGfxPath,
                worldPos,
                Math.Max(1f, spell.GroundTargetWidth),
                Math.Max(1f, spell.GroundTargetHeight),
                TimeManager.TotalFrameTime + lifetimeMs));
            groundSpellVisualSnapshots = activeGroundSpellVisuals.ToArray();
        }

        static GroundTargetPlacement ResolveGroundTargetPlacement(Spell spell, WorldSpace hoveredPos)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (spell == null || player == null)
            {
                return new GroundTargetPlacement(hoveredPos, true);
            }

            float maxRange = Math.Max(0f, spell.CastDistance);
            if (maxRange <= 0f)
            {
                return new GroundTargetPlacement(hoveredPos, false);
            }

            WorldSpace casterPos = player.FeetPosition;
            WorldSpace toHovered = hoveredPos - casterPos;
            float distance = toHovered.ToVector2().Length();
            if (distance <= maxRange || distance <= 0.0001f)
            {
                return new GroundTargetPlacement(hoveredPos, false);
            }

            float graceRange = maxRange * GroundTargetGraceRangeRatio;
            if (distance > maxRange + graceRange)
            {
                return new GroundTargetPlacement(hoveredPos, true);
            }

            WorldSpace clamped = casterPos + (toHovered / distance) * maxRange;
            return new GroundTargetPlacement(clamped, false);
        }

        static bool IsGroundSpellUnavailableFromCooldown(Spell spell, Player player)
        {
            ThreadAffinity.AssertSimThread();
            if (spell == null || player == null) return true;
            if (!spell.OffCooldown) return true;
            if (!player.OffGlobalCooldown) return true;
            return false;
        }

        public static void DrawGroundSpellEffects(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            if (batch == null) return;

            GroundSpellVisualSnapshot[] snapshots = groundSpellVisualSnapshots;
            for (int i = 0; i < snapshots.Length; i++)
            {
                Texture2D texture = TextureManager.GetTexture(snapshots[i].TexturePath);
                if (texture == null) continue;

                WorldSpace topLeftWorld = snapshots[i].Center - new WorldSpace(snapshots[i].Width * 0.5f, snapshots[i].Height * 0.5f);
                AbsoluteScreenPosition topLeft = topLeftWorld.ToAbsoltueScreenPosition();
                Point size = new Point(
                    Math.Max(1, (int)MathF.Round(snapshots[i].Width * Camera.Camera.Scale)),
                    Math.Max(1, (int)MathF.Round(snapshots[i].Height * Camera.Camera.Scale)));
                batch.Draw(texture, new Rectangle(topLeft, size), Color.White * 0.85f);
            }
        }

        static bool TryExecuteGroundTargetedSpellAt(WorldSpace worldPos)
        {
            ThreadAffinity.AssertSimThread();
            if (!HasGroundTargetPendingSpell) return false;

            Player player = ObjectManager.Player;
            if (player == null) return false;

            Spell spell = groundTargetPendingSpell;
            if (spell == null || !spell.RequiresGroundTarget) return false;
            if (!player.StartCastAt(spell, worldPos)) return false;

            AddGroundSpellVisual(spell, worldPos);
            return true;
        }

        public static void DrawGroundTargetPreview(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            if (batch == null) return;

            GroundTargetPreviewSnapshot snapshot = groundTargetPreviewSnapshot;
            if (!snapshot.Enabled) return;

            GfxPath texturePath = snapshot.OutOfGrace
                ? groundTargetInvalidIndicatorPath
                : snapshot.Shape == SpellData.GroundTargetShapeType.Rectangle
                    ? groundTargetRectangleIndicatorPath
                    : groundTargetCircleIndicatorPath;
            Texture2D texture = TextureManager.GetTexture(texturePath);
            if (texture == null) return;

            float width = Math.Max(1f, snapshot.Width);
            float height = Math.Max(1f, snapshot.Height);
            WorldSpace topLeftWorld = snapshot.Center - new WorldSpace(width * 0.5f, height * 0.5f);
            AbsoluteScreenPosition topLeft = topLeftWorld.ToAbsoltueScreenPosition();
            Point size = new Point(
                Math.Max(1, (int)MathF.Round(width * Camera.Camera.Scale)),
                Math.Max(1, (int)MathF.Round(height * Camera.Camera.Scale)));

            Color tint = snapshot.OutOfGrace ? Color.White : Color.IndianRed * 0.45f;
            batch.Draw(texture, new Rectangle(topLeft, size), tint);
        }

        sealed class GroundTargetPreviewSnapshot
        {
            public static readonly GroundTargetPreviewSnapshot Inactive = new GroundTargetPreviewSnapshot(false, WorldSpace.Zero, 0f, 0f, SpellData.GroundTargetShapeType.Circle, false);

            GroundTargetPreviewSnapshot(bool enabled, WorldSpace center, float width, float height, SpellData.GroundTargetShapeType shape, bool outOfGrace)
            {
                Enabled = enabled;
                Center = center;
                Width = width;
                Height = height;
                Shape = shape;
                OutOfGrace = outOfGrace;
            }

            public bool Enabled { get; }
            public WorldSpace Center { get; }
            public float Width { get; }
            public float Height { get; }
            public SpellData.GroundTargetShapeType Shape { get; }
            public bool OutOfGrace { get; }

            public static GroundTargetPreviewSnapshot Active(WorldSpace center, float width, float height, SpellData.GroundTargetShapeType shape, bool outOfGrace)
            {
                return new GroundTargetPreviewSnapshot(true, center, width, height, shape, outOfGrace);
            }
        }

        readonly struct GroundTargetPlacement
        {
            public GroundTargetPlacement(WorldSpace castPosition, bool outOfGrace)
            {
                CastPosition = castPosition;
                OutOfGrace = outOfGrace;
            }

            public WorldSpace CastPosition { get; }
            public bool OutOfGrace { get; }
        }

        readonly struct GroundSpellVisualSnapshot
        {
            public GroundSpellVisualSnapshot(GfxPath texturePath, WorldSpace center, float width, float height, double expireAtMs)
            {
                TexturePath = texturePath;
                Center = center;
                Width = width;
                Height = height;
                ExpireAtMs = expireAtMs;
            }

            public GfxPath TexturePath { get; }
            public WorldSpace Center { get; }
            public float Width { get; }
            public float Height { get; }
            public double ExpireAtMs { get; }
        }
    }
}
