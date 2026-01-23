using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_1.GameObjects;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.AoE;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Particles;
using Project_1.Messaging;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.CharacterCreator;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using System.Runtime.InteropServices;

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
                Mailboxes.Main.Publish(new Messaging.Events.ContinueLastSaveRequested());
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
            TimeManager.Update(gameTime);

            GraphicsManager.Update();
            InputManager.Update();
            if (UiThread.IsRunning)
            {
                UiThread.PulseAndWait(false);
            }
            else
            {
                lock (HUDManager.UiLock)
                {
                    Mailboxes.Ui.DispatchAll();
                }
            }
            if (SimThread.IsRunning)
            {
                SimThread.PulseAndWait();
            }
            else
            {
                Mailboxes.Main.DispatchAll();
                StateManager.Update();
                Mailboxes.Main.DispatchAll();
                DebugManager.Update();
            }
            if (UiThread.IsRunning)
            {
                UiThread.PulseAndWait(true);
            }
            else
            {
                lock (HUDManager.UiLock)
                {
                    Mailboxes.Ui.DispatchAll();
                    UiTextInputManager.Update();
                    StateManager.UiUpdate();
                    HUDManager.Update();
                    HUDManager.BuildDrawLists();
                }
            }
            SaveManager.ProcessPendingScreenshots();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ThreadAffinity.AssertMainThread();
            GraphicsDevice.Clear(Color.HotPink);

            EffectManager.EffectDraw();
            StateManager.Draw();
            

            base.Draw(gameTime);
        }

        static void InitializeMainThreadSystems()
        {
            ThreadAffinity.InitMainThread();
            ThreadAffinity.AssertMainThread();
            Mailboxes.InitMainThread();
            GraphicsManager.Init();
            DebugManager.Init();
            ThreadingSettings.Init();
            SaveManager.Init();
            RandomManager.Init();
            TimeManager.Init();
            Camera.Camera.Init();
            KeyBindManager.Init();
            TileManager.Init();
            TileFactory.Init();
            TextureManager.Init();
            EffectManager.Init();
            ObjectFactory.Init();
            ItemFactory.Init();
            LootFactory.Init();
            SpellFactory.Init();
            ProjectileFactory.Init();
            OptionManager.Init();
            StateManager.Init();
            HUDManager.Init();
            ObjectManager.Init();
            SpawnerManager.Init();
            DoodadManager.Init();
            FloatingTextManager.Init();
            CorpseManager.Init();
            ProjectileManager.Init();
            ParticleManager.Init();
            UiInputBridge.Init();
            if (ThreadingSettings.UseWorkerThreads) WorkerPool.Start();
            if (ThreadingSettings.UseUiThread) UiThread.Start();
            if (ThreadingSettings.UseSimThread) SimThread.Start();
        }
    }
}
