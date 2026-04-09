using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging.Events;
using Project_1.Textures;

namespace Project_1.GameObjects.Spells
{
    internal static class GroundTargetingController
    {
        const float GroundTargetGraceRangeRatio = 0.10f;

        static Spell groundTargetPendingSpell;
        static readonly GfxPath groundTargetCircleIndicatorPath = new GfxPath(GfxType.UI, "AoECircle");
        static readonly GfxPath groundTargetRectangleIndicatorPath = new GfxPath(GfxType.UI, "WhiteBackground");
        static readonly GfxPath groundTargetInvalidIndicatorPath = new GfxPath(GfxType.UI, "AoEOutOfRange");
        static volatile GroundTargetPreviewSnapshot groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Inactive;
        static readonly List<GroundSpellVisualSnapshot> activeGroundSpellVisuals = new List<GroundSpellVisualSnapshot>();
        static volatile GroundSpellVisualSnapshot[] groundSpellVisualSnapshots = Array.Empty<GroundSpellVisualSnapshot>();

        public static bool HasPendingSpell => groundTargetPendingSpell != null;

        public static void Begin(Spell spell)
        {
            ThreadAffinity.AssertSimThread();
            if (spell == null || !spell.RequiresGroundTarget)
            {
                Cancel();
                return;
            }

            Player player = ObjectManager.Player;
            if (IsGroundSpellUnavailableFromCooldown(spell, player))
            {
                Cancel();
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

        public static void Cancel()
        {
            if (SimThread.IsRunning)
            {
                ThreadAffinity.AssertSimThread();
            }

            groundTargetPendingSpell = null;
            groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Inactive;
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            UpdateGroundTargetPreview();
            UpdateGroundSpellVisuals();
        }

        public static bool TryHandleWorldClick(WorldClickRequested clickEvent)
        {
            ThreadAffinity.AssertSimThread();
            if (!HasPendingSpell) return false;

            if (clickEvent.Button == ClickKind.Right)
            {
                Cancel();
                return true;
            }

            if (clickEvent.Button != ClickKind.Left) return true;

            Spell spell = groundTargetPendingSpell;
            Player player = ObjectManager.Player;
            if (spell == null || player == null)
            {
                Cancel();
                return true;
            }

            WorldSpace hoveredPos = WorldSpace.FromRelativeScreenSpace(clickEvent.RelativePos);
            GroundTargetPlacement placement = ResolveGroundTargetPlacement(spell, hoveredPos);
            if (placement.OutOfGrace)
            {
                Cancel();
                return true;
            }

            if (IsGroundSpellUnavailableFromCooldown(spell, player))
            {
                Cancel();
                return true;
            }

            if (!TryExecuteAt(placement.CastPosition))
            {
                if (IsGroundSpellUnavailableFromCooldown(spell, player))
                {
                    Cancel();
                }
                return true;
            }

            Cancel();
            return true;
        }

        public static bool TryExecuteAt(WorldSpace worldPos)
        {
            ThreadAffinity.AssertSimThread();
            if (!HasPendingSpell) return false;

            Player player = ObjectManager.Player;
            if (player == null) return false;

            Spell spell = groundTargetPendingSpell;
            if (spell == null || !spell.RequiresGroundTarget) return false;
            if (!player.StartCastAt(spell, worldPos)) return false;

            AddGroundSpellVisual(spell, worldPos);
            return true;
        }

        public static void DrawSpellEffects(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            if (batch == null) return;

            GroundSpellVisualSnapshot[] snapshots = groundSpellVisualSnapshots;
            for (int i = 0; i < snapshots.Length; i++)
            {
                Texture2D texture = TextureManager.GetTexture(snapshots[i].TexturePath);
                if (texture == null) continue;

                WorldSpace topLeftWorld = snapshots[i].Center - new WorldSpace((float)snapshots[i].Width * 0.5f, (float)snapshots[i].Height * 0.5f);
                AbsoluteScreenPosition topLeft = topLeftWorld.ToAbsoltueScreenPosition();
                Point size = new Point(
                    Math.Max(1, (int)Math.Round(snapshots[i].Width * Camera.Camera.Scale)),
                    Math.Max(1, (int)Math.Round(snapshots[i].Height * Camera.Camera.Scale)));
                batch.Draw(texture, new Rectangle(topLeft, size), Color.White * 0.85f);
            }
        }

        public static void DrawTargetPreview(SpriteBatch batch)
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

            double width = Math.Max(1f, snapshot.Width);
            double height = Math.Max(1f, snapshot.Height);
            WorldSpace topLeftWorld = snapshot.Center - new WorldSpace((float)width * 0.5f, (float)height * 0.5f);
            AbsoluteScreenPosition topLeft = topLeftWorld.ToAbsoltueScreenPosition();
            Point size = new Point(
                Math.Max(1, (int)Math.Round(width * Camera.Camera.Scale)),
                Math.Max(1, (int)Math.Round(height * Camera.Camera.Scale)));

            Color tint = snapshot.OutOfGrace ? Color.White : Color.IndianRed * 0.45f;
            batch.Draw(texture, new Rectangle(topLeft, size), tint);
        }

        static void UpdateGroundTargetPreview()
        {
            if (!HasPendingSpell)
            {
                groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Inactive;
                return;
            }

            if (StateManager.CurrentState != StateManager.States.Game)
            {
                Cancel();
                return;
            }

            WorldSpace hoveredPos = WorldSpace.FromRelativeScreenSpace(MouseStateCache.Relative);
            Spell spell = groundTargetPendingSpell;
            Player player = ObjectManager.Player;
            if (IsGroundSpellUnavailableFromCooldown(spell, player))
            {
                Cancel();
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
            if (spell?.HitEffectGfxPath == null) return;
            if (string.IsNullOrWhiteSpace(spell.HitEffectGfxPath.Name)) return;
            if (string.Equals(spell.HitEffectGfxPath.Name, "None", StringComparison.OrdinalIgnoreCase)) return;

            const double lifetimeMs = 1000d;
            activeGroundSpellVisuals.Add(new GroundSpellVisualSnapshot(
                spell.HitEffectGfxPath,
                worldPos,
                Math.Max(1, spell.GroundTargetWidth),
                Math.Max(1, spell.GroundTargetHeight),
                TimeManager.TotalFrameTime + lifetimeMs));
            groundSpellVisualSnapshots = activeGroundSpellVisuals.ToArray();
        }

        static GroundTargetPlacement ResolveGroundTargetPlacement(Spell spell, WorldSpace hoveredPos)
        {
            Player player = ObjectManager.Player;
            if (spell == null || player == null)
            {
                return new GroundTargetPlacement(hoveredPos, true);
            }

            double maxRange = Math.Max(0f, spell.CastDistance);
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

            double graceRange = maxRange * GroundTargetGraceRangeRatio;
            if (distance > maxRange + graceRange)
            {
                return new GroundTargetPlacement(hoveredPos, true);
            }

            WorldSpace clamped = casterPos + (toHovered / distance) * (float)maxRange;
            return new GroundTargetPlacement(clamped, false);
        }

        static bool IsGroundSpellUnavailableFromCooldown(Spell spell, Player player)
        {
            if (spell == null || player == null) return true;
            if (!spell.OffCooldown) return true;
            if (!player.OffGlobalCooldown) return true;
            return false;
        }

        sealed class GroundTargetPreviewSnapshot
        {
            public static readonly GroundTargetPreviewSnapshot Inactive = new GroundTargetPreviewSnapshot(false, WorldSpace.Zero, 0f, 0f, SpellData.GroundTargetShapeType.Circle, false);

            GroundTargetPreviewSnapshot(bool enabled, WorldSpace center, double width, double height, SpellData.GroundTargetShapeType shape, bool outOfGrace)
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
            public double Width { get; }
            public double Height { get; }
            public SpellData.GroundTargetShapeType Shape { get; }
            public bool OutOfGrace { get; }

            public static GroundTargetPreviewSnapshot Active(WorldSpace center, double width, double height, SpellData.GroundTargetShapeType shape, bool outOfGrace)
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
            public GroundSpellVisualSnapshot(GfxPath texturePath, WorldSpace center, double width, double height, double expireAtMs)
            {
                TexturePath = texturePath;
                Center = center;
                Width = width;
                Height = height;
                ExpireAtMs = expireAtMs;
            }

            public GfxPath TexturePath { get; }
            public WorldSpace Center { get; }
            public double Width { get; }
            public double Height { get; }
            public double ExpireAtMs { get; }
        }
    }
}
