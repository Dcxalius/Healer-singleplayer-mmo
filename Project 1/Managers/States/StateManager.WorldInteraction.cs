using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Tiles;

namespace Project_1.Managers.States
{
    internal static partial class StateManager
    {
        static void HandleTargetRequested(TargetRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!e.TargetRenderId.HasValue)
            {
                player.SetTarget(player);
                return;
            }

            if (!TryResolveEntityByRenderId(e.TargetRenderId.Value, out Entity target))
            {
                player.SetTarget(player);
                return;
            }

            player.SetTarget(target);
        }

        static void HandlePartyMemberInviteRequested(PartyMemberInviteRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId, out GuildMember member)) return;
            ObjectManager.SpawnGuildMemberToParty(member, null);
        }

        static void HandlePartyMemberKickRequested(PartyMemberKickRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId, out GuildMember member)) return;
            ObjectManager.RemoveGuildMemberFromParty(member);
        }

        static void HandleWorldClickRequested(WorldClickRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (currentState == null || currentState.GetStateEnum != States.Game) return;
            if (TryHandleGroundTargetWorldClick(e)) return;
            RouteWorldClick(e);
        }

        static void HandleWorldReleaseRequested(WorldReleaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ReleaseEvent releaseEvent = new ReleaseEvent(null, e.RelativePos, e.Button.ToInputClickType(), e.ModifiersMask);
            Release(releaseEvent);
        }

        static void HandleWorldScrollRequested(WorldScrollRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ScrollEvent.Direction direction = e.Up ? ScrollEvent.Direction.Up : ScrollEvent.Direction.Down;
            ScrollEvent scrollEvent = new ScrollEvent(e.RelativePos, e.Steps, direction, e.ModifiersMask);
            Scroll(scrollEvent);
        }

        static bool TryHandleGroundTargetWorldClick(in WorldClickRequested clickEvent)
        {
            ThreadAffinity.AssertSimThread();
            if (!HasGroundTargetPendingSpell) return false;

            if (clickEvent.Button == ClickKind.Right)
            {
                CancelGroundTargeting();
                return true;
            }

            if (clickEvent.Button != ClickKind.Left) return true;

            Spell spell = groundTargetPendingSpell;
            Player player = ObjectManager.Player;
            if (spell == null || player == null)
            {
                CancelGroundTargeting();
                return true;
            }

            WorldSpace hoveredPos = WorldSpace.FromRelativeScreenSpace(clickEvent.RelativePos);
            GroundTargetPlacement placement = ResolveGroundTargetPlacement(spell, hoveredPos);
            if (placement.OutOfGrace)
            {
                CancelGroundTargeting();
                return true;
            }

            if (IsGroundSpellUnavailableFromCooldown(spell, player))
            {
                CancelGroundTargeting();
                return true;
            }

            if (!TryExecuteGroundTargetedSpellAt(placement.CastPosition))
            {
                if (IsGroundSpellUnavailableFromCooldown(spell, player))
                {
                    CancelGroundTargeting();
                }
                return true;
            }

            CancelGroundTargeting();
            return true;
        }

        static void RouteWorldClick(in WorldClickRequested clickEvent)
        {
            WorldSpace worldPos = WorldSpace.FromRelativeScreenSpace(clickEvent.RelativePos);

            if (ObjectManager.TryGetEntityAt(worldPos, out Entity entity))
            {
                HandleEntityWorldClick(entity, clickEvent);
                return;
            }

            if (SpawnerManager.TryGetSpawnAt(worldPos, out Entity spawn))
            {
                HandleEntityWorldClick(spawn, clickEvent);
                return;
            }

            if (CorpseManager.TryGetCorpseAt(worldPos, out Corpse corpse))
            {
                Mailboxes.PublishSimCommand(new InteractRequested(corpse.RenderId, clickEvent.Button));
                return;
            }

            if (TileManager.TryGetDoodadAt(worldPos, out Doodad doodad))
            {
                Mailboxes.PublishSimCommand(new InteractRequested(doodad.RenderId, clickEvent.Button));
                return;
            }

            HandleGroundWorldClick(worldPos, clickEvent);
        }

        static void HandleEntityWorldClick(Entity entity, in WorldClickRequested clickEvent)
        {
            bool noModifiers = clickEvent.NoModifiers();
            bool rightClick = clickEvent.Button == ClickKind.Right;

            if (noModifiers)
            {
                Mailboxes.PublishSimCommand(new TargetRequested(entity.RenderId));
                if (rightClick)
                {
                    Mailboxes.PublishSimCommand(new PartyTargetOrderRequested(entity.RenderId));
                }
            }
            else if (entity is GuildMember member)
            {
                if (clickEvent.Modifier(InputManager.HoldModifier.Shift))
                {
                    Mailboxes.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.Add, member.RenderId));
                }
                else if (clickEvent.Modifier(InputManager.HoldModifier.Ctrl))
                {
                    Mailboxes.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.NeedyAdd, member.RenderId));
                }
            }

            if (entity is Npc npc)
            {
                Mailboxes.PublishSimCommand(new InteractRequested(npc.RenderId, clickEvent.Button));
            }
        }

        static void HandleGroundWorldClick(WorldSpace worldPos, in WorldClickRequested clickEvent)
        {
            if (clickEvent.Button == ClickKind.Left)
            {
                if (clickEvent.Modifier(InputManager.HoldModifier.Shift) || clickEvent.Modifier(InputManager.HoldModifier.Ctrl))
                {
                    Mailboxes.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.Clear, null));
                    return;
                }

                Mailboxes.PublishSimCommand(new TargetClearedRequested());
                return;
            }

            if (clickEvent.Button == ClickKind.Right)
            {
                bool append = clickEvent.Modifier(InputManager.HoldModifier.Shift);
                Mailboxes.PublishSimCommand(new MoveOrderRequested(worldPos, append));
            }
        }

        static void HandlePlayerMovementRequested(PlayerMovementRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.ApplyMoveInput(e.Left, e.Right, e.Up, e.Down);
        }

        static void HandleMoveOrderRequested(MoveOrderRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Party.IssueMoveOrder(e.Destination, e.Append);
        }

        static void HandlePartyTargetOrderRequested(PartyTargetOrderRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!TryResolveEntityByRenderId(e.TargetRenderId, out Entity target)) return;
            player.Party.IssueTargetOrder(target);
        }

        static void HandleTargetClearedRequested()
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.RemoveTarget();
        }

        static void HandlePartyCommandRequested(PartyCommandRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            switch (e.Action)
            {
                case PartyCommandAction.Clear:
                    player.Party.ClearCommand();
                    break;
                case PartyCommandAction.Add:
                    if (e.MemberRenderId.HasValue &&
                        ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId.Value, out GuildMember addMember))
                    {
                        player.Party.AddToCommand(addMember);
                    }
                    break;
                case PartyCommandAction.NeedyAdd:
                    if (e.MemberRenderId.HasValue &&
                        ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId.Value, out GuildMember needyMember))
                    {
                        player.Party.NeedyAddToCommand(needyMember);
                    }
                    break;
            }
        }

        static void HandleInteractRequested(InteractRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (CorpseManager.TryGetCorpseByRenderId(e.TargetRenderId, out Corpse corpse))
            {
                if (e.Button != ClickKind.Right) return;
                corpse.TryOpenLoot();
                return;
            }

            if (TileManager.TryGetDoodadByRenderId(e.TargetRenderId, out Doodad doodad))
            {
                if (doodad is Chest chest)
                {
                    chest.TryOpenLoot();
                }
                return;
            }

            if (!ObjectManager.TryGetEntityByRenderId(e.TargetRenderId, out Entity entity)) return;
            if (entity is Npc npc)
            {
                npc.TryBeginConversation();
            }
        }

        static bool TryResolveEntityByRenderId(int renderId, out Entity entity)
        {
            ThreadAffinity.AssertSimThread();
            if (ObjectManager.TryGetEntityByRenderId(renderId, out entity))
            {
                return true;
            }

            if (SpawnerManager.TryGetSpawnByRenderId(renderId, out entity))
            {
                return true;
            }

            entity = null;
            return false;
        }
    }
}
