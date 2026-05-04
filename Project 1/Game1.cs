using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Particles;
using Project_1.Messaging;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.World.GameObjects.Unit.Talents;
using Project_1.UI;
using Project_1.UI.CharacterCreator;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Project_1
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static Microsoft.Xna.Framework.Game Instance { get; private set; }
        public static ContentManager ContentManager { get; private set; }
        public Game1()
        {
            GraphicsManager.SetManager(this);
            Content.RootDirectory = "Content";
            ContentManager = Content;
            IsMouseVisible = true;
            Instance = this;
            InitializeMainThreadSystems();
        }

        protected override void Initialize()
        {

            //DEBUG

            

            if (DebugManager.Mode(DebugMode.InstantlyContinue))
            {
                MailboxManager.PublishSimCommand(new Messaging.Events.ContinueLastSaveRequested());
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            DebugManager.LoadContent();
            //GraphicsManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            ThreadAffinity.AssertMainThread();
            TimeManager.Update(gameTime);

            GraphicsManager.Update();
            InputManager.Update();
            if (UiThread.IsRunning)
            {
                UiThread.PulseAndWait(true);
            }
            else if (!SimThread.IsRunning)
            {
                // Single-thread mode: process UI input before sim update.
                MailboxManager.Ui.DispatchAll();
            }
            if (!SimThread.IsRunning)
            {
                MailboxManager.Main.DispatchAll();
                MailboxManager.Sim.DispatchAll();
                StateManager.Update();
                MailboxManager.Main.DispatchAll();
                MailboxManager.Sim.DispatchAll();
                DebugManager.Update();
            }
            if (!UiThread.IsRunning)
            {
                if (SimThread.IsRunning)
                {
                    lock (HUDManager.UiLock)
                    {
                        MailboxManager.Ui.DispatchAll();
                        UiTextInputManager.Update();
                        StateManager.UiUpdate();
                        HUDManager.Update();
                        HUDManager.BuildDrawLists();
                    }
                }
                else
                {
                    MailboxManager.Ui.DispatchAll();
                    UiTextInputManager.Update();
                    StateManager.UiUpdate();
                    HUDManager.Update();
                    HUDManager.BuildDrawLists();
                }
            }
            if (StateManager.CurrentState != StateManager.States.Game)
            {
                ParticleManager.Update();
                FloatingTextManager.Update();
            }
            SaveManager.ProcessPendingScreenshots();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ThreadAffinity.AssertMainThread();
            Camera.Camera.BeginMainThreadRenderFrame();
            try
            {
                GraphicsDevice.Clear(Color.HotPink);

                EffectManager.EffectDraw();
                StateManager.Draw();

                base.Draw(gameTime);
            }
            finally
            {
                Camera.Camera.EndMainThreadRenderFrame();
            }
        }

        protected override void EndRun()
        {
            ThreadAffinity.AssertMainThread();

            // Deterministic thread shutdown order:
            // 1) stop UI producer, 2) stop worker jobs, 3) stop sim loop

            //TODO: Make sure saving happens properly before shutting down threads
            UiThread.Stop();
            WorkerPool.Stop();
            SimThread.Stop();
            DebugManager.Shutdown();

            base.EndRun();
        }

        static void InitializeMainThreadSystems()
        {
            ThreadAffinity.InitMainThread();
            ThreadAffinity.AssertMainThread();
            MailboxManager.InitMainThread();
            GraphicsManager.Init();
            SaveManager.Init();
            DebugManager.Init();
            ThreadingSettings.Init();
            RandomManager.Init();
            TimeManager.Init();
            Camera.Camera.Init();
            KeyBindManager.Init();
            TileManager.Init();
            TileRenderCache.Init();
            TextureManager.Init();
            TileFactory.Init();
            EffectManager.Init();
            TalentFactory.Init();
            ObjectFactory.Init();
            ItemFactory.Init();
            InventoryCommandRouter.Init();
            ShopCommandRouter.Init();
            LootFactory.Init();
            SpellFactory.Init();
            SpellCastRouter.Init();
            SpellTrainingRouter.Init();
            ProjectileFactory.Init();
            OptionManager.Init();
            ChatCommandRouter.Init();
            SessionFlowRouter.Init();
            StateManager.Init();
            WorldInteractionRouter.Init();
            LogicWindowSnapshotRouter.Init();
            TalentWindowSnapshotRouter.Init();
            HUDManager.Init();
            ObjectManager.Init();
            SpawnerManager.Init();
            FloatingTextManager.Init();
            CorpseManager.Init();
            ProjectileManager.Init();
            ParticleManager.Init();
            UiInputBridge.Init();
            if (ThreadingSettings.UseWorkerThreads) WorkerPool.Start();
            if (ThreadingSettings.UseUiThread) UiThread.Start();
            if (ThreadingSettings.UseSimThread) SimThread.Start();
        }

        EventHandler<TextInputEventArgs> currentTextInputEvent;

        public void RegisterToTextInput(EventHandler<TextInputEventArgs> e)
        {
            UnregisterFromTextInput();

            currentTextInputEvent = e;
            Window.TextInput += e;
        }

        public void UnregisterFromTextInput()
        {
            if (currentTextInputEvent != null)
            {
                Window.TextInput -= currentTextInputEvent;
                currentTextInputEvent = null;
            }
        }
    }
}
