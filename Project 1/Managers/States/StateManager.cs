using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Particles;
using Project_1.GameObjects.Spells;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements;
using Project_1.Tiles;
using Project_1.Textures;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.Managers;
using Project_1.Managers.Saves;

namespace Project_1.Managers.States
{

    internal static class StateManager
    {
        public enum States
        {
            StartScreen,
            Game,
            MoveHUD,
            PauseMenu,
            OptionMenu,
            LoadingMenu,
            NewGame
        }

        static State currentState;
        static States currentStateEnum;

        static StartScreen startScreen;
        static Game game;
        static MoveHUD moveHUD;
        static PauseMenu pauseMenu;
        static OptionMenu optionMenu;
        static LoadingMenu loadingMenu;
        static NewGame newGame;

        static SpriteBatch finalBatch;

        public static RenderTarget2D CleanGameTarget => game.CleanGameDraw();
        public static RenderTarget2D FinalGameFrame { get => finalGameFrame; set => finalGameFrame = value; }
        static RenderTarget2D finalGameFrame;
        static bool pendingRedrawGame;
        public static Rectangle RenderTargetPosition { set => renderTargetPosition = value; }
        static Rectangle renderTargetPosition;

        public static States PreviousState => previousState;
        static States previousState;
        static bool stateChangePending;
        static States pendingState;
        static bool initialized;
        const float GroundTargetGraceRangeRatio = 0.10f;
        static Spell groundTargetPendingSpell;
        static readonly GfxPath groundTargetCircleIndicatorPath = new GfxPath(GfxType.UI, "AoECircle");
        static readonly GfxPath groundTargetRectangleIndicatorPath = new GfxPath(GfxType.UI, "WhiteBackground");
        static readonly GfxPath groundTargetInvalidIndicatorPath = new GfxPath(GfxType.UI, "AoEOutOfRange");
        static volatile GroundTargetPreviewSnapshot groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Inactive;
        static readonly List<GroundSpellVisualSnapshot> activeGroundSpellVisuals = new List<GroundSpellVisualSnapshot>();
        static volatile GroundSpellVisualSnapshot[] groundSpellVisualSnapshots = Array.Empty<GroundSpellVisualSnapshot>();
        static readonly ChatCommandSpec[] chatCommandSpecs =
        {
            new ChatCommandSpec("help", "/help", "Shows available chat commands.", ChatCommandAccess.System),
            new ChatCommandSpec("clear", "/clear", "Clears the chat panel.", ChatCommandAccess.System),
            new ChatCommandSpec("where", "/where <friendly name>", "Prints world position for a friendly.", ChatCommandAccess.System),
            new ChatCommandSpec("chunklevels", "/chunklevels", "Prints chunk average levels (10x10 near player, or all generated if under 100 chunks).", ChatCommandAccess.System),
            new ChatCommandSpec("tp", "/tp <friendly name> <x> <y>", "Teleports a friendly to world coordinates.", ChatCommandAccess.Debug),
            new ChatCommandSpec("createitem", "/createitem <friendly name> <item id> <count>", "Creates item(s) and gives them to a friendly with inventory.", ChatCommandAccess.Debug)
        };
        const int ChunkLevelsWindowSize = 10;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            finalBatch = GraphicsManager.CreateSpriteBatch();

            startScreen = new StartScreen();
            game = new Game();
            moveHUD = new MoveHUD();
            pauseMenu = new PauseMenu();
            optionMenu = new OptionMenu();
            loadingMenu = new LoadingMenu();
            newGame = new NewGame();

            currentState = startScreen;
            currentStateEnum = States.StartScreen;

            Mailboxes.Ui.Subscribe<StateChanged>(HandleUiStateChanged);
            SubscribeSimCommand<StateChangeRequested>(e => SetState(e.State.ToStateManagerState()));
            SubscribeSimCommand<ResetToMainMenuRequested>(_ => HandleResetToMainMenuRequested());
            SubscribeSimCommand<CreateNewPlayerRequested>(HandleCreateNewPlayerRequested);
            SubscribeSimCommand<SaveDataRequested>(_ => HandleSaveDataRequested());
            SubscribeSimCommand<LoadSaveRequested>(HandleLoadSaveRequested);
            SubscribeSimCommand<ContinueLastSaveRequested>(_ => HandleContinueLastSaveRequested());
            SubscribeSimCommand<NewGameRequested>(_ => HandleNewGameRequested());
            SubscribeSimCommand<ShopPurchaseRequested>(HandleShopPurchaseRequested);
            SubscribeSimCommand<SpellCastRequested>(HandleSpellCastRequested);
            SubscribeSimCommand<TargetRequested>(HandleTargetRequested);
            SubscribeSimCommand<LogicWindowSnapshotRequested>(HandleLogicWindowSnapshotRequested);
            SubscribeSimCommand<SaveLoadParsed>(HandleSaveLoadParsed);
            SubscribeSimCommand<PartyMemberInviteRequested>(HandlePartyMemberInviteRequested);
            SubscribeSimCommand<PartyMemberKickRequested>(HandlePartyMemberKickRequested);
            SubscribeSimCommand<InventorySwapItemsRequested>(HandleInventorySwapItemsRequested);
            SubscribeSimCommand<InventorySwapEquipmentRequested>(HandleInventorySwapEquipmentRequested);
            SubscribeSimCommand<InventoryEquipBagRequested>(HandleInventoryEquipBagRequested);
            SubscribeSimCommand<InventoryUnequipBagRequested>(HandleInventoryUnequipBagRequested);
            SubscribeSimCommand<InventorySwapBagsRequested>(HandleInventorySwapBagsRequested);
            SubscribeSimCommand<InventorySwapBagSlotsRequested>(HandleInventorySwapBagSlotsRequested);
            SubscribeSimCommand<LootItemRequested>(HandleLootItemRequested);
            SubscribeSimCommand<InventoryEquipRequested>(HandleInventoryEquipRequested);
            SubscribeSimCommand<InventoryConsumeRequested>(HandleInventoryConsumeRequested);
            SubscribeSimCommand<EquipmentSwapRequested>(HandleEquipmentSwapRequested);
            SubscribeSimCommand<EquipmentMoveToInventoryRequested>(HandleEquipmentMoveToInventoryRequested);
            SubscribeSimCommand<WorldClickRequested>(HandleWorldClickRequested);
            SubscribeSimCommand<WorldReleaseRequested>(HandleWorldReleaseRequested);
            SubscribeSimCommand<WorldScrollRequested>(HandleWorldScrollRequested);
            SubscribeSimCommand<PlayerMovementRequested>(HandlePlayerMovementRequested);
            SubscribeSimCommand<MoveOrderRequested>(HandleMoveOrderRequested);
            SubscribeSimCommand<PartyTargetOrderRequested>(HandlePartyTargetOrderRequested);
            SubscribeSimCommand<TargetClearedRequested>(_ => HandleTargetClearedRequested());
            SubscribeSimCommand<PartyCommandRequested>(HandlePartyCommandRequested);
            SubscribeSimCommand<InteractRequested>(HandleInteractRequested);
            SubscribeSimCommand<ChatCommandRequested>(HandleChatCommandRequested);
            SubscribeSimCommand<ChatSayRequested>(HandleChatSayRequested);
            SubscribeSimMailbox<KeyboardSnapshot>(e => KeyboardStateCache.Update(e));
            SubscribeSimMailbox<KeyBindSnapshot>(e => KeyBindStateCache.Update(e));
            SubscribeSimMailbox<MouseSnapshot>(e => MouseStateCache.Update(e));
            SubscribeSimCommand<EscapeRequested>(_ => HandleEscapePressed());
            Mailboxes.Ui.Subscribe<HudRescaleRequested>(e => UiRescale(e.WindowSize));
        }

        static void SubscribeSimCommand<T>(Action<T> handler)
        {
            Mailboxes.RegisterSimCommandType<T>();
            SubscribeSimMailbox(handler);
        }

        static void SubscribeSimMailbox<T>(Action<T> handler)
        {
            Mailboxes.Sim.Subscribe(handler);
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            KeyboardStateCache.BeginFrame();
            KeyBindStateCache.BeginFrame();
            ApplyPendingStateChange();
            UpdateGroundTargetPreview();
            UpdateGroundSpellVisuals();
            currentState.Update();
        }

        public static void PopUp(DialogueBox aDialogueBox) => currentState.PopUp(aDialogueBox);
        public static void RemovePopUp(DialogueBox aDialogueBox) => currentState.RemovePopUp(aDialogueBox);

        public static void SetState(States aState)
        {
            ThreadAffinity.AssertSimThread();
            pendingState = aState;
            stateChangePending = true;
        }

        public static void RequestStateChange(States aState)
        {
            if (!SimThread.IsRunning || ThreadAffinity.IsSimThread)
            {
                SetState(aState);
                return;
            }
            Mailboxes.PublishSimCommand(new StateChangeRequested(aState.ToStateKind()));
        }

        static void ApplyPendingStateChange()
        {
            if (!stateChangePending) return;
            stateChangePending = false;
            ApplyStateChange(pendingState);
        }

        static void ApplyStateChange(States aState)
        {
            if (aState != States.Game)
            {
                CancelGroundTargeting();
            }

            States leavingState = currentStateEnum;
            currentState.OnLeave();
            previousState = leavingState;
            switch (aState)
            {
                case States.StartScreen:
                    currentState = startScreen;
                    break;
                case States.Game:
                    currentState = game;
                    break;
                case States.PauseMenu:
                    currentState = pauseMenu;
                    break;
                case States.OptionMenu:
                    currentState = optionMenu;
                    break;
                case States.MoveHUD:
                    currentState = moveHUD;
                    break;
                case States.LoadingMenu:
                    currentState = loadingMenu;
                    break;
                case States.NewGame:
                    currentState = newGame;
                    break;
                default:
                    throw new NotImplementedException();
            }
            currentStateEnum = aState;
            currentState.OnEnter();
            if (UiThread.IsRunning || SimThread.IsRunning)
            {
                Mailboxes.PublishUiEvent(new StateChanged(leavingState.ToStateKind(), aState.ToStateKind()));
                return;
            }

            UiOnLeave(leavingState);
            UiOnEnter(aState);
        }

        public static void RedrawGame()
        {
            if (!ThreadAffinity.IsMainThread)
            {
                pendingRedrawGame = true;
                return;
            }
            finalGameFrame = game.Draw();
        }


        public static bool Release(ReleaseEvent aRelease)
        {
            ThreadAffinity.AssertSimThread();
            return currentState.Release(aRelease);
        }

        public static bool Scroll(ScrollEvent aScroll)
        {
            ThreadAffinity.AssertSimThread();
            if (currentState.Scroll(aScroll)) return true;
            Camera.Camera.Scroll(aScroll);
            return true;
        }

        public static void Rescale()
        {
            ThreadAffinity.AssertMainThread();
            if (currentState == null) return;
            game.Rescale();
            moveHUD.Rescale();
            optionMenu.Rescale();
            pauseMenu.Rescale();
            startScreen.Rescale();
            loadingMenu.Rescale();
            newGame.Rescale();
        }
        public static void Draw()
        {
            ThreadAffinity.AssertMainThread();
            if (pendingRedrawGame && ThreadAffinity.IsMainThread)
            {
                pendingRedrawGame = false;
                finalGameFrame = game.Draw();
            }
            RenderTarget2D target = currentState.Draw();

            finalBatch.Begin();
            finalBatch.Draw(target, renderTargetPosition, Color.White);
            finalBatch.End();
        }
        public static States CurrentState => currentStateEnum;

        static void AssertUiOwnerThread()
        {
            if (UiThread.IsRunning)
            {
                ThreadAffinity.AssertUiThread();
                return;
            }

            ThreadAffinity.AssertMainThread();
        }

        internal static bool UiClick(ClickEvent aClick)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiClick(aClick);
                case States.PauseMenu:
                    return pauseMenu.UiClick(aClick);
                case States.OptionMenu:
                    return optionMenu.UiClick(aClick);
                case States.LoadingMenu:
                    return loadingMenu.UiClick(aClick);
                case States.NewGame:
                    return newGame.UiClick(aClick);
                case States.MoveHUD:
                    return moveHUD.UiClick(aClick);
                default:
                    return false;
            }
        }

        internal static bool UiRelease(ReleaseEvent aRelease)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiRelease(aRelease);
                case States.PauseMenu:
                    return pauseMenu.UiRelease(aRelease);
                case States.OptionMenu:
                    return optionMenu.UiRelease(aRelease);
                case States.LoadingMenu:
                    return loadingMenu.UiRelease(aRelease);
                case States.NewGame:
                    return newGame.UiRelease(aRelease);
                case States.MoveHUD:
                    return moveHUD.UiRelease(aRelease);
                default:
                    return false;
            }
        }

        internal static bool UiScroll(ScrollEvent aScroll)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiScroll(aScroll);
                case States.PauseMenu:
                    return pauseMenu.UiScroll(aScroll);
                case States.OptionMenu:
                    return optionMenu.UiScroll(aScroll);
                case States.LoadingMenu:
                    return loadingMenu.UiScroll(aScroll);
                case States.NewGame:
                    return newGame.UiScroll(aScroll);
                case States.MoveHUD:
                    return moveHUD.UiScroll(aScroll);
                default:
                    return false;
            }
        }

        internal static bool UiEscapePressed()
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiEscapePressed();
                case States.PauseMenu:
                    return pauseMenu.UiEscapePressed();
                case States.OptionMenu:
                    return optionMenu.UiEscapePressed();
                case States.LoadingMenu:
                    return loadingMenu.UiEscapePressed();
                case States.NewGame:
                    return newGame.UiEscapePressed();
                case States.MoveHUD:
                    return moveHUD.UiEscapePressed();
                default:
                    return false;
            }
        }

        internal static void UiInvalidate()
        {
            AssertUiOwnerThread();
            currentState?.MarkUiDirty();
        }

        internal static void UiUpdate()
        {
            AssertUiOwnerThread();
            long interactionVersionBefore = UIElement.InteractionVersion;
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    startScreen.UiUpdate();
                    break;
                case States.PauseMenu:
                    pauseMenu.UiUpdate();
                    break;
                case States.OptionMenu:
                    optionMenu.UiUpdate();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiUpdate();
                    break;
                case States.NewGame:
                    newGame.UiUpdate();
                    break;
                case States.MoveHUD:
                    moveHUD.UiUpdate();
                    break;
                default:
                    break;
            }

            if (currentStateEnum != States.Game
                && currentStateEnum != States.MoveHUD
                && UIElement.InteractionVersion != interactionVersionBefore)
            {
                currentState?.MarkUiDirty();
            }
        }

        static void HandleEscapePressed()
        {
            ThreadAffinity.AssertSimThread();
            switch (currentStateEnum)
            {
                case States.Game:
                    if (HasGroundTargetPendingSpell)
                    {
                        CancelGroundTargeting();
                        break;
                    }
                    RequestStateChange(States.PauseMenu);
                    break;
                case States.PauseMenu:
                    pauseMenu.HandleEscapePressed();
                    break;
                default:
                    break;
            }
        }

        internal static void UiOnLeave(States state)
        {
            AssertUiOwnerThread();
            switch (state)
            {
                case States.Game:
                    game.UiOnLeave();
                    break;
                case States.MoveHUD:
                    moveHUD.UiOnLeave();
                    break;
                case States.OptionMenu:
                    optionMenu.UiOnLeave();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiOnLeave();
                    break;
                case States.NewGame:
                    newGame.UiOnLeave();
                    break;
                default:
                    break;
            }
        }

        internal static void UiOnEnter(States state)
        {
            AssertUiOwnerThread();
            switch (state)
            {
                case States.MoveHUD:
                    moveHUD.UiOnEnter();
                    break;
                case States.OptionMenu:
                    optionMenu.UiOnEnter();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiOnEnter();
                    break;
                case States.NewGame:
                    newGame.UiOnEnter();
                    break;
                default:
                    break;
            }
        }

        internal static void UiRescale(Point windowSize)
        {
            AssertUiOwnerThread();

            HUDManager.Rescale();
            HUDManager.InvalidateUi();
            HUDManager.InvalidatePlates();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    startScreen.UiRescale();
                    break;
                case States.PauseMenu:
                    pauseMenu.UiRescale();
                    break;
                case States.OptionMenu:
                    optionMenu.UiRescale();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiRescale();
                    break;
                case States.NewGame:
                    newGame.UiRescale();
                    break;
                case States.MoveHUD:
                    moveHUD.UiRescale();
                    break;
                default:
                    break;
            }
        }

        static void HandleShopPurchaseRequested(ShopPurchaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;

            Items.Item item = ItemFactory.CreateItem(e.ItemId, e.Count);
            if (item == null) return;
            if (item.Cost > player.Gold) return;

            player.ChangeGold(-item.Cost);
            player.Inventory.AddItem(item);
        }

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

        static void HandleChatCommandRequested(ChatCommandRequested e)
        {
            ThreadAffinity.AssertSimThread();
            string raw = (e.CommandText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw)) return;

            if (raw[0] == '/')
            {
                raw = raw.Length == 1 ? string.Empty : raw.Substring(1);
            }

            if (string.IsNullOrWhiteSpace(raw)) return;

            if (!TryTokenizeCommand(raw, out string[] tokens, out string tokenError))
            {
                PublishChatSystemMessage(tokenError);
                return;
            }

            if (tokens.Length == 0) return;

            string command = tokens[0].ToLowerInvariant();
            string[] args = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, args, 0, args.Length);
            ChatCommandSpec? spec = TryGetChatCommandSpec(command);
            if (!spec.HasValue)
            {
                PublishChatSystemMessage($"Unknown command: /{command}. Use /help.");
                return;
            }

            if (spec.Value.Access == ChatCommandAccess.Debug && !DebugManager.Mode(DebugMode.ChatCheats))
            {
                return;
            }

            switch (command)
            {
                case "help":
                    HandleChatHelp();
                    break;
                case "clear":
                    Mailboxes.PublishUiEvent(new ChatCleared());
                    break;
                case "where":
                    HandleChatWhere(args);
                    break;
                case "chunklevels":
                    HandleChatChunkLevels(args);
                    break;
                case "tp":
                    HandleChatTeleport(args);
                    break;
                case "createitem":
                    HandleChatCreateItem(args);
                    break;
            }
        }

        static void HandleChatSayRequested(ChatSayRequested e)
        {
            ThreadAffinity.AssertSimThread();
            string text = (e.MessageText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            string senderName = ObjectManager.Player?.Name;
            if (string.IsNullOrWhiteSpace(senderName))
            {
                senderName = "Player";
            }

            Mailboxes.PublishUiEvent(new ChatMessagePosted(
                new ChatMessage(ChatMessageType.Say, text, senderName, ChatSpeakerType.Player)));
        }

        static void HandleChatHelp()
        {
            string systemCommands = string.Join(" | ", chatCommandSpecs
                .Where(x => x.Access == ChatCommandAccess.System)
                .OrderBy(x => x.Name)
                .Select(x => $"{x.Usage}: {x.Description}"));
            string debugCommands = string.Join(" | ", chatCommandSpecs
                .Where(x => x.Access == ChatCommandAccess.Debug)
                .OrderBy(x => x.Name)
                .Select(x => $"{x.Usage}: {x.Description}"));
            PublishChatSystemMessage($"System: {systemCommands}");
            PublishChatSystemMessage($"Debug (requires ChatCheats): {debugCommands}");
        }

        static void HandleChatWhere(string[] args)
        {
            if (args.Length > 1)
            {
                PublishChatSystemMessage("Usage: /where <friendly name>");
                return;
            }

            Friendly friendly;
            if (args.Length == 0)
            {
                friendly = ObjectManager.Player;
                if (friendly == null) return;
            }
            else
            {
                string name = args[0];
                if (!ObjectManager.TryGetFriendlyByName(name, out friendly))
                {
                    PublishChatSystemMessage($"Could not find non-mob named '{name}'.");
                    return;
                }
            }

            PublishChatSystemMessage($"{friendly.Name} is at {FormatCoordinate(friendly.FeetPosition.X)} {FormatCoordinate(friendly.FeetPosition.Y)}");
        }

        static void HandleChatTeleport(string[] args)
        {
            if (args.Length != 3)
            {
                PublishChatSystemMessage("Usage: /tp <friendly name> <x> <y>");
                return;
            }

            string name = args[0];
            if (!ObjectManager.TryGetFriendlyByName(name, out Friendly friendly))
            {
                PublishChatSystemMessage($"Could not find non-mob named '{name}'.");
                return;
            }

            if (!TryParseFloat(args[1], out float x) || !TryParseFloat(args[2], out float y))
            {
                PublishChatSystemMessage("Coordinates must be numbers. Usage: /tp <friendly name> <x> <y>");
                return;
            }

            friendly.Teleport(new WorldSpace(x, y));
            PublishChatSystemMessage($"{friendly.Name} teleported to {FormatCoordinate(x)} {FormatCoordinate(y)}");
        }

        static void HandleChatChunkLevels(string[] args)
        {
            if (args.Length != 0)
            {
                PublishChatSystemMessage("Usage: /chunklevels");
                return;
            }

            Chunk[] chunks = TileManager.GetChunksSnapshot();
            if (chunks.Length == 0)
            {
                PublishChatSystemMessage("No chunks are currently generated.");
                return;
            }

            Point center = ResolveChunkLevelsCenter();
            if (!HasCompleteChunkLevelsWindow(chunks, center))
            {
                PublishChunkLevelsAllGenerated(chunks);
                return;
            }

            PublishChunkLevelsWindow(chunks, center);
        }

        static void PublishChunkLevelsAllGenerated(Chunk[] chunks)
        {
            Chunk[] ordered = chunks
                .OrderBy(c => c.ChunkPosition.Y)
                .ThenBy(c => c.ChunkPosition.X)
                .ToArray();

            PublishChatSystemMessage($"Chunk levels for all generated chunks ({ordered.Length}):");

            const int entriesPerMessage = 6;
            for (int i = 0; i < ordered.Length; i += entriesPerMessage)
            {
                int take = Math.Min(entriesPerMessage, ordered.Length - i);
                string[] entries = new string[take];
                for (int j = 0; j < take; j++)
                {
                    Chunk chunk = ordered[i + j];
                    Point p = chunk.ChunkPosition;
                    entries[j] = $"({p.X},{p.Y})={chunk.AverageLevel:00}";
                }
                PublishChatSystemMessage(string.Join("  ", entries));
            }
        }

        static void PublishChunkLevelsWindow(Chunk[] chunks, Point center)
        {
            Dictionary<Point, int> levelsByChunk = new Dictionary<Point, int>(chunks.Length);
            for (int i = 0; i < chunks.Length; i++)
            {
                Chunk chunk = chunks[i];
                levelsByChunk[chunk.ChunkPosition] = chunk.AverageLevel;
            }

            int halfLow = ChunkLevelsWindowSize / 2;
            int halfHigh = ChunkLevelsWindowSize - halfLow - 1;
            int minX = center.X - halfLow;
            int maxX = center.X + halfHigh;
            int minY = center.Y - halfLow;
            int maxY = center.Y + halfHigh;

            PublishChatSystemMessage($"Chunk levels 10x10 near ({center.X},{center.Y}) [-- = not generated]:");

            string[] xLabels = new string[ChunkLevelsWindowSize];
            for (int x = minX; x <= maxX; x++)
            {
                xLabels[x - minX] = x.ToString("00;-00;00", CultureInfo.InvariantCulture);
            }
            PublishChatSystemMessage($"x: {string.Join(" ", xLabels)}");

            for (int y = maxY; y >= minY; y--)
            {
                string[] row = new string[ChunkLevelsWindowSize];
                for (int x = minX; x <= maxX; x++)
                {
                    Point key = new Point(x, y);
                    if (levelsByChunk.TryGetValue(key, out int level))
                    {
                        row[x - minX] = level.ToString("00", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row[x - minX] = "--";
                    }
                }

                PublishChatSystemMessage($"y {y.ToString("00;-00;00", CultureInfo.InvariantCulture)}: {string.Join(" ", row)}");
            }
        }

        static Point ResolveChunkLevelsCenter()
        {
            Chunk playerChunk = ObjectManager.Player != null
                ? TileManager.GetChunkUnder(ObjectManager.Player.FeetPosition)
                : null;
            return playerChunk != null ? playerChunk.ChunkPosition : Point.Zero;
        }

        static bool HasCompleteChunkLevelsWindow(Chunk[] chunks, Point center)
        {
            HashSet<Point> generated = new HashSet<Point>();
            for (int i = 0; i < chunks.Length; i++)
            {
                generated.Add(chunks[i].ChunkPosition);
            }

            int halfLow = ChunkLevelsWindowSize / 2;
            int halfHigh = ChunkLevelsWindowSize - halfLow - 1;
            int minX = center.X - halfLow;
            int maxX = center.X + halfHigh;
            int minY = center.Y - halfLow;
            int maxY = center.Y + halfHigh;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!generated.Contains(new Point(x, y))) return false;
                }
            }

            return true;
        }

        static void HandleChatCreateItem(string[] args)
        {
            if (args.Length != 3)
            {
                PublishChatSystemMessage("Usage: /createitem <friendly name> <item id> <count>");
                return;
            }

            string name = args[0];
            if (!ObjectManager.TryGetFriendlyByName(name, out Friendly friendly))
            {
                PublishChatSystemMessage($"Could not find non-mob named '{name}'.");
                return;
            }

            if (friendly is not Player player)
            {
                PublishChatSystemMessage($"{friendly.Name} cannot receive items (no inventory).");
                return;
            }

            if (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemId) || itemId < 0)
            {
                PublishChatSystemMessage("Item id must be a non-negative integer.");
                return;
            }

            if (!int.TryParse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count <= 0)
            {
                PublishChatSystemMessage("Item count must be greater than 0.");
                return;
            }

            if (!TryGetItemData(itemId, out ItemData itemData))
            {
                PublishChatSystemMessage($"Item id {itemId} does not exist.");
                return;
            }

            Items.Item item = ItemFactory.CreateItem(itemData, count);
            bool addedAll = player.Inventory.AddItem(item);
            if (addedAll)
            {
                PublishChatSystemMessage($"Gave {player.Name} {count}x {item.Name}");
                return;
            }

            int leftover = item.Count;
            int added = count - leftover;
            if (added <= 0)
            {
                PublishChatSystemMessage($"{player.Name} has no room for {item.Name}.");
                return;
            }

            PublishChatSystemMessage($"Gave {player.Name} {added}x {item.Name} ({leftover}x did not fit)");
        }

        static bool TryGetItemData(int itemId, out ItemData itemData)
        {
            itemData = null;
            ItemData[] all = ItemFactory.GetAllItemDataSnapshot();
            if (all.Length == 0) return false;

            if (itemId >= 0 && itemId < all.Length)
            {
                ItemData indexed = all[itemId];
                if (indexed != null && indexed.ID == itemId)
                {
                    itemData = indexed;
                    return true;
                }
            }

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null || all[i].ID != itemId) continue;
                itemData = all[i];
                return true;
            }

            return false;
        }

        static bool TryTokenizeCommand(string text, out string[] tokens, out string error)
        {
            error = null;
            List<string> parsed = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\\' && i + 1 < text.Length && (text[i + 1] == '"' || text[i + 1] == '\\'))
                {
                    current.Append(text[i + 1]);
                    i++;
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        parsed.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }

                current.Append(c);
            }

            if (inQuotes)
            {
                tokens = Array.Empty<string>();
                error = "Unclosed quote in command.";
                return false;
            }

            if (current.Length > 0)
            {
                parsed.Add(current.ToString());
            }

            tokens = parsed.ToArray();
            return true;
        }

        static bool TryParseFloat(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
        }

        static ChatCommandSpec? TryGetChatCommandSpec(string commandName)
        {
            for (int i = 0; i < chatCommandSpecs.Length; i++)
            {
                if (!string.Equals(chatCommandSpecs[i].Name, commandName, StringComparison.OrdinalIgnoreCase)) continue;
                return chatCommandSpecs[i];
            }

            return null;
        }

        static string FormatCoordinate(float value)
        {
            float rounded = MathF.Round(value);
            if (MathF.Abs(value - rounded) < 0.0001f)
            {
                return ((int)rounded).ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        static void PublishChatSystemMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            Mailboxes.PublishUiEvent(new ChatMessagePosted(
                new ChatMessage(ChatMessageType.System, message, "System", ChatSpeakerType.System)));
        }

        readonly struct ChatCommandSpec
        {
            public ChatCommandSpec(string name, string usage, string description, ChatCommandAccess access)
            {
                Name = name;
                Usage = usage;
                Description = description;
                Access = access;
            }

            public string Name { get; }
            public string Usage { get; }
            public string Description { get; }
            public ChatCommandAccess Access { get; }
        }

        enum ChatCommandAccess
        {
            System,
            Debug
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

        static void HandleLogicWindowSnapshotRequested(LogicWindowSnapshotRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId, out GuildMember member))
            {
                Mailboxes.PublishUiEvent(new LogicWindowSnapshotSet(e.MemberRenderId, Array.Empty<LogicNodeUiSnapshot>()));
                return;
            }

            LogicNode root = member.AttackLogic?.RootNode;
            Mailboxes.PublishUiEvent(new LogicWindowSnapshotSet(e.MemberRenderId, BuildLogicNodeSnapshot(root)));
        }

        static LogicNodeUiSnapshot[] BuildLogicNodeSnapshot(LogicNode root)
        {
            if (root == null) return Array.Empty<LogicNodeUiSnapshot>();

            Dictionary<LogicNode, int> ids = new Dictionary<LogicNode, int>();
            List<LogicNodeUiSnapshot> snapshots = new List<LogicNodeUiSnapshot>();
            Queue<(LogicNode node, int parentId, int depth)> queue = new Queue<(LogicNode node, int parentId, int depth)>();
            queue.Enqueue((root, -1, 0));

            while (queue.Count > 0)
            {
                (LogicNode node, int parentId, int depth) = queue.Dequeue();
                if (node == null) continue;

                if (!ids.TryGetValue(node, out int nodeId))
                {
                    nodeId = ids.Count;
                    ids[node] = nodeId;
                    string resultName = GetLogicNodeName(node);
                    snapshots.Add(new LogicNodeUiSnapshot(nodeId, parentId, depth, resultName));

                    IReadOnlyList<LogicNode> children = node.Children;
                    for (int i = 0; i < children.Count; i++)
                    {
                        queue.Enqueue((children[i], nodeId, depth + 1));
                    }
                    continue;
                }

                // If the tree reuses node instances, keep an explicit edge for this parent.
                if (parentId >= 0)
                {
                    snapshots.Add(new LogicNodeUiSnapshot(nodeId, parentId, depth, GetLogicNodeName(node)));
                }
            }

            return snapshots.ToArray();
        }

        static string GetLogicNodeName(LogicNode node)
        {
            ILogicResult raw = node.RawResult;
            if (raw == null || raw is Continue) return "Branch";
            string name = raw.GetType().Name;
            const string suffix = "Result";
            if (name.EndsWith(suffix))
            {
                name = name.Substring(0, name.Length - suffix.Length);
            }
            return name;
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

        static void HandleResetToMainMenuRequested()
        {
            ThreadAffinity.AssertSimThread();
            SetState(States.StartScreen);
            ObjectManager.Reset();
        }

        static void HandleCreateNewPlayerRequested(CreateNewPlayerRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (string.IsNullOrWhiteSpace(e.Name) || string.IsNullOrWhiteSpace(e.ClassName)) return;
            ObjectManager.CreateNewPlayer(e.Name, e.ClassName);
            SaveManager.CreateNewSave(e.Name);
            SetState(States.Game);
        }

        static void HandleSaveDataRequested()
        {
            ThreadAffinity.AssertSimThread();
            SaveManager.SaveData();
        }

        static void HandleLoadSaveRequested(LoadSaveRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (string.IsNullOrWhiteSpace(e.SaveName))
            {
                if (!SaveManager.RequestLoadData(SaveManager.CurrentSave)) return;
                SetState(States.LoadingMenu);
                return;
            }

            if (!SaveManager.TryGetSaveByName(e.SaveName, out Save save)) return;
            if (!SaveManager.RequestLoadData(save)) return;
            SetState(States.LoadingMenu);
        }

        static void HandleContinueLastSaveRequested()
        {
            ThreadAffinity.AssertSimThread();
            if (!SaveManager.RequestContinueLastSave()) return;
            SetState(States.LoadingMenu);
        }

        static void HandleNewGameRequested()
        {
            ThreadAffinity.AssertSimThread();
            TileManager.New();
            SetState(States.NewGame);
        }

        static void HandleSaveLoadParsed(SaveLoadParsed e)
        {
            ThreadAffinity.AssertSimThread();
            if (e.Payload == null) return;
            if (!SaveManager.ApplyLoadPayload(e.Payload, e.RequestId)) return;
            SetState(States.Game);
            RedrawGame();
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

            if (DoodadManager.TryGetDoodadAt(worldPos, out Doodad doodad))
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

            if (DoodadManager.TryGetDoodadByRenderId(e.TargetRenderId, out Doodad doodad))
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

        static void HandleInventorySwapItemsRequested(InventorySwapItemsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapItems(e.From, e.To);
        }

        static void HandleInventorySwapEquipmentRequested(InventorySwapEquipmentRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.SwapEquipment(e.From, e.EquipmentSlot, target);
        }

        static void HandleInventoryEquipBagRequested(InventoryEquipBagRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.EquipBag(e.From);
        }

        static void HandleInventoryUnequipBagRequested(InventoryUnequipBagRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (e.ToSlot.HasValue)
            {
                player.Inventory.UnequipBag(e.BagSlot, e.ToSlot.Value);
                return;
            }
            player.Inventory.UnequipBag(e.BagSlot);
        }

        static void HandleInventorySwapBagsRequested(InventorySwapBagsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapBags(e.From, e.BagSlot);
        }

        static void HandleInventorySwapBagSlotsRequested(InventorySwapBagSlotsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapPlacesOfBags(e.FromBagSlot, e.ToBagSlot);
        }

        static void HandleLootItemRequested(LootItemRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (e.ToSlot.HasValue)
            {
                player.Inventory.LootItem(e.LootSlotIndex, e.ToSlot.Value);
                return;
            }
            player.Inventory.LootItem(e.LootSlotIndex);
        }

        static void HandleInventoryEquipRequested(InventoryEquipRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.Equip(e.Index, target);
        }

        static void HandleInventoryConsumeRequested(InventoryConsumeRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.ConsumeItem(e.Index, target);
        }

        static void HandleEquipmentSwapRequested(EquipmentSwapRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);

            Equipment fromEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.FromSlot) as Equipment;
            if (fromEquip == null) return;
            if (!GameObjects.Unit.Equipment.FitsInSlot(fromEquip.type, (GameObjects.Unit.Equipment.Slot)e.ToSlot)) return;

            Equipment toEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.ToSlot) as Equipment;
            if (toEquip == null)
            {
                target.EquipInParticularSlot(fromEquip, (GameObjects.Unit.Equipment.Slot)e.ToSlot);
                target.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)e.FromSlot);
                return;
            }

            if (fromEquip.type != toEquip.type) return;
            if (fromEquip.type >= Equipment.Type.MainHander) return;
            if (toEquip.type >= Equipment.Type.MainHander) return;

            target.EquipInParticularSlot(fromEquip, (GameObjects.Unit.Equipment.Slot)e.ToSlot);
            target.EquipInParticularSlot(toEquip, (GameObjects.Unit.Equipment.Slot)e.FromSlot);
        }

        static void HandleEquipmentMoveToInventoryRequested(EquipmentMoveToInventoryRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);

            Equipment fromEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.EquipmentSlot) as Equipment;
            if (fromEquip == null) return;

            Item destItem = player.Inventory.GetItemInSlot(e.InventorySlot);
            if (destItem == null)
            {
                player.Inventory.AddItem(fromEquip, e.InventorySlot);
                target.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot);
                return;
            }

            Equipment destEquip = destItem as Equipment;
            if (destEquip == null) return;
            if (!GameObjects.Unit.Equipment.FitsInSlot(destEquip.type, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot)) return;

            player.Inventory.AssignItem(fromEquip, e.InventorySlot);
            target.EquipInParticularSlot(destEquip, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot);
        }

        static Friendly ResolveFriendlyTarget(int? targetRenderId, Player fallback)
        {
            ThreadAffinity.AssertSimThread();
            if (!targetRenderId.HasValue) return fallback;
            if (ObjectManager.TryGetFriendlyByRenderId(targetRenderId.Value, out Friendly target))
            {
                return target;
            }
            return fallback;
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

        static void HandleUiStateChanged(StateChanged e)
        {
            AssertUiOwnerThread();
            UiOnLeave(e.Previous.ToStateManagerState());
            UiOnEnter(e.Current.ToStateManagerState());
        }
    }
}
