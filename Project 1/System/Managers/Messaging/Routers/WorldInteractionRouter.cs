using Project_1.Camera;
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
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Tiles;
using System;

namespace Project_1.GameObjects
{
    internal static class WorldInteractionRouter
    {
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            SubscribeSimCommand<TargetRequested>(HandleTargetRequested);
            SubscribeSimCommand<PartyMemberInviteRequested>(HandlePartyMemberInviteRequested);
            SubscribeSimCommand<PartyMemberKickRequested>(HandlePartyMemberKickRequested);
            SubscribeSimCommand<WorldClickRequested>(HandleWorldClickRequested);
            SubscribeSimCommand<WorldReleaseRequested>(HandleWorldReleaseRequested);
            SubscribeSimCommand<WorldScrollRequested>(HandleWorldScrollRequested);
            SubscribeSimCommand<PlayerMovementRequested>(HandlePlayerMovementRequested);
            SubscribeSimCommand<MoveOrderRequested>(HandleMoveOrderRequested);
            SubscribeSimCommand<PartyTargetOrderRequested>(HandlePartyTargetOrderRequested);
            SubscribeSimCommand<TargetClearedRequested>(_ => HandleTargetClearedRequested());
            SubscribeSimCommand<PartyCommandRequested>(HandlePartyCommandRequested);
            SubscribeSimCommand<InteractRequested>(HandleInteractRequested);
        }

        static void SubscribeSimCommand<T>(Action<T> handler)
        {
            MailboxManager.RegisterSimCommandType<T>();
            MailboxManager.Sim.Subscribe(handler);
        }

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
            if (StateManager.CurrentState != StateManager.States.Game) return;
            if (GroundTargetingController.TryHandleWorldClick(e)) return;
            RouteWorldClick(e);
        }

        static void HandleWorldReleaseRequested(WorldReleaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ReleaseEvent releaseEvent = new ReleaseEvent(null, e.RelativePos, e.Button.ToInputClickType(), e.ModifiersMask);
            StateManager.Release(releaseEvent);
        }

        static void HandleWorldScrollRequested(WorldScrollRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ScrollEvent.Direction direction = e.Up ? ScrollEvent.Direction.Up : ScrollEvent.Direction.Down;
            ScrollEvent scrollEvent = new ScrollEvent(e.RelativePos, e.Steps, direction, e.ModifiersMask);
            StateManager.Scroll(scrollEvent);
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
                MailboxManager.PublishSimCommand(new InteractRequested(corpse.RenderId, clickEvent.Button));
                return;
            }

            if (TileManager.TryGetDoodadAt(worldPos, out Doodad doodad))
            {
                MailboxManager.PublishSimCommand(new InteractRequested(doodad.RenderId, clickEvent.Button));
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
                MailboxManager.PublishSimCommand(new TargetRequested(entity.RenderId));
                if (rightClick)
                {
                    MailboxManager.PublishSimCommand(new PartyTargetOrderRequested(entity.RenderId));
                }
            }
            else if (entity is GuildMember member)
            {
                if (clickEvent.Modifier(InputManager.HoldModifier.Shift))
                {
                    MailboxManager.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.Add, member.RenderId));
                }
                else if (clickEvent.Modifier(InputManager.HoldModifier.Ctrl))
                {
                    MailboxManager.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.NeedyAdd, member.RenderId));
                }
            }

            if (entity is Npc npc)
            {
                MailboxManager.PublishSimCommand(new InteractRequested(npc.RenderId, clickEvent.Button));
            }
        }

        static void HandleGroundWorldClick(WorldSpace worldPos, in WorldClickRequested clickEvent)
        {
            if (clickEvent.Button == ClickKind.Left)
            {
                if (clickEvent.Modifier(InputManager.HoldModifier.Shift) || clickEvent.Modifier(InputManager.HoldModifier.Ctrl))
                {
                    MailboxManager.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.Clear, null));
                    return;
                }

                MailboxManager.PublishSimCommand(new TargetClearedRequested());
                return;
            }

            if (clickEvent.Button == ClickKind.Right)
            {
                bool append = clickEvent.Modifier(InputManager.HoldModifier.Shift);
                MailboxManager.PublishSimCommand(new MoveOrderRequested(worldPos, append));
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
