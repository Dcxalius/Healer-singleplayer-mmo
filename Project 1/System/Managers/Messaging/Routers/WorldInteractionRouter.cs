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
using Microsoft.Xna.Framework;
using System;

namespace Project_1.GameObjects
{
    internal static class WorldInteractionRouter
    {
        enum WorldDragMode
        {
            None,
            OtsCameraRotate,
            OtsPlayerRotate,
            FreeCameraRotate,
            SuppressOnly
        }

        struct PendingWorldInteraction
        {
            public bool Active;
            public WorldClickRequested Click;
            public AbsoluteScreenPosition PressAbsolute;
            public AbsoluteScreenPosition LastAbsolute;
            public bool DragRecognized;
            public WorldDragMode DragMode;
        }

        const int DragThresholdPixels = 8;
        const float MouseRotationRadiansPerPixel = 0.01f;

        static bool initialized;
        static PendingWorldInteraction pendingInteraction;

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
            MailboxManager.Sim.Subscribe<MouseSnapshot>(HandleMouseSnapshot);
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
            ArmPendingInteraction(e);
        }

        static void HandleWorldReleaseRequested(WorldReleaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ReleaseEvent releaseEvent = new ReleaseEvent(null, e.RelativePos, e.Button.ToInputClickType(), e.ModifiersMask);
            StateManager.Release(releaseEvent);
            ResolvePendingInteractionOnRelease(e);
        }

        static void HandleWorldScrollRequested(WorldScrollRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ScrollEvent.Direction direction = e.Up ? ScrollEvent.Direction.Up : ScrollEvent.Direction.Down;
            ScrollEvent scrollEvent = new ScrollEvent(e.RelativePos, e.Steps, direction, e.ModifiersMask);
            StateManager.Scroll(scrollEvent);
        }

        static void HandleMouseSnapshot(MouseSnapshot snapshot)
        {
            ThreadAffinity.AssertSimThread();
            if (!pendingInteraction.Active) return;
            if (StateManager.CurrentState != StateManager.States.Game)
            {
                ClearPendingInteraction();
                return;
            }

            AbsoluteScreenPosition previousAbsolute = pendingInteraction.LastAbsolute;
            if (!DebugManager.Mode(DebugMode.ModelPreview))
            {
                pendingInteraction.LastAbsolute = snapshot.Absolute;
                return;
            }

            if (!pendingInteraction.DragRecognized)
            {
                Vector2 dragVector = (snapshot.Absolute - pendingInteraction.PressAbsolute).ToVector2();
                if (dragVector.LengthSquared() < DragThresholdPixels * DragThresholdPixels)
                {
                    pendingInteraction.LastAbsolute = snapshot.Absolute;
                    return;
                }

                pendingInteraction.DragRecognized = true;
                pendingInteraction.DragMode = ResolveDragMode(pendingInteraction.Click.Button);
                if (pendingInteraction.DragMode == WorldDragMode.None)
                {
                    pendingInteraction.DragRecognized = false;
                    return;
                }
            }

            int deltaX = snapshot.Absolute.X - previousAbsolute.X;
            pendingInteraction.LastAbsolute = snapshot.Absolute;
            if (deltaX == 0)
            {
                return;
            }

            float yawDelta = deltaX * MouseRotationRadiansPerPixel;
            switch (pendingInteraction.DragMode)
            {
                case WorldDragMode.OtsCameraRotate:
                    Camera.Camera.RotatePreviewCamera(yawDelta, false);
                    break;
                case WorldDragMode.OtsPlayerRotate:
                    ObjectManager.Player?.RotatePreviewFacing(yawDelta);
                    break;
                case WorldDragMode.FreeCameraRotate:
                    Camera.Camera.RotatePreviewCamera(yawDelta, true);
                    break;
                case WorldDragMode.SuppressOnly:
                    break;
            }

        }

        static void ArmPendingInteraction(in WorldClickRequested clickEvent)
        {
            AbsoluteScreenPosition absolute = AbsoluteScreenPosition.FromRelativeScreenPosition(clickEvent.RelativePos, Camera.Camera.WindowSize);
            pendingInteraction = new PendingWorldInteraction
            {
                Active = true,
                Click = clickEvent,
                PressAbsolute = absolute,
                LastAbsolute = absolute,
                DragRecognized = false,
                DragMode = WorldDragMode.None
            };
        }

        static void ResolvePendingInteractionOnRelease(in WorldReleaseRequested releaseEvent)
        {
            if (!pendingInteraction.Active) return;
            if (releaseEvent.Button != pendingInteraction.Click.Button) return;

            WorldClickRequested clickEvent = pendingInteraction.Click;
            bool dragRecognized = pendingInteraction.DragRecognized;
            WorldDragMode dragMode = pendingInteraction.DragMode;
            ClearPendingInteraction();
            if (dragRecognized || StateManager.CurrentState != StateManager.States.Game)
            {
                if (dragRecognized && dragMode == WorldDragMode.OtsCameraRotate)
                {
                    Camera.Camera.SnapPreviewCameraBehindPlayer();
                }
                return;
            }

            if (GroundTargetingController.TryHandleWorldClick(clickEvent)) return;
            RouteWorldClick(clickEvent);
        }

        static void ClearPendingInteraction()
        {
            pendingInteraction = default;
        }

        static WorldDragMode ResolveDragMode(ClickKind aButton)
        {
            PreviewCameraMode previewMode = Camera.Camera.CurrentPreviewCameraMode;
            return (previewMode, aButton) switch
            {
                (PreviewCameraMode.OverTheShoulder, ClickKind.Left) => WorldDragMode.OtsCameraRotate,
                (PreviewCameraMode.OverTheShoulder, ClickKind.Right) => WorldDragMode.OtsPlayerRotate,
                (PreviewCameraMode.Free, ClickKind.Left) => WorldDragMode.FreeCameraRotate,
                (PreviewCameraMode.Free, ClickKind.Right) => WorldDragMode.SuppressOnly,
                _ => WorldDragMode.None
            };
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
